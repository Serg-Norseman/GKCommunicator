﻿/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using BencodeNET;
using BSLib;
using GKNet.Blockchain;
using GKNet.Database;
using GKNet.DHT;
using GKNet.Logging;
using GKNet.STUN;
using GKNet.TCP;
using LumiSoft.Net.STUN.Client;
using Mono.Nat;

namespace GKNet
{
    public sealed class CommunicatorCore : BaseObject, ICommunicatorCore
    {
        public const string APP_NAME = "GKCommunicator";
        public const string APP_DESC = "Distributed, decentralized, serverless, peer-to-peer communication plugin for GEDKeeper.";
        public const string APP_COPYRIGHT = "Copyright © 2018-2023 by Sergey V. Zhdanovskih";
        public const string APP_VERSION = "0.30.0.0";

        private static readonly DHTId GKNInfoHash = ProtocolHelper.CreateSignInfoKey();

        private static readonly TimeSpan PingInterval = TimeSpan.FromMinutes(1);

        private readonly IBlockchainNode fBlockchainNode;
        private ConnectionState fConnectionState;
        private readonly GKNetDatabase fDatabase;
        private List<IDataPlugin> fDataPlugins;
        private readonly DHTClient fDHTClient;
        private readonly IChatForm fForm;
        private Peer fLocalPeer;
        private readonly GKNet.Logging.ILogger fLogger;
        private string fPassword; // TODO: remove this field, combine password hashing methods to store passwordHash here
        private readonly IList<Peer> fPeers;
        private readonly UserProfile fProfile;
        private IPEndPoint fPublicEndPoint;
        private bool fShowConnectionInfo;
        private STUN_Result fSTUNInfo;
        private readonly TCPDuplexClient fTCPClient;
        private int fTCPListenerPort;
        private Semaphore fUPnPSem = new Semaphore(0, 1);


        public IBlockchainNode BlockchainNode
        {
            get {
                return fBlockchainNode;
            }
        }

        public DHTId ClientNodeId
        {
            get { return fProfile.NodeId; }
        }

        public ConnectionState ConnectionState
        {
            get { return fConnectionState; }
        }

        public GKNetDatabase Database
        {
            get { return fDatabase; }
        }

        public List<IDataPlugin> DataPlugins
        {
            get { return fDataPlugins; }
        }

        public DHTClient DHTClient
        {
            get { return fDHTClient; }
        }

        public Peer LocalPeer
        {
            get { return fLocalPeer; }
        }

        public IList<Peer> Peers
        {
            get { return fPeers; }
        }

        public int Port
        {
            get; set;
        }

        public UserProfile Profile
        {
            get { return fProfile; }
        }

        public bool ShowConnectionInfo
        {
            get { return fShowConnectionInfo; }
            set {
                fShowConnectionInfo = value;
                fDatabase.SetParameterBool("show_connection_info", value);
            }
        }

        public STUN_Result STUNInfo
        {
            get { return fSTUNInfo; }
        }

        public IPEndPoint PublicEndPoint
        {
            get { return fPublicEndPoint; }
            set { fPublicEndPoint = value; }
        }

        public int TCPListenerPort
        {
            get { return fTCPListenerPort; }
            set { fTCPListenerPort = value; }
        }

        public bool UPnPEnabled
        {
            get; set;
        }


        public CommunicatorCore(IChatForm form)
        {
            if (form == null) {
                throw new ArgumentNullException("form");
            }

            fConnectionState = ConnectionState.Disconnected;
            fForm = form;
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "CommCore");
            fPeers = new List<Peer>();

            fProfile = new UserProfile();

            fDatabase = new GKNetDatabase();
            fDatabase.Connect();
            fDatabase.LoadProfile(fProfile);

            fBlockchainNode = new BlockchainNode(this, fDatabase);

            fDataPlugins = new List<IDataPlugin>();
            LoadPlugins(Utilities.GetAppPath());

            Port = DHTClient.PublicDHTPort;
            fLogger.WriteInfo("Port: {0}", Port);

            try {
                fSTUNInfo = STUNUtility.Detect(Port);
                fPublicEndPoint = (fSTUNInfo.NetType != STUN_NetType.UdpBlocked) ? fSTUNInfo.PublicEndPoint : null;
            } catch (Exception ex) {
                fLogger.WriteError("DetectSTUN() error", ex);
                fPublicEndPoint = null;
            }

            if (UPnPEnabled) {
                CreatePortMapping();
            }

            fDHTClient = new DHTClient(new IPEndPoint(DHTClient.IPAnyAddress, Port), this, ProtocolHelper.CLIENT_VER);
            fDHTClient.PublicEndPoint = fPublicEndPoint;
            fDHTClient.PeersFound += OnPeersFound;
            fDHTClient.PeerPinged += OnPeerPinged;
            fDHTClient.QueryReceived += OnQueryReceive;
            fDHTClient.ResponseReceived += OnResponseReceive;

            InitializePeers();

            fTCPClient = new TCPDuplexClient();
            fTCPClient.DataReceive += OnDataReceive;

            fTCPListenerPort = ProtocolHelper.PublicTCPPort;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                fDHTClient.PeersFound -= OnPeersFound;
                fDHTClient.PeerPinged -= OnPeerPinged;
                fDHTClient.QueryReceived -= OnQueryReceive;
                fDHTClient.ResponseReceived -= OnResponseReceive;
                fTCPClient.DataReceive -= OnDataReceive;

                fDatabase.Disconnect();
            }
            base.Dispose(disposing);
        }

        public void CreatePortMapping()
        {
            NatUtility.DeviceFound += DeviceFound;

            fLogger.WriteInfo("NAT Discovery started");
            NatUtility.StartDiscovery();

            fUPnPSem.WaitOne();

            fLogger.WriteInfo("NAT Discovery stopped");
            NatUtility.StopDiscovery();
        }

        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            try {
                INatDevice device = args.Device;

                fLogger.WriteInfo("Device found, type: {0}", device.GetType().Name);
                if (device.GetType().Name == "PmpNatDevice") {
                    fLogger.WriteInfo("Device skipped");
                    return;
                }

                fLogger.WriteInfo("External IP: {0}", device.GetExternalIP());

                try {
                    Mapping m;
                    /*Mapping m = device.GetSpecificMapping(Mono.Nat.Protocol.Tcp, ProtocolHelper.PublicTCPPort);
                    if (m != null) {
                        fLogger.WriteInfo("Specific Mapping: protocol={0}, public={1}, private={2}", m.Protocol, m.PublicPort, m.PrivatePort);
                    } else {*/
                    /*m = new Mapping(Protocol.Tcp, ProtocolHelper.PublicTCPPort, ProtocolHelper.PublicTCPPort);
                    device.CreatePortMap(m);
                    fLogger.WriteInfo("Create Mapping: protocol={0}, public={1}, private={2}", m.Protocol, m.PublicPort, m.PrivatePort);*/
                    //}

                    m = device.GetSpecificMapping(Protocol.Udp, DHTClient.PublicDHTPort);
                    if (m != null) {
                        try {
                            device.DeletePortMap(m);
                        } catch {
                        }
                    }
                    m = new Mapping(Protocol.Udp, DHTClient.PublicDHTPort, DHTClient.PublicDHTPort);
                    device.CreatePortMap(m);
                    fLogger.WriteInfo("Create Mapping: protocol={0}, public={1}, private={2}", m.Protocol, m.PublicPort, m.PrivatePort);
                } catch (Exception ex) {
                    fLogger.WriteError("Couldn't create specific mapping", ex);
                }

                foreach (Mapping mp in device.GetAllMappings()) {
                    fLogger.WriteInfo("Existing Mapping: protocol={0}, public={1}, private={2}", mp.Protocol, mp.PublicPort, mp.PrivatePort);
                }

                fUPnPSem.Release();
            } catch (Exception ex) {
                fLogger.WriteError("NATMapper.DeviceFound()", ex);
            }
        }

        public void Connect()
        {
            var dhtNodes = fDatabase.LoadNodes();
            fDHTClient.RoutingTable.UpdateNodes(dhtNodes);

            fDHTClient.Start(GKNInfoHash);

            fTCPClient.Connect(fTCPListenerPort);

            fConnectionState = ConnectionState.Connection;
            new Thread(() => {
                while (fConnectionState != ConnectionState.Disconnected) {
                    CheckPeers();
                    Thread.Sleep(60000);
                }
            }).Start();
        }

        public void Disconnect()
        {
            fConnectionState = ConnectionState.Disconnected;
            fTCPClient.Disconnect();
            fDHTClient.Stop();

            Thread.Sleep(2000);
        }

        public void Identify(UserProfile userProfile, string password)
        {
            userProfile.Identify(password);
            fPassword = password;
        }

        public bool Authenticate(string password)
        {
            bool result = fProfile.Authenticate(password);
            fPassword = result ? password : string.Empty;
            return result;
        }

        private void InitializePeers()
        {
            fShowConnectionInfo = fDatabase.GetParameterBool("show_connection_info");

            fLocalPeer = AddPeer(fPublicEndPoint, fProfile);
            fLocalPeer.State = PeerState.Unknown;
            fLocalPeer.Presence = fDatabase.LoadPresence();

            var dbPeers = fDatabase.LoadPeers();
            foreach (var rp in dbPeers) {
                var remoteProfile = new PeerProfile();
                remoteProfile.NodeId = DHTId.Parse(rp.node_id);
                remoteProfile.UserName = rp.user_name;
                remoteProfile.Country = rp.country;
                remoteProfile.Languages = rp.langs;
                remoteProfile.TimeZone = rp.timezone;
                remoteProfile.Email = rp.email;
                remoteProfile.PublicKey = rp.public_key;

                var remotePeer = AddPeer(Utilities.ParseIPEndPoint(rp.last_endpoint), remoteProfile);
                remotePeer.IsLocal = false;
                remotePeer.State = PeerState.Unknown;
                remotePeer.Presence = PresenceStatus.Offline;
            }
        }

        // TODO: implement periodic clearing of the cache of nodes from outdated information
        public void SaveNode(DHTNode node)
        {
            fDatabase.SaveNode(node);
        }

        /// <summary>
        /// Peer confirmation after receiving a ping from it.
        /// </summary>
        /// <param name="peerEndPoint"></param>
        /// <returns></returns>
        public bool CheckPeer(IPEndPoint peerEndPoint)
        {
            bool result = false;
            if (peerEndPoint == null) return false;

            Peer peer = FindPeer(peerEndPoint);
            if (peer == null) {
                // ping can come from various sources,
                // if a node with such an endpoint is not found - then the source is not a peer
                return false;
            }

            peer.PingTries = 0;

            if (peer.State != PeerState.Checked && !peer.IsLocal) {
                peer.State = PeerState.Checked;
                SendData(peer.EndPoint, ProtocolHelper.CreateGetPeerInfoQuery(DHTTransactions.GetNextId(), fDHTClient.LocalID));
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Peer update after receiving it from DHT nodes.
        /// </summary>
        /// <param name="peerEndPoint"></param>
        /// <returns></returns>
        public bool UpdatePeer(IPEndPoint peerEndPoint)
        {
            bool result = false;
            if (peerEndPoint == null) return false;

            Peer peer;
            if (CheckLocalAddress(peerEndPoint.Address)) {
                peer = fLocalPeer;
                if (peer.State < PeerState.Unchecked && peer.EndPoint.Port != peerEndPoint.Port) {
                    peer.State = PeerState.Unchecked;
                    result = true;
                } else if (peer.State < PeerState.Checked && peer.EndPoint.Port == peerEndPoint.Port) {
                    peer.State = PeerState.Checked;
                    result = true;
                }
            } else {
                peer = FindPeer(peerEndPoint);
                if (peer == null) {
                    peer = AddPeer(peerEndPoint);
                    result = true;
                }
            }

            peer.LastUpdateTime = DateTime.UtcNow;

            if (!peer.IsLocal) {
                SendPing(peer, true);
            }

            return result;
        }

        /// <summary>
        /// Many thanks to Konstantinos Gkinis <aka raidenfreeman> for project https://github.com/raidenfreeman/ICE-Experiment,
        /// who explained in a simple way how to implement UDP Hole punching.
        /// </summary>
        private void SendPing(Peer peer, bool holePunch)
        {
            DateTime dtNow = DateTime.UtcNow;
            if (dtNow - peer.LastPingTime > PingInterval) {
                if (!holePunch) {
                    fDHTClient.SendPingQuery(peer.EndPoint, true);
                } else {
                    for (int i = 0; i < 10; i++) {
                        fDHTClient.SendPingQuery(peer.EndPoint, false);
                    }
                }

                peer.PingTries += 1;
                peer.LastPingTime = dtNow;
            }
        }

        public bool CheckLocalAddress(IPAddress peerAddress)
        {
            return ((fPublicEndPoint != null) && (Utilities.PrepareAddress(fPublicEndPoint.Address).Equals(peerAddress)));
        }

        private Peer AddPeer(IPEndPoint peerEndPoint, PeerProfile profile = null)
        {
            lock (fPeers) {
                Peer peer = new Peer(peerEndPoint, profile);
                peer.IsLocal = CheckLocalAddress(peerEndPoint.Address);

                if (!peer.IsLocal) {
                    fLogger.WriteInfo("Found new peer: {0}", peerEndPoint);
                }

                fPeers.Add(peer);
                return peer;
            }
        }

        private Peer AddPeer(string nodeId)
        {
            lock (fPeers) {
                Peer peer = new Peer(new IPEndPoint(DHTClient.IPAnyAddress, 0), new PeerProfile());
                peer.ID = DHTId.Parse(nodeId);
                fPeers.Add(peer);
                return peer;
            }
        }

        public Peer FindPeer(IPEndPoint peerEndPoint)
        {
            return fPeers.FirstOrDefault(x => x.EndPoint.Equals(peerEndPoint));
        }

        public Peer FindPeer(string id)
        {
            return fPeers.FirstOrDefault(x => x.ID.ToString().Equals(id));
        }

        #region Protocol features

        private void SendData(IPEndPoint endPoint, BDictionary data)
        {
            fDHTClient.Send(endPoint, data);

            //fTCPClient.Send(endPoint, data.EncodeAsBytes());
        }

        public Message SendMessage(Peer peer, string message)
        {
            if (peer == null || peer.ID == null || string.IsNullOrEmpty(message))
                return null;

            fLogger.WriteDebug("SendMessage: {0}, `{1}`", peer.EndPoint, message);

            var msg = new Message(DateTime.UtcNow, message, fLocalPeer.ID.ToString(), peer.ID.ToString());
            fDatabase.SaveMessage(msg);

            bool encrypted = false;
            if (peer.Profile != null && !string.IsNullOrEmpty(peer.Profile.PublicKey)) {
                message = Utilities.Encrypt(message, peer.Profile.PublicKey);
                encrypted = true;
            }

            SendData(peer.EndPoint, ProtocolHelper.CreateChatMessage(DHTTransactions.GetNextId(), fDHTClient.LocalID, message, encrypted, msg.Timestamp.ToBinary()));

            return msg;
        }

        #endregion

        private void CheckPeers()
        {
            DateTime dtNow = DateTime.UtcNow;
            bool changed = false;
            bool hasCheckedPeers = false;

            for (int i = fPeers.Count - 1; i >= 0; i--) {
                var peer = fPeers[i];
                if (peer.IsLocal) continue;

                if (peer.IsUnknown && (peer.PingTries >= 10 || dtNow - peer.LastUpdateTime > TimeSpan.FromMinutes(10))) {
                    fPeers.Remove(peer);
                    changed = true;
                    continue;
                }

                // always send ping
                SendPing(peer, (peer.State < PeerState.Checked));

                if (peer.State >= PeerState.Checked) {
                    hasCheckedPeers = true;
                }

                if (peer.State == PeerState.Unknown) {
                    fDHTClient.FindUnkPeer(peer);
                }

                SendData(peer.EndPoint, ProtocolHelper.CreateHandshakeQuery(DHTTransactions.GetNextId(), fDHTClient.LocalID));
            }

            if (hasCheckedPeers) {
                if (fConnectionState == ConnectionState.Connection) {
                    fConnectionState = ConnectionState.Connected;
                    changed = true;
                }
            } else {
                if (fConnectionState == ConnectionState.Connected) {
                    fConnectionState = ConnectionState.Connection;
                    changed = true;
                }
            }

            if (changed) {
                fForm.OnPeersListChanged();
            }
        }

        private void OnPeersFound(object sender, PeersFoundEventArgs e)
        {
            bool changed = false;
            foreach (var p in e.Peers) {
                if (UpdatePeer(p)) {
                    changed = true;
                }
            }

            if (changed) {
                fForm.OnPeersListChanged();
            }
        }

        private void OnPeerPinged(object sender, PeerPingedEventArgs e)
        {
            bool changed = CheckPeer(e.EndPoint);

            if (changed) {
                fForm.OnPeersListChanged();
            }
        }

        private void OnQueryReceive(object sender, MessageEventArgs e)
        {
            fLogger.WriteDebug("Query received: {0} :: {1}", e.EndPoint, e.Data.EncodeAsString());

            var pr = FindPeer(e.EndPoint);

            string queryType = e.Data.Get<BString>("q").ToString();
            var args = e.Data.Get<BDictionary>("a");
            switch (queryType) {
                case "handshake":
                    SendData(e.EndPoint, ProtocolHelper.CreateHandshakeResponse(DHTTransactions.GetNextId(), fDHTClient.LocalID, fLocalPeer.Presence));
                    break;

                case "get_peer_info":
                    SendData(e.EndPoint, ProtocolHelper.CreateGetPeerInfoResponse(DHTTransactions.GetNextId(), fDHTClient.LocalID, fProfile));
                    break;

                case ProtocolHelper.MSG_SIGN_CHAT:
                    long timestamp = args.Get<BNumber>("ts").Value;
                    try {
                        var enc = Convert.ToBoolean(args.Get<BNumber>("enc").Value);
                        var msgdata = args.Get<BString>("msg").Value;
                        string msg = Encoding.UTF8.GetString(msgdata);
                        OnMessageReceive(pr, msg, enc);
                    } finally {
                        SendData(e.EndPoint, ProtocolHelper.CreateChatResponse(DHTTransactions.GetNextId(), fDHTClient.LocalID, timestamp));
                    }
                    break;
            }
        }

        private void OnResponseReceive(object sender, MessageEventArgs e)
        {
            fLogger.WriteDebug("Response received: {0} :: {1}", e.EndPoint, e.Data.EncodeAsString());

            var peer = FindPeer(e.EndPoint);

            var resp = e.Data.Get<BDictionary>("r");
            var qt = resp.Get<BString>("q");
            string queryType = (qt != null) ? qt.ToString() : "";
            switch (queryType) {
                case "handshake":
                    if (peer != null) {
                        peer.Presence = (PresenceStatus)resp.Get<BNumber>("presence").Value;
                    }
                    break;

                case "get_peer_info":
                    if (peer != null) {
                        peer.Profile.Load(resp);
                        peer.ID = e.NodeId;
                        peer.State = PeerState.Identified;
                        fDatabase.SavePeer(peer.Profile, e.EndPoint);
                    }
                    break;

                case ProtocolHelper.MSG_SIGN_CHAT:
                    if (peer != null) {
                        long timestamp = resp.Get<BNumber>("ts").Value;
                        fDatabase.UpdateMessageDelivered(e.NodeId, timestamp);
                    }
                    break;
            }
        }

        private void OnMessageReceive(Peer peer, string msgText, bool encoded)
        {
            if (encoded && !string.IsNullOrEmpty(fProfile.PrivateKey) && !string.IsNullOrEmpty(fPassword)) {
                msgText = Utilities.Decrypt(msgText, fProfile.PrivateKey, fPassword) + " [decrypted]"; // FIXME: temporary debug sign
            }

            var msg = new Message(DateTime.UtcNow, msgText, peer.ID.ToString(), fLocalPeer.ID.ToString());
            fForm.OnMessageReceived(peer, msg);

            fDatabase.SaveMessage(msg);
        }

        private void OnDataReceive(object sender, DataReceiveEventArgs e)
        {
            // TODO: TCP channel of data, future?
            /*var parser = new BencodeParser();
            var dic = parser.Parse<BDictionary>(e.Data);
            fForm.OnMessageReceived(null, "TCP: " + dic.EncodeAsString());

            /*string msgType = dic.Get<BString>("y").ToString();
            switch (msgType) {
                case "q":
                    string queryType = dic.Get<BString>("q").ToString();
                    var args = dic.Get<BDictionary>("a");
                    switch (queryType) {
                        case "handshake":
                            SendHandshakeResponse(e.Peer);
                            break;

                        case "getpeerinfo":
                            SendGetPeerInfoResponse(e.Peer);
                            break;

                        case CHAT_MSG_SIGN:
                            var pr = FindPeer(e.Peer.Address);
                            var msgdata = args.Get<BString>("msg").Value;
                            string msg = Encoding.UTF8.GetString(msgdata);
                            fForm.OnMessageReceived(pr, msg);
                            break;
                    }
                    break;

                case "r":
                    string respType = dic.Get<BString>("r").ToString();
                    switch (respType) {
                        case "handshake":
                            break;

                        case "getpeerinfo":
                            break;
                    }
                    break;
            }*/
        }

        public IList<IDHTPeer> GetPeersList()
        {
            IList<IDHTPeer> result = new List<IDHTPeer>();
            foreach (var peer in fPeers) {
                result.Add(peer);
            }
            return result;
        }

        public IEnumerable<Message> LoadMessages(Peer peer)
        {
            var result = (peer == null) ? new List<Message>() : fDatabase.LoadMessages(peer.ID.ToString());
            return result;
        }

        public void SendInvitation()
        {
            string message = string.Format("I invite you to join the GEDKeeper program network.\r\nMy network node id: {0}\r\n--\r\n{1}", fLocalPeer.ID.ToString(), fLocalPeer.Profile.UserName);
            MailHelper.SendMail("", "Invite", message);
        }

        public void AcceptInvitation(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return;

            Peer peer = FindPeer(nodeId);
            if (peer == null) {
                peer = AddPeer(nodeId);
            }
        }

        public void AddProfile(PeerProfile peerProfile)
        {
            // TODO: add and check existing
        }

        private void LoadPlugin(/*IHost host,*/ Assembly asm)
        {
            if (/*host == null ||*/ asm == null) {
                return;
            }

            Type pluginType = typeof(IDataPlugin);

            Type[] types = asm.GetTypes();
            foreach (Type type in types) {
                if (type.IsInterface || type.IsAbstract)
                    continue;
                if (type.GetInterface(pluginType.FullName) == null)
                    continue;

                IDataPlugin plugin = (IDataPlugin)Activator.CreateInstance(type);
                plugin.Startup(this);
                fDataPlugins.Add(plugin);
            }
        }

        public void LoadPlugins(/*IHost host,*/ string path)
        {
            if (!Directory.Exists(path))
                return;

            fLogger.WriteInfo("Plugins load path: " + path);

            try {
#if !NETSTANDARD
                AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = path;
#else
#endif

                string[] pluginFiles = Directory.GetFiles(path, "*Plugin.dll");
                foreach (string pfn in pluginFiles) {
                    try {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(pfn);
                        Assembly asm = Assembly.Load(assemblyName);

                        if (asm != null) {
                            LoadPlugin(/*host,*/ asm);
                        }
                    } catch (Exception ex) {
                        fLogger.WriteError("LoadPlugins(" + path + ").1", ex);
                        // block exceptions for bad or non-dotnet assemblies
                    }
                }
            } catch (Exception ex) {
                fLogger.WriteError("LoadPlugins(" + path + ")", ex);
            }
        }

        #region Path processing

        public string GetBinPath()
        {
            return Utilities.GetAppPath();
        }

        public string GetDataPath()
        {
            return Utilities.GetAppDataPath();
        }

        #endregion
    }
}
