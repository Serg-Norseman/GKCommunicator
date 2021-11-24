/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using BencodeNET;
using BSLib;
using GKNet.Database;
using GKNet.DHT;
using GKNet.Logging;
using GKNet.STUN;
using GKNet.TCP;
using LumiSoft.Net.STUN.Client;
using Mono.Nat;

namespace GKNet
{
    public sealed class CommunicatorCore : BaseObject, ICommunicatorCore, IDHTPeersHolder
    {
        private static readonly byte[] GKNInfoHash = ProtocolHelper.CreateSignInfoKey();

        private static readonly TimeSpan PingInterval = TimeSpan.FromMinutes(1);

        private ConnectionState fConnectionState;
        private readonly GKNetDatabase fDatabase;
        private readonly DHTClient fDHTClient;
        private readonly IChatForm fForm;
        private readonly ILogger fLogger;
        private string fPassword; // TODO: remove this field, combine password hashing methods to store passwordHash here
        private readonly IList<Peer> fPeers;
        private readonly UserProfile fProfile;
        private STUN_Result fSTUNInfo;
        private readonly TCPDuplexClient fTCPClient;
        private int fTCPListenerPort;
        private Semaphore fUPnPSem = new Semaphore(0, 1);


        public byte[] ClientNodeId
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

        public DHTClient DHTClient
        {
            get { return fDHTClient; }
        }

        public IList<Peer> Peers
        {
            get { return fPeers; }
        }

        public UserProfile Profile
        {
            get { return fProfile; }
        }

        public STUN_Result STUNInfo
        {
            get { return fSTUNInfo; }
        }

        public IPEndPoint PublicEndPoint
        {
            get { return fDHTClient.PublicEndPoint; }
            set { fDHTClient.PublicEndPoint = value; }
        }

        public int TCPListenerPort
        {
            get { return fTCPListenerPort; }
            set { fTCPListenerPort = value; }
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

            IPEndPoint publicEndPoint;
            try {
                fSTUNInfo = STUNUtility.Detect(DHTClient.PublicDHTPort);
                publicEndPoint = (fSTUNInfo.NetType != STUN_NetType.UdpBlocked) ? fSTUNInfo.PublicEndPoint : null;
            } catch (Exception ex) {
                fLogger.WriteError("DetectSTUN() error", ex);
                publicEndPoint = null;
            }

            fDHTClient = new DHTClient(DHTClient.IPAnyAddress, DHTClient.PublicDHTPort, this, ProtocolHelper.CLIENT_VER);
            fDHTClient.PeersFound += OnPeersFound;
            fDHTClient.PeerPinged += OnPeerPinged;
            fDHTClient.QueryReceived += OnQueryReceive;
            fDHTClient.ResponseReceived += OnResponseReceive;

            PublicEndPoint = publicEndPoint;
            AddPeer(PublicEndPoint);

            fTCPClient = new TCPDuplexClient();
            fTCPClient.DataReceive += OnDataReceive;

            fTCPListenerPort = ProtocolHelper.PublicTCPPort;

            //NATHolePunching();
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

        /*private void NATHolePunching()
        {
            new Thread(() => {
                CreatePortMapping();

                Thread.Sleep(1000);

                fForm.OnInitialized();
            }).Start();
        }*/

        public void CreatePortMapping()
        {
            new Thread(() => {
                NatUtility.DeviceFound += DeviceFound;
                NatUtility.DeviceLost += DeviceLost;
                fLogger.WriteInfo("NAT Discovery started");
                NatUtility.StartDiscovery();
                fUPnPSem.WaitOne();
                fLogger.WriteInfo("NAT Discovery stopped");
                NatUtility.StopDiscovery();
            }).Start();
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
                        device.DeletePortMap(m);
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

        private void DeviceLost(object sender, DeviceEventArgs args)
        {
            fLogger.WriteInfo("Device lost, type: {0}", args.Device.GetType().Name);
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
        }

        public bool Authentication(string password)
        {
            bool result = Utilities.VerifyPassword(password, fProfile.PasswordHash);
            fPassword = result ? password : string.Empty;
            return result;
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
                peer = AddPeer(peerEndPoint);
                result = true;
            }

            peer.PingTries = 0;

            if (peer.State != PeerState.Checked && !peer.IsLocal) {
                peer.State = PeerState.Checked;
                SendData(peer.EndPoint, ProtocolHelper.CreateGetPeerInfoQuery(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
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

            if (CheckLocalAddress(peerEndPoint.Address)) {
                return false;
            }

            Peer peer = FindPeer(peerEndPoint);
            if (peer == null) {
                peer = AddPeer(peerEndPoint);
                result = true;
            }

            peer.LastUpdateTime = DateTime.Now;

            SendPing(peer, true);

            return result;
        }

        /// <summary>
        /// Many thanks to Konstantinos Gkinis <aka raidenfreeman> for project https://github.com/raidenfreeman/ICE-Experiment,
        /// who explained in a simple way how to implement UDP Hole punching.
        /// </summary>
        private void SendPing(Peer peer, bool holePunch)
        {
            DateTime dtNow = DateTime.Now;
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
            return ((PublicEndPoint != null) && (DHTHelper.PrepareAddress(PublicEndPoint.Address).Equals(peerAddress)));
        }

        public Peer AddPeer(IPEndPoint peerEndPoint)
        {
            lock (fPeers) {
                Peer peer = new Peer(peerEndPoint);
                peer.IsLocal = CheckLocalAddress(peerEndPoint.Address);

                if (!peer.IsLocal) {
                    fLogger.WriteInfo("Found new peer: {0}", peerEndPoint);
                }

                fPeers.Add(peer);
                return peer;
            }
        }

        public Peer FindPeer(IPEndPoint peerEndPoint)
        {
            return fPeers.FirstOrDefault(x => x.EndPoint.Equals(peerEndPoint));
        }

        #region Protocol features

        private void SendData(IPEndPoint endPoint, BDictionary data)
        {
            fDHTClient.Send(endPoint, data);

            //fTCPClient.Send(endPoint, data.EncodeAsBytes());
        }

        public void SendMessage(Peer peer, string message)
        {
            fLogger.WriteDebug("SendMessage: {0}, `{1}`", peer.EndPoint, message);

            bool encrypted = false;
            if (peer.Profile != null && !string.IsNullOrEmpty(peer.Profile.PublicKey)) {
                message = Utilities.Encrypt(message, peer.Profile.PublicKey);
                encrypted = true;
            }

            SendData(peer.EndPoint, ProtocolHelper.CreateChatMessage(DHTHelper.GetTransactionId(), fDHTClient.LocalID, message, encrypted));
        }

        #endregion

        private void CheckPeers()
        {
            DateTime dtNow = DateTime.Now;
            bool changed = false;
            bool hasCheckedPeers = false;

            for (int i = fPeers.Count - 1; i >= 0; i--) {
                var peer = fPeers[i];
                if (peer.IsLocal) continue;

                bool isChecked = (peer.State == PeerState.Checked);

                SendPing(peer, !isChecked);

                if (!isChecked && (peer.PingTries >= 10 || dtNow - peer.LastUpdateTime > TimeSpan.FromMinutes(10))) {
                    fPeers.Remove(peer);
                    changed = true;
                }

                if (isChecked) {
                    hasCheckedPeers = true;
                }

                /*{
                    peer.State = PeerState.Unchecked;
                    SendData(peer.EndPoint, ProtocolHelper.CreateHandshakeQuery(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
                }*/
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
                    SendData(e.EndPoint, ProtocolHelper.CreateHandshakeResponse(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
                    break;

                case "get_peer_info":
                    SendData(e.EndPoint, ProtocolHelper.CreateGetPeerInfoResponse(DHTHelper.GetTransactionId(), fDHTClient.LocalID, fProfile));
                    break;

                case "chat":
                    var enc = Convert.ToBoolean(args.Get<BNumber>("enc").Value);
                    var msgdata = args.Get<BString>("msg").Value;
                    string msg = Encoding.UTF8.GetString(msgdata);
                    if (enc && !string.IsNullOrEmpty(fProfile.PrivateKey) && !string.IsNullOrEmpty(fPassword)) {
                        msg = Utilities.Decrypt(msg, fProfile.PrivateKey, fPassword) + " [decrypted]"; // FIXME: temporary debug sign
                    }
                    fForm.OnMessageReceived(pr, msg);
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
                        peer.State = PeerState.Checked;
                        SendData(e.EndPoint, ProtocolHelper.CreateGetPeerInfoQuery(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
                    }
                    break;

                case "get_peer_info":
                    if (peer != null) {
                        peer.Profile.Load(resp);
                        peer.Profile.NodeId = e.NodeId;
                        fDatabase.SavePeer(peer.Profile, e.EndPoint);
                    }
                    break;
            }
        }

        private void OnDataReceive(object sender, DataReceiveEventArgs e)
        {
            // TODO: TCP channel of data, future?
            var parser = new BencodeParser();
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

                        case "chat":
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

        public void Join(string member)
        {
        }

        public void Leave(string member)
        {
        }

        public void Send(Peer target, string message)
        {
            SendMessage(target, message);
        }

        public void SendToAll(string message)
        {
            foreach (var peer in fPeers) {
                if (!peer.IsLocal) {
                    SendMessage(peer, message);
                }
            }
        }

        public IList<IDHTPeer> GetPeersList()
        {
            IList<IDHTPeer> result = new List<IDHTPeer>();
            foreach (var peer in fPeers) {
                result.Add(peer);
            }
            return result;
        }
    }
}
