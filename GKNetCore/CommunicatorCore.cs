/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BencodeNET;
using BSLib;
using GKNet.Database;
using GKNet.DHT;
using GKNet.Logging;
using GKNet.TCP;
using LumiSoft.Net.STUN.Client;

namespace GKNet
{
    public sealed class CommunicatorCore : BaseObject, ICommunicatorCore, IDHTPeersHolder
    {
        private static readonly byte[] GKNInfoHash = ProtocolHelper.CreateSignInfoKey();

        private bool fConnected;
        private readonly GKNetDatabase fDatabase;
        private readonly DHTClient fDHTClient;
        private readonly IChatForm fForm;
        private readonly ILogger fLogger;
        private readonly IList<Peer> fPeers;
        private readonly UserProfile fProfile;
        private IPEndPoint fPublicEndPoint;
        private STUN_Result fSTUNInfo;
        private readonly TCPDuplexClient fTCPClient;
        private int fTCPListenerPort;


        public GKNetDatabase Database
        {
            get { return fDatabase; }
        }

        public DHTClient DHTClient
        {
            get { return fDHTClient; }
        }

        public bool IsConnected
        {
            get { return fConnected; }
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

            fConnected = false;
            fForm = form;
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "ChatDHTCP");
            fPeers = new List<Peer>();
            fSTUNInfo = null;

            fProfile = new UserProfile();

            fDatabase = new GKNetDatabase();
            fDatabase.Connect();
            fDatabase.LoadProfile(fProfile);

            fDHTClient = new DHTClient(DHTClient.IPAnyAddress, DHTClient.PublicDHTPort, this, ProtocolHelper.CLIENT_VER);
            fDHTClient.PeersFound += OnPeersFound;
            fDHTClient.PeerPinged += OnPeerPinged;
            fDHTClient.QueryReceived += OnQueryReceive;
            fDHTClient.ResponseReceived += OnResponseReceive;

            NATHolePunching(fDHTClient.Socket);

            fTCPClient = new TCPDuplexClient();
            fTCPClient.DataReceive += OnDataReceive;

            fTCPListenerPort = ProtocolHelper.PublicTCPPort;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                fDatabase.Disconnect();
            }
            base.Dispose(disposing);
        }

        private void NATHolePunching(Socket socket)
        {
            new Thread(() => {
                /*using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                    socket.SetIPProtectionLevelUnrestricted();
                    socket.Bind(new IPEndPoint(IPAddress.Any, 0));

                    DetectSTUN(ProtocolHelper.STUNServer, socket);
                    //NATMapper.CreateNATMapping(this, stunResult);
                }*/

                DetectSTUN(ProtocolHelper.STUNServer, socket);
                NATMapper.CreateNATMapping(fSTUNInfo);

                fForm.OnInitialized();
            }).Start();
        }

        private void DetectSTUN(string stunServer, Socket socket)
        {
            if (string.IsNullOrEmpty(stunServer)) {
                throw new ArgumentException("Not specified STUN server");
            }

            STUN_Result result = null;
            try {
                result = STUN_Client.Query(stunServer, 3478, socket);

                fLogger.WriteInfo("STUN Info:");
                fLogger.WriteInfo("NetType: {0}", result.NetType);
                fLogger.WriteInfo("LocalEndPoint: {0}", socket.LocalEndPoint);
                if (result.NetType != STUN_NetType.UdpBlocked) {
                    fPublicEndPoint = result.PublicEndPoint;
                } else {
                    fPublicEndPoint = null;
                }
                fLogger.WriteInfo("PublicEndPoint: {0}", fPublicEndPoint);
            } catch (Exception ex) {
                fLogger.WriteError("DetectSTUN() error", ex);
            }

            fSTUNInfo = result;
            fDHTClient.PublicEndPoint = fPublicEndPoint;
        }

        public void Connect()
        {
            fDHTClient.Run();
            fDHTClient.JoinNetwork();
            fDHTClient.SearchNodes(GKNInfoHash);

            fTCPClient.Connect(fTCPListenerPort);

            fConnected = true;
            new Thread(() => {
                int x = 0;
                while (fConnected) {
                    if (++x >= 60) {
                        CheckPeers();
                        x = 0;
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public void Disconnect()
        {
            fConnected = false;
            fTCPClient.Disconnect();
            fDHTClient.StopSearch();
        }

        public bool CheckPeer(IPEndPoint peerEndPoint)
        {
            bool result = false;
            if (peerEndPoint == null) return false;

            var peerAddress = peerEndPoint.Address;
            Peer peer = FindPeer(peerAddress);
            if (peer == null) {
                peer = AddPeer(peerAddress, peerEndPoint.Port);
                result = true;
            }

            if (peer.State != PeerState.Checked && !peer.IsLocal) {
                peer.EndPoint.Port = peerEndPoint.Port;
                peer.State = PeerState.Checked;
                result = true;
            }

            SendData(peer.EndPoint, ProtocolHelper.CreateGetPeerInfoQuery(DHTHelper.GetTransactionId(), fDHTClient.LocalID));

            return result;
        }

        public bool UpdatePeer(IPEndPoint peerEndPoint)
        {
            bool result = false;
            if (peerEndPoint == null) return false;

            var peerAddress = peerEndPoint.Address;
            Peer peer = FindPeer(peerAddress);
            if (peer == null) {
                peer = AddPeer(peerAddress, peerEndPoint.Port);
                result = true;
            }

            // TODO: it's bad place for peers' ping!
            // FIXME: find out which ping gets the answer! check it for a long time testing!
            //fDHTClient.SendPingQuery(peerEndPoint);
            fDHTClient.SendPingQuery(new IPEndPoint(peerAddress, fDHTClient.LocalEndPoint.Port));

            return result;
        }

        public bool CheckLocalAddress(IPAddress peerAddress)
        {
            return ((fPublicEndPoint != null) && (DHTHelper.PrepareAddress(fPublicEndPoint.Address).Equals(peerAddress)));
        }

        public Peer AddPeer(IPAddress peerAddress, int port)
        {
            fLogger.WriteInfo("Found new peer: {0}:{1}", peerAddress, port);

            lock (fPeers) {
                Peer peer = new Peer(peerAddress, port);
                peer.IsLocal = CheckLocalAddress(peerAddress);
                fPeers.Add(peer);
                return peer;
            }
        }

        public Peer FindPeer(IPAddress peerAddress)
        {
            return fPeers.FirstOrDefault(x => x.Address.Equals(peerAddress));
        }

        #region Protocol features

        private void SendData(IPEndPoint endPoint, BDictionary data)
        {
            fDHTClient.Send(endPoint, data);

            fTCPClient.Send(endPoint, data.EncodeAsBytes());
        }

        public void SendMessage(Peer peer, string message)
        {
            SendData(peer.EndPoint, ProtocolHelper.CreateChatMessage(DHTHelper.GetTransactionId(), fDHTClient.LocalID, message));
        }

        #endregion

        private void CheckPeers()
        {
            // FIXME: think through this logic better!
            /*foreach (var peer in fPeers) {
                if (!peer.IsLocal) {
                    peer.State = PeerState.Unchecked;
                    SendData(peer.EndPoint, ProtocolHelper.CreateHandshakeQuery(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
                }
            }
            fForm.OnPeersListChanged();*/
        }

        private void OnPeersFound(object sender, PeersFoundEventArgs e)
        {
            bool changed = false;
            int newFounded = 0;
            foreach (var p in e.Peers) {
                changed = UpdatePeer(p);
                if (changed) {
                    newFounded += 1;
                }
            }

            if (changed) {
                fLogger.WriteDebug("Found DHT peers: {0}", newFounded);
                fForm.OnPeersListChanged();
            }
        }

        private void OnPeerPinged(object sender, PeerPingedEventArgs e)
        {
            fLogger.WriteDebug("Peer pinged: {0}", e.EndPoint);

            bool changed = CheckPeer(e.EndPoint);

            if (changed) {
                fForm.OnPeersListChanged();
            }
        }

        private void OnQueryReceive(object sender, MessageEventArgs e)
        {
            fLogger.WriteDebug("Query received: {0} :: {1}", e.EndPoint, e.Data.EncodeAsString());

            var pr = FindPeer(e.EndPoint.Address);

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
                    var msgdata = args.Get<BString>("msg").Value;
                    string msg = Encoding.UTF8.GetString(msgdata);
                    fForm.OnMessageReceived(pr, msg);
                    break;
            }
        }

        private void OnResponseReceive(object sender, MessageEventArgs e)
        {
            fLogger.WriteDebug("Response received: {0} :: {1}", e.EndPoint, e.Data.EncodeAsString());

            var pr = FindPeer(e.EndPoint.Address);

            var resp = e.Data.Get<BDictionary>("r");
            var qt = resp.Get<BString>("q");
            string queryType = (qt != null) ? qt.ToString() : "";
            switch (queryType) {
                case "handshake":
                    if (pr != null) {
                        pr.State = PeerState.Checked;
                        SendData(e.EndPoint, ProtocolHelper.CreateGetPeerInfoQuery(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
                    }
                    break;

                case "get_peer_info":
                    if (pr != null) {
                        pr.Profile.Load(resp);
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
                SendMessage(peer, message);
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
