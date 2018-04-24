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

#define ECHO_MODE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using GKNet.Core;
using GKNet.DHT;
using GKNet.Protocol;
using GKNet.TCP;

namespace GKNet
{
    public class ChatDHTCP : IChatCore, ILogger
    {
        private IChatForm fForm;

        private bool fConnected;
        private DHTClient fDHTClient;
        private string fMemberName;
        private readonly BencodeParser fParser;
        private IList<Peer> fPeers;
        private readonly UserProfile fProfile;
        private TCPDuplexClient fTCPClient;

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

        public ChatDHTCP(IChatForm form)
        {
            if (form == null) {
                throw new ArgumentNullException("form");
            }

            fConnected = false;
            fForm = form;
            fProfile = new UserProfile();
            fParser = new BencodeParser();
            fPeers = new List<Peer>();

            InitLogs();

            fDHTClient = new DHTClient(IPAddress.Any, DHTClient.PublicDHTPort, this);
            fDHTClient.PeersFound += delegate (object sender, PeersFoundEventArgs e) {
                WriteLog(string.Format("Found peers: {0}", e.Peers.Count));

                foreach (var p in e.Peers) {
                    var peerAddress = p.Address;
                    var ex = FindPeer(peerAddress);
                    if (ex == null) {
                        AddPeer(peerAddress);
                    }
                }
            };

            fTCPClient = new TCPDuplexClient();
            fTCPClient.DataReceive += RaiseDataReceive;
        }

        public void Connect()
        {
            // FIXME: sometimes it does not work correctly
            WriteLog("Local IP: " + SysHelper.GetPublicIPAddress().ToString());

            var snkInfoHash = ProtocolHelper.CreateSignInfoKey();
            WriteLog("Search for: " + snkInfoHash.ToHexString());

            fDHTClient.Run();
            fDHTClient.JoinNetwork();
            fDHTClient.SearchNodes(snkInfoHash);

            fTCPClient.Connect(ProtocolHelper.PublicTCPPort);

            fConnected = true;
            new Thread(() => {
                while (fConnected) {
                    CheckPeers();
                    Thread.Sleep(60000);
                }
            }).Start();
        }

        public void Disconnect()
        {
            fConnected = false;
            fTCPClient.Disconnect();
            fDHTClient.StopSearch();
        }

        public void AddPeer(IPAddress peerAddress)
        {
            lock (fPeers) {
                fPeers.Add(new Peer(peerAddress));
                fForm.OnPeersListChanged();
                WriteLog(string.Format("Found new peer: {0}", peerAddress.ToString()));
            }
        }

        public Peer FindPeer(IPAddress peerAddress)
        {
            return fPeers.FirstOrDefault(x => x.Address.Equals(peerAddress));
        }

        #region Protocol features

        public void SendHandshakeQuery(Peer peer)
        {
            peer.State = PeerState.Unchecked;

            var data = ProtocolHelper.CreateHandshakeQuery();
            var endPoint = new IPEndPoint(peer.Address, ProtocolHelper.PublicTCPPort);
            var conn = fTCPClient.GetConnection(endPoint);
            if (conn != null) {
                conn.Send(data);
            }
        }

        public void SendHandshakeResponse(IPEndPoint endPoint)
        {
            var data = ProtocolHelper.CreateHandshakeResponse();
            var conn = fTCPClient.GetConnection(endPoint);
            if (conn != null) {
                conn.Send(data);
            }
        }

        public void SendMessage(Peer peer, string message)
        {
            var data = ProtocolHelper.CreateChatMessage(message);
            var endPoint = new IPEndPoint(peer.Address, ProtocolHelper.PublicTCPPort);
            var conn = fTCPClient.GetConnection(endPoint);
            if (conn != null) {
                conn.Send(data);
            }
        }

        #endregion

        private void CheckPeers()
        {
            foreach (var p in fPeers) {
                SendHandshakeQuery(p);
            }
            fForm.OnPeersListChanged();
        }

        private void RaiseDataReceive(object sender, DataReceiveEventArgs e)
        {
            var dic = fParser.Parse<BDictionary>(e.Data);

            string msgType = dic.Get<BString>("y").ToString();
            switch (msgType) {
                case "q":
                    string queryType = dic.Get<BString>("q").ToString();
                    var args = dic.Get<BDictionary>("a");
                    switch (queryType) {
                        case "handshake":
                            SendHandshakeResponse(e.Peer);
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
                            var pr = FindPeer(e.Peer.Address);
                            if (pr != null) {
                                pr.State = PeerState.Checked;
                            }
                            break;
                    }
                    break;
            }
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
            foreach (var p in fPeers) {
                SendMessage(p, message);
            }
        }

        private void InitLogs()
        {
            if (File.Exists("./dht.log")) {
                File.Delete("./dht.log");
            }
        }

        public void WriteLog(string str, bool display = true)
        {
            var fswriter = new StreamWriter(new FileStream("./dht.log", FileMode.Append));
            fswriter.WriteLine(str);
            fswriter.Flush();
            fswriter.Close();
        }
    }
}
