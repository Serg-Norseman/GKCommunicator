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
using BencodeNET.Objects;
using BencodeNET.Parsing;
using GKNet.DHT;
using GKNet.Logging;
using GKNet.TCP;
using LumiSoft.Net.STUN.Client;

namespace GKNet
{
    public sealed class CommunicatorCore : ICommunicatorCore, IDHTPeersHolder
    {
        private bool fConnected;
        private readonly DHTClient fDHTClient;
        private readonly IChatForm fForm;
        private readonly ILogger fLogger;
        private string fMemberName;
        private readonly BencodeParser fParser;
        private readonly IList<Peer> fPeers;
        private readonly UserProfile fProfile;
        private IPEndPoint fPublicEndPoint;
        private STUN_Result fSTUNInfo;
        private readonly TCPDuplexClient fTCPClient;
        private int fTCPListenerPort;


        public DHTClient DHTClient
        {
            get { return fDHTClient; }
        }

        public bool IsConnected
        {
            get { return fConnected; }
        }

        public string MemberName
        {
            get { return fMemberName; }
            set { fMemberName = value; }
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
            fProfile = new UserProfile();
            fProfile.ResetSystem();
            fParser = new BencodeParser();
            fPeers = new List<Peer>();
            fSTUNInfo = null;

            fDHTClient = new DHTClient(DHTClient.IPAnyAddress, DHTClient.PublicDHTPort, this, ProtocolHelper.CLIENT_VER);
            fDHTClient.PeersFound += OnPeersFound;
            fDHTClient.PeerPinged += OnPeerPinged;
            fDHTClient.QueryReceived += OnQueryReceive;
            fDHTClient.ResponseReceived += OnResponseReceive;

            NATHolePunching();

            fTCPClient = new TCPDuplexClient();
            fTCPClient.DataReceive += OnDataReceive;
        }

        private void NATHolePunching()
        {
            new Thread(() => {
                Thread.Sleep(1000);

                //DetectSTUN(ProtocolHelper.STUNServer, fDHTClient.Socket);
                //NATMapper.CreateNATMapping(fSTUNInfo);

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                    socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
                    socket.Bind(new IPEndPoint(IPAddress.Any, 0));

                    DetectSTUN(ProtocolHelper.STUNServer, socket);
                    //NATMapper.CreateNATMapping(this, stunResult);
                }
            }).Start();
        }

        private void DetectSTUN(string stunServer, Socket socket)
        {
            if (string.IsNullOrEmpty(stunServer)) {
                throw new Exception("Not specified STUN server");
            }

            STUN_Result result = null;
            try {
                result = STUN_Client.Query(stunServer, 3478, socket);

                fLogger.WriteInfo("STUN Info:");
                fLogger.WriteInfo("NetType: {0}", result.NetType.ToString());
                fLogger.WriteInfo("LocalEndPoint: {0}", socket.LocalEndPoint.ToString());
                if (result.NetType != STUN_NetType.UdpBlocked) {
                    fPublicEndPoint = result.PublicEndPoint;
                    fLogger.WriteInfo("PublicEndPoint: {0}", fPublicEndPoint.ToString());
                } else {
                    fPublicEndPoint = null;
                    fLogger.WriteInfo("PublicEndPoint: -");
                }
            } catch (Exception ex) {
                fLogger.WriteError("DetectSTUN() error", ex);
            }

            fSTUNInfo = result;
            fDHTClient.PublicEndPoint = fPublicEndPoint;
        }

        public void Connect()
        {
            fLogger.WriteInfo("Public IP: " + NetHelper.GetPublicIPAddress());

            var snkInfoHash = ProtocolHelper.CreateSignInfoKey();
            fLogger.WriteInfo("Search for: " + snkInfoHash.ToHexString());

            fDHTClient.Run();
            fDHTClient.JoinNetwork();
            fDHTClient.SearchNodes(snkInfoHash);

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

            var peerAddress = peerEndPoint.Address;
            Peer peer = FindPeer(peerAddress);
            if (peer == null) {
                peer = AddPeer(peerAddress, peerEndPoint.Port);
                result = true;
            }

            if (peer.State != PeerState.Checked) {
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

            var peerAddress = peerEndPoint.Address;
            Peer peer = FindPeer(peerAddress);
            if (peer == null) {
                peer = AddPeer(peerAddress, peerEndPoint.Port);
                result = true;
            }

            if (CheckLocalAddress(peerAddress) && !peer.IsLocal) {
                peer.IsLocal = true;
                result = true;
            }

            fDHTClient.SendPingQuery(peerEndPoint);
            fDHTClient.SendPingQuery(new IPEndPoint(peerAddress, fDHTClient.LocalEndPoint.Port));
            fDHTClient.SendPingQuery(new IPEndPoint(peerAddress, fPublicEndPoint.Port));

            return result;
        }

        public bool CheckLocalAddress(IPAddress peerAddress)
        {
            return ((fPublicEndPoint != null) && (DHTHelper.PrepareAddress(fPublicEndPoint.Address).Equals(peerAddress)));
        }

        public Peer AddPeer(IPAddress peerAddress, int port)
        {
            fLogger.WriteInfo(string.Format("Found new peer: {0}:{1}", peerAddress, port));

            lock (fPeers) {
                Peer peer = new Peer(peerAddress, port);
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

            /*var conn = fTCPClient.GetConnection(endPoint);
            if (conn != null) {
                conn.Send(data.EncodeAsBytes());
            }*/
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
            fLogger.WriteInfo(string.Format("Found DHT peers: {0}", e.Peers.Count));

            bool changed = false;
            foreach (var p in e.Peers) {
                changed = UpdatePeer(p);
            }

            if (changed) {
                fForm.OnPeersListChanged();
            }
        }

        private void OnPeerPinged(object sender, PeerPingedEventArgs e)
        {
            fLogger.WriteInfo(string.Format("Peer pinged: {0}", e.EndPoint));

            bool changed = CheckPeer(e.EndPoint);

            if (changed) {
                fForm.OnPeersListChanged();
            }
        }

        private void OnQueryReceive(object sender, MessageEventArgs e)
        {
            fLogger.WriteDebug(string.Format("Query received: {0}", e.EndPoint));

            var pr = FindPeer(e.EndPoint.Address);
            fForm.OnMessageReceived(pr, e.Data.EncodeAsString());

            string queryType = e.Data.Get<BString>("q").ToString();
            var args = e.Data.Get<BDictionary>("a");
            switch (queryType) {
                case "handshake":
                    SendData(e.EndPoint, ProtocolHelper.CreateHandshakeResponse(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
                    break;

                case "get_peer_info":
                    SendData(e.EndPoint, ProtocolHelper.CreateGetPeerInfoResponse(DHTHelper.GetTransactionId(), fDHTClient.LocalID));
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
            fLogger.WriteDebug(string.Format("Response received: {0}", e.EndPoint));

            var pr = FindPeer(e.EndPoint.Address);
            fForm.OnMessageReceived(pr, e.Data.EncodeAsString());

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
                    var peerInfo = new PeerProfile();
                    peerInfo.Load(resp);
                    fForm.OnMessageReceived(pr, peerInfo.UserName + "/" + peerInfo.Country  + "/" + peerInfo.TimeZone + "/" + peerInfo.Languages);
                    break;
            }
        }

        private void OnDataReceive(object sender, DataReceiveEventArgs e)
        {
            // TODO: TCP channel of data, future?
            /*var dic = fParser.Parse<BDictionary>(e.Data);
            fForm.OnMessageReceived(null, dic.EncodeAsString());

            string msgType = dic.Get<BString>("y").ToString();
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
