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
using System.Threading;
using BencodeNET.Objects;
using BencodeNET.Parsing;

namespace GKNet.DHT
{
    public sealed class DHTClient
    {
        public const int PublicDHTPort = 6881;
        public const int KTableSize = 2048;

        //private byte[] fBuffer = new byte[65535];
        //private Socket fSocket;

        private byte[] fLocalID;
        private IList<IPAddress> fRouters;
        private byte[] fSearchInfoHash;
        private bool fSearchRunned;

        private readonly UdpClient fClient;
        private readonly IPEndPoint fDefaultIP;
        private readonly IPEndPoint fLocalIP;
        private readonly ILogger fLogger;
        private readonly BencodeParser fParser;
        private readonly DHTRoutingTable fRoutingTable;
        private readonly Dictionary<int, DHTMessage> fTransactions;

        public event EventHandler<PeersFoundEventArgs> PeersFound;

        public DHTClient(IPAddress addr, int port, ILogger logger)
        {
            fDefaultIP = new IPEndPoint(IPAddress.Loopback, 0);
            fLocalID = DHTHelper.GetRandomID();
            fLocalIP = new IPEndPoint(addr, port);
            fLogger = logger;
            fParser = new BencodeParser();
            fRoutingTable = new DHTRoutingTable(KTableSize);
            fSearchRunned = false;
            fTransactions = new Dictionary<int, DHTMessage>();

            const long IOC_IN = 0x80000000;
            const long IOC_VENDOR = 0x18000000;
            const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];

            //fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //fSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
            //fSocket.Bind(fLocalIP);

            fClient = new UdpClient(fLocalIP);
            fClient.AllowNatTraversal(true);
            fClient.Ttl = 255;
            fClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
            fClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void Run()
        {
            //EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
            //fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
            fClient.BeginReceive(EndRecv, null);
        }

        public void Stop()
        {
            fClient.Close();
        }

        public void JoinNetwork()
        {
            fRouters = new List<string>() {
                            "router.bittorrent.com",
                            "dht.transmissionbt.com",
                            "router.utorrent.com"
                        }.Select(x => Dns.GetHostEntry(x).AddressList[0]).ToList();

            /*new Thread(() => {
                while (true) {
                    Thread.Sleep(5000);
                }
            }).Start();*/
        }

        public void SearchNodes(byte[] searchInfoHash)
        {
            fSearchInfoHash = searchInfoHash;
            fSearchRunned = true;

            new Thread(() => {
                while (fSearchRunned) {
                    int count = 0;
                    lock (fRoutingTable) {
                        count = fRoutingTable.Count;
                    }

                    if (count == 0) {
                        // reboot if empty
                        foreach (var t in fRouters) {
                            SendFindNodeQuery(new IPEndPoint(t, PublicDHTPort), null);
                        }
                    } else {
                        // search
                        var nodes = fRoutingTable.FindNodes(fSearchInfoHash);
                        foreach (var node in nodes) {
                            SendGetPeersQuery(node.EndPoint, fSearchInfoHash);
                        }
                    }

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public void StopSearch()
        {
            fSearchRunned = false;
        }

        #region Receive messages and data

        private void EndRecv(IAsyncResult result)
        {
            /*EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
            try {
                int count = fSocket.EndReceiveFrom(result, ref remoteAddress);
                if (count > 0)
                    OnRecvMessage(fBuffer.Take(count).ToArray(), (IPEndPoint)remoteAddress);
            } catch (Exception ex) {
                //WriteLog(ex.ToString());
            }
            bool notsuccess = false;
            do {
                try {
                    fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
                    notsuccess = false;
                } catch (Exception ex) {
                    WriteLog("EndRecv(): " + ex);
                    notsuccess = true;
                }
            } while (notsuccess);*/

            try {
                IPEndPoint e = new IPEndPoint(IPAddress.Any, fLocalIP.Port);
                byte[] buffer = fClient.EndReceive(result, ref e);

                OnRecvMessage(buffer, e);

                fClient.BeginReceive(EndRecv, null);
            } catch (SocketException) {
                fClient.BeginReceive(EndRecv, null);
            } catch (Exception) {
            }
        }

        private void OnRecvMessage(byte[] data, IPEndPoint ipinfo)
        {
            try {
                var dic = fParser.Parse<BDictionary>(data);
                string msgType = dic.Get<BString>("y").ToString();
                switch (msgType) {
                    case "r":
                        // on receive response
                        OnRecvResponseX(ipinfo, dic);
                        break;

                    case "q":
                        // on receive query
                        string queryType = dic.Get<BString>("q").ToString();
                        switch (queryType) {
                            case "ping":
                                OnRecvPingQuery(ipinfo, dic);
                                break;

                            case "find_node":
                                OnRecvFindNodeQuery(ipinfo, dic);
                                break;

                            case "get_peers":
                                OnRecvGetPeersQuery(ipinfo, dic);
                                break;

                            case "announce_peer":
                                OnRecvAnnouncePeerQuery(ipinfo, dic);
                                break;
                        }
                        break;

                    case "e":
                        // on receive error
                        OnRecvErrorX(ipinfo, dic);
                        break;
                }
            } catch (Exception ex) {
                fLogger.WriteLog("OnRecvMessage(): " + ex);
            }
        }

        private void OnRecvErrorX(IPEndPoint ipinfo, BDictionary data)
        {
            var errData = data.Get<BList>("e");
            if (errData != null && errData.Count != 0) {
                var errCode = errData.Get<BNumber>(0);
                var errText = errData.Get<BString>(1);
                fLogger.WriteLog("error receive: " + errCode + " / " + errText);
            }
        }

        private void OnRecvResponseX(IPEndPoint ipinfo, BDictionary data)
        {
            var tid = data.Get<BString>("t");
            var returnValues = data.Get<BDictionary>("r");

            var id = returnValues.Get<BString>("id");
            var tokStr = returnValues.Get<BString>("token");
            var valuesList = returnValues.Get<BList>("values");
            var nodesStr = returnValues.Get<BString>("nodes");

            fRoutingTable.AddOrUpdateNode(new DHTNode(id.Value, ipinfo));

            // according to bep_????, most types of response contain a list of nodes
            ProcessNodesStr(ipinfo, nodesStr);

            // define type of response by transactionId of query/response
            QueryType queryType = CheckTransaction(tid);

            bool canAnnounce = false;
            switch (queryType) {
                case QueryType.ping:
                    if ((id != null && id.Length != 0)) {
                        // correct and complete response
                    }
                    break;

                case QueryType.find_node:
                    if ((id != null && id.Length != 0)) {
                        // correct and complete response
                    }
                    break;

                case QueryType.get_peers:
                    if (tokStr != null && tokStr.Length != 0) {
                        // correct and complete response
                        if (!ProcessValuesStr(ipinfo, valuesList)) {
                            canAnnounce = true;
                        }
                    }
                    break;

                case QueryType.announce_peer:
                    if ((id != null && id.Length != 0)) {
                        // correct and complete response
                    }
                    break;

                case QueryType.none:
                    // TransactionId bad or unknown
                    break;
            }

            if (canAnnounce) {
                SendAnnouncePeerQuery(ipinfo, fSearchInfoHash, 1, fLocalIP.Port, tokStr);
            }
        }

        private bool ProcessValuesStr(IPEndPoint ipinfo, BList valuesList)
        {
            bool result = false;
            if (valuesList != null && valuesList.Count != 0) {
                var values = DHTHelper.ParseValuesList(valuesList);
                if (values.Count > 0) {
                    // if infohash and peers for it was found
                    fLogger.WriteLog("receive " + values.Count + " values from " + ipinfo.ToString());

                    foreach (var val in values) {
                        fLogger.WriteLog("send ping " + values[0].ToString());
                        SendPingQuery(values[0], true);

                        var newaddr = new IPEndPoint(values[0].Address, fLocalIP.Port);
                        fLogger.WriteLog("send ping " + newaddr.ToString());
                        SendPingQuery(newaddr, true);
                    }

                    // TODO: handshake and other
                    // TODO: check that there is no current node in the response

                    RaisePeersFound(fSearchInfoHash, values);
                    result = true;
                }
            }
            return result;
        }

        private void ProcessNodesStr(IPEndPoint ipinfo, BString nodesStr)
        {
            if (nodesStr != null && nodesStr.Length != 0) {
                if (nodesStr.Value.Length % 26 != 0)
                    throw new Exception("sd");

                var nodesList = DHTHelper.ParseNodesList(nodesStr.Value);
                fLogger.WriteLog("receive " + nodesList.Count + " nodes from " + ipinfo.ToString());

                foreach (var t in nodesList) {
                    fRoutingTable.AddOrUpdateNode(t);
                }
            }
        }

        private void OnRecvAnnouncePeerQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var infoHash = args.Get<BString>("info_hash");
            var impliedPort = args.Get<BNumber>("implied_port");
            int port = (impliedPort != null && impliedPort.Value == 1) ? ipinfo.Port : (int)args.Get<BNumber>("port").Value;

            fLogger.WriteLog("receive `announce_peer` query " + ipinfo.ToString());

            fRoutingTable.AddOrUpdateNode(new DHTNode(id.Value, ipinfo));

            // Did we receive our infohash? Is this our partner?
            if (DHTHelper.ArraysEqual(infoHash.Value, fSearchInfoHash)) {
                // TODO: Create list from our neighbours
            } else {
                // TODO: Accept the announcement of all hashes for network support
            }

            //SendAnnouncePeerResponse(ipinfo, t);
        }

        private void OnRecvPingQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");

            var handshake = data.Get<BString>("handshake");
            if (handshake != null && handshake.ToString() == "gkn") {
                fLogger.WriteLog("Found a peer! " + ipinfo.ToString());
            }

            fRoutingTable.AddOrUpdateNode(new DHTNode(id.Value, ipinfo));

            fLogger.WriteLog("receive `ping` query " + ipinfo.ToString());
            SendPingResponse(ipinfo, t);
        }

        private void OnRecvFindNodeQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");

            fRoutingTable.AddOrUpdateNode(new DHTNode(id.Value, ipinfo));

            fLogger.WriteLog("receive `find_node` query");
            // TODO: response!
        }

        private void OnRecvGetPeersQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var info_hash = args.Get<BString>("info_hash");
            var neighbor = DHTHelper.GetNeighbor(info_hash.Value, fLocalID);
            var nodesList = fRoutingTable.FindNodes(info_hash.Value);
            BList values = null;

            fLogger.WriteLog("receive `get_peers` query from " + ipinfo.ToString() + " [" + id.Value.ToHexString() + "] for " + info_hash.Value.ToHexString());

            fRoutingTable.AddOrUpdateNode(new DHTNode(id.Value, ipinfo));

            byte[] nodesArray = new byte[nodesList.Count * 26];
            var i = 0;
            foreach (var node in nodesList) {
                var compact = DHTHelper.CompactNode(node);
                Array.Copy(compact, 0, nodesArray, i * 26, 26);
            }

            if (DHTHelper.ArraysEqual(info_hash.Value, fSearchInfoHash)) {
                fLogger.WriteLog("receive `get_peers` query for our HASH");

                // TODO: create list from our neighbours
                //values = new BList();
                //values.Add(new BString(Helpers.CompactEndPoint(???)));
            }

            Send(ipinfo, DHTMessage.CreateGetPeersResponse(t, neighbor, info_hash.Value, values, new BString(nodesArray)));
        }

        #endregion

        #region Events processing

        private void RaisePeersFound(byte[] infoHash, List<IPEndPoint> peers)
        {
            if (PeersFound != null) {
                PeersFound(this, new PeersFoundEventArgs(infoHash, peers));
            }
        }

        #endregion

        #region Transactions

        public void SetTransaction(BString transactionId, DHTMessage message)
        {
            int tid = BitConverter.ToInt16(transactionId.Value, 0);
            fTransactions[tid] = message;
        }

        public QueryType CheckTransaction(BString transactionId)
        {
            QueryType result = QueryType.none;

            if (transactionId != null && transactionId.Length == 2) {
                DHTMessage message = null;
                int tid = BitConverter.ToInt16(transactionId.Value, 0);

                if (fTransactions.TryGetValue(tid, out message)) {
                    result = message.QueryType;
                    fTransactions.Remove(tid);
                }
            }

            return result;
        }

        #endregion

        #region Queries and responses

        private void SendPingResponse(IPEndPoint address, BString transactionID)
        {
            Send(address, DHTMessage.CreatePingResponse(transactionID, fLocalID));
        }

        private void SendPingQuery(IPEndPoint address, bool extHandshake = false)
        {
            var transactionID = DHTHelper.GetTransactionId();
            byte[] nid = fLocalID;

            BDictionary sendData = DHTMessage.CreatePingQuery(transactionID, nid);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.ping, sendData));

            if (extHandshake) {
                sendData.Add("handshake", "gkn");
            }

            Send(address, sendData);
        }

        private void SendFindNodeQuery(IPEndPoint address, byte[] data)
        {
            var transactionID = DHTHelper.GetTransactionId();
            byte[] nid = (data == null) ? fLocalID : DHTHelper.GetNeighbor(data, fLocalID);

            BDictionary sendData = DHTMessage.CreateFindNodeQuery(transactionID, nid);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.find_node, sendData));
            Send(address, sendData);
        }

        public void SendAnnouncePeerQuery(IPEndPoint address, byte[] infoHash, byte implied_port, int port, BString token)
        {
            var transactionID = DHTHelper.GetTransactionId();
            byte[] nid = fLocalID;

            BDictionary sendData = DHTMessage.CreateAnnouncePeerQuery(transactionID, nid, infoHash, implied_port, port, token);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.announce_peer, sendData));
            Send(address, sendData);
        }

        private void SendAnnouncePeerResponse(IPEndPoint address, BString transactionID)
        {
            Send(address, DHTMessage.CreateAnnouncePeerResponse(transactionID, fLocalID));
        }

        private void SendGetPeersQuery(IPEndPoint address, byte[] infoHash)
        {
            var transactionID = DHTHelper.GetTransactionId();
            byte[] nid = fLocalID;

            BDictionary sendData = DHTMessage.CreateGetPeersQuery(transactionID, nid, infoHash);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.get_peers, sendData));
            Send(address, sendData);
        }

        private void Send(IPEndPoint address, BDictionary data)
        {
            try {
                byte[] dataArray = data.EncodeAsBytes();
                //fSocket.BeginSendTo(dataArray, 0, dataArray.Length, SocketFlags.None, address, (ar) => { fSocket.EndReceive(ar); }, null);
                fClient.Send(dataArray, dataArray.Length, address);
            } catch (Exception ex) {
                fLogger.WriteLog("Send(): " + ex.Message);
            }
        }

        #endregion
    }
}
