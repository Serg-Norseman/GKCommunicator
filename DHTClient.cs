using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Torrents;

namespace DHTConnector
{
    public class DHTClient
    {
        public const int KTableSize = 2048;

        private byte[] fBuffer = new byte[65535];
        private IPEndPoint fDefaultIP = new IPEndPoint(IPAddress.Loopback, 0);
        private IPEndPoint fLocalIP;
        private byte[] fLocalID = null;
        private Queue<DHTNode> fNodes = new Queue<DHTNode>();
        //private List<PeerNode> fCommonNodes = new List<PeerNode>();
        private BencodeParser fParser = new BencodeParser();
        private Socket fSocket = null;
        private string fSubnetKey;
        private byte[] fSNKInfoHash;
        private Dictionary<int, DHTMessage> fTransactions = new Dictionary<int, DHTMessage>();
        private readonly DHTRoutingTable fRoutingTable;

        public string SubnetKey
        {
            get { return fSubnetKey; }
            set {
                fSubnetKey = value;

                BDictionary resultDict = new BDictionary();
                resultDict.Add("info", fSubnetKey);
                fSNKInfoHash = TorrentUtil.CalculateInfoHashBytes(resultDict);
            }
        }

        public DHTClient(int port, IPAddress addr)
        {
            fLocalID = DHTHelper.GetRandomID();
            fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            const long IOC_IN = 0x80000000;
            const long IOC_VENDOR = 0x18000000;
            const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            fSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);

            fLocalIP = new IPEndPoint(addr, port);
            fSocket.Bind(fLocalIP);

            fRoutingTable = new DHTRoutingTable(KTableSize);
        }

        public void Run()
        {
            EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
            fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
        }

        public void AddNode(string address, int port)
        {
            fNodes.Enqueue(new DHTNode(null, new IPEndPoint(IPAddress.Parse(address), port)));
        }

        private void EndRecv(IAsyncResult result)
        {
            EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
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
            } while (notsuccess);
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
                                WriteLog("receive `ping` query " + ipinfo.ToString());
                                SendPingResponse(ipinfo, dic.Get<BString>("t"));
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
                WriteLog("OnRecvMessage(): " + ex);
            }
        }

        private void OnRecvErrorX(IPEndPoint ipinfo, BDictionary data)
        {
            var errData = data.Get<BList>("e");

            if (errData != null && errData.Count != 0) {
                var errCode = errData.Get<BNumber>(0);
                var errText = errData.Get<BString>(1);
                WriteLog(">> error receive: " + errCode + " / " + errText);
            }
        }

        private void OnRecvResponseX(IPEndPoint ipinfo, BDictionary data)
        {
            //_kTable.AddOrUpdateNode(responseNode);

            //some nodes send a mirror response with the same version
            //var ver = data.Get<BString>("v");

            var tid = data.Get<BString>("t");
            var returnValues = data.Get<BDictionary>("r");

            var idStr = returnValues.Get<BString>("id");
            var tokStr = returnValues.Get<BString>("token");
            var valuesList = returnValues.Get<BList>("values");
            var nodesStr = returnValues.Get<BString>("nodes");

            // define type of response by transactionId of query/response
            QueryType queryType = QueryType.none;
            if (tid != null && tid.Length == 2) {
                queryType = CheckTransaction(tid);
            } else {
                WriteLog("response without transactionId: " + ipinfo.ToString(), false);
            }

            bool canAnnounce = false;
            switch (queryType) {
                case QueryType.ping:
                    if ((idStr != null && idStr.Length != 0)) {
                        WriteLog(">>>>> ping query responsed and checked: " + ipinfo.ToString());
                    } else {
                        WriteLog(">>> ping response checked");
                    }
                    break;

                case QueryType.find_node:
                    if ((idStr != null && idStr.Length != 0)) {
                        WriteLog(">>>>> find_node query responsed and checked: " + ipinfo.ToString());
                        //ProcessNodesStr(ipinfo, nodesStr);
                    } else {
                        WriteLog(">>> find_node response checked");
                    }
                    break;

                case QueryType.get_peers:
                    if (tokStr != null && tokStr.Length != 0) {
                        WriteLog(">>>>> get_peers query responsed and checked: " + ipinfo.ToString());
                        //ProcessNodesStr(ipinfo, nodesStr);

                        if (!ProcessValuesStr(ipinfo, valuesList)) {
                            canAnnounce = true;
                        }
                    } else {
                        WriteLog(">>>> get_peers response checked");
                    }
                    break;

                case QueryType.announce_peer:
                    if ((idStr != null && idStr.Length != 0)) {
                        WriteLog(">>>>> announce_peer query responsed and checked: " + ipinfo.ToString());
                    } else {
                        WriteLog(">>>> announce_peer response checked: " + ipinfo.ToString());
                    }
                    break;

                case QueryType.none:
                    break;
            }

            ProcessNodesStr(ipinfo, nodesStr);

            if (canAnnounce /*tokStr != null && tokStr.Length != 0*/) {
                //WriteLog("get_peers_token_response!!!: " + ipinfo.ToString());
                SendAnnouncePeerQuery(ipinfo, fSNKInfoHash, /*1, */fLocalIP.Port, tokStr);
                //SendGetPeersQuery(ipinfo, fSNKInfoHash);
                //return;
            }
        }

        private bool ProcessValuesStr(IPEndPoint ipinfo, BList valuesList)
        {
            bool result = false;
            if (valuesList != null && valuesList.Count != 0) {
                // if infohash and peers for it was found
                Console.ForegroundColor = ConsoleColor.Green;
                WriteLog(">>>>>>>>>>>> get_peers_values_response!!!: " + ipinfo.ToString());

                var values = DHTHelper.ParseValuesList(valuesList);

                if (values.Count > 0) {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    WriteLog("receive " + values.Count + " values from " + ipinfo.ToString(), true);

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    WriteLog("send ping " + values[0].EndPoint.ToString(), true);
                    SendPingQuery(values[0].EndPoint);

                    // TODO: handshake and other
                    // TODO: check that there is no current node in the response
                }

                result = true;
            }
            return result;
        }

        private void ProcessNodesStr(IPEndPoint ipinfo, BString nodesStr)
        {
            // bootstrap's trackers not response with values on GetPeers(infohash) ???
            if (nodesStr != null && nodesStr.Length != 0) {
                if (nodesStr.Value.Length % 26 != 0)
                    throw new Exception("sd");

                var nodesData = nodesStr.Value;
                var nodesList = DHTHelper.ParseNodesList(nodesData);
                WriteLog("receive " + nodesList.Count + " nodes from " + ipinfo.ToString(), false);

                /*lock (fCommonNodes) {
                    foreach (var t in nodesList) {
                        if (fCommonNodes.Count < 20) {
                            fCommonNodes.Add(t);
                            WriteLog("added node " + t.EndPoint.ToString(), false);
                        }
                    }
                }*/

                foreach (var t in nodesList) {
                    lock (fNodes) {
                        fNodes.Enqueue(t);
                        fRoutingTable.AddNode(t);
                        //WriteLog("find a node " + t.Item2);
                    }
                }
            }
        }

        private void OnRecvAnnouncePeerQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var info_hash = args.Get<BString>("info_hash");
            int port = 0;
            if (args.ContainsKey("implied_port") && args.Get<BNumber>("implied_port") == 1) {
                port = ipinfo.Port;
            } else
                port = args.Get<BNumber>("port");

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            WriteLog("receive `announce_peer` query " + ipinfo.ToString());

            fRoutingTable.AddNode(new DHTNode(id.Value, ipinfo));

            if (DHTHelper.ArraysEqual(info_hash.Value, fSNKInfoHash)) {
                Console.ForegroundColor = ConsoleColor.Magenta;
                WriteLog("receive `announce_peer` query for our HASH " + ipinfo.ToString());

                // TODO: check node! It's our partner?

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                WriteLog("send ping " + ipinfo.ToString(), true);
                SendPingQuery(ipinfo); // TODO: other response
            }
        }

        private void OnRecvFindNodeQuery(IPEndPoint ipinfo, BDictionary data)
        {
            WriteLog("receive `find_node` query");
        }

        private void OnRecvGetPeersQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");
            var rid = args.Get<BString>("id");
            var info_hash = args.Get<BString>("info_hash");
            var neighbor = DHTHelper.GetNeighbor(info_hash.Value, fLocalID);
            var nodesList = fRoutingTable.FindNodes(info_hash.Value);
            BList values = null;

            WriteLog("receive `get_peers` query from " + ipinfo.ToString() + " [" + rid.Value.ToHexString() + "] for " + info_hash.Value.ToHexString());

            fRoutingTable.AddNode(new DHTNode(rid.Value, ipinfo));

            byte[] nodesArray = new byte[nodesList.Count * 26];
            var i = 0;
            foreach (var node in nodesList) {
                var compact = DHTHelper.CompactNode(node);
                Array.Copy(compact, 0, nodesArray, i * 26, 26);
            }

            if (DHTHelper.ArraysEqual(info_hash.Value, fSNKInfoHash)) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteLog("receive `get_peers` query for our HASH");

                //values = new BList();
                //values.Add(new BString(Helpers.CompactEndPoint(fLocalIP)));
            }

            Send(ipinfo, DHTMessage.CreateGetPeersResponse(t, neighbor, info_hash.Value, values, new BString(nodesArray)));
            //InfoHashList.Add(info_hash.Value);
        }

        public void ReJoin()
        {
            var hosts = new List<string>()
                    {
                            "router.bittorrent.com",
                            "dht.transmissionbt.com",
                            "router.utorrent.com"
                        }.Select(x => Dns.GetHostEntry(x).AddressList[0]).ToList();

            /*lock (fCommonNodes) {
                foreach (var t in hosts) {
                    fCommonNodes.Add(new PeerNode(new IPEndPoint(t, 6881)));
                }
            }*/

            lock (fNodes) {
                foreach (var t in hosts) {
                    fNodes.Enqueue(new DHTNode(new IPEndPoint(t, 6881)));
                }
            }

            /*new Thread(() => {
                while (true) {
                    int count = 0;
                    lock (fNodes) {
                        count = fNodes.Count;
                    }
                    if (count == 0) {
                        foreach (var t in hosts) {
                            SendFindNodeQuery(null, new IPEndPoint(t, 6881));
                        }
                    }
                    Thread.Sleep(3 * 1000);
                }
            }).Start();*/
        }

        public void SendFindNodes()
        {
            Console.ForegroundColor = ConsoleColor.White;
            WriteLog("Search for: " + fSNKInfoHash.ToHexString());

            while (true) {
                DHTNode result = null;
                /*lock (fNodes) {
                    if (fNodes.Count > 0) {
                        result = fNodes.Dequeue();
                    }
                }
                if (result != null) {
                    //SendFindNodeQuery(result.ID, result.EndPoint);

                    SendGetPeersQuery(result.EndPoint, fSNKInfoHash);
                }*/

                lock (fRoutingTable) {
                    var nodes = fRoutingTable.FindNodes(fSNKInfoHash);

                    if (nodes.Count <= 0) {
                        lock (fNodes) {
                            if (fNodes.Count > 0) {
                                result = fNodes.Dequeue();
                            }
                        }
                        if (result != null) {
                            SendGetPeersQuery(result.EndPoint, fSNKInfoHash);
                        }
                    }

                    foreach (var node in nodes) {
                        SendGetPeersQuery(node.EndPoint, fSNKInfoHash);
                    }
                }

                /*foreach (var peerNode in fCommonNodes) {
                    //SendFindNodeQuery(null, peerNode.EndPoint);

                    //SendPingResponse(peerNode.EndPoint, new BString(Helpers.GetTransactionID()));
                    SendGetPeersQuery(peerNode.EndPoint, fSNKInfoHash);
                }*/

                //Thread.Sleep((int)((1 / 50 / 5) * 1000));
                Thread.Sleep(1000);
            }
        }

        public void SetTransaction(BString transactionId, DHTMessage message)
        {
            int tid = BitConverter.ToInt16(transactionId.Value, 0);
            fTransactions[tid] = message;
        }

        public QueryType CheckTransaction(BString transactionId)
        {
            QueryType result = QueryType.none;

            DHTMessage message = null;
            int tid = BitConverter.ToInt16(transactionId.Value, 0);
            if (fTransactions.TryGetValue(tid, out message)) {
                result = message.QueryType;
                fTransactions[tid] = null;
            } else {
                WriteLog(">> error: transaction not found!", false);
            }

            return result;
        }

        #region Queries and responses

        private void SendPingResponse(IPEndPoint address, BString transactionID)
        {
            WriteLog("send `ping` response " + address.ToString());

            Send(address, DHTMessage.CreatePingResponse(transactionID, fLocalID));
        }

        private void SendPingQuery(IPEndPoint address)
        {
            var transactionID = TransactionId.NextId();
            byte[] nid = fLocalID;

            BDictionary sendData = DHTMessage.CreatePingQuery(transactionID, nid);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.ping, sendData));
            Send(address, sendData);
        }

        private void SendFindNodeQuery(byte[] data, IPEndPoint address, byte[] aaa = null, byte[] ttid = null)
        {
            var transactionID = TransactionId.NextId();
            byte[] nid = (data == null) ? fLocalID : DHTHelper.GetNeighbor(data, fLocalID);

            BDictionary sendData = DHTMessage.CreateFindNodeQuery(transactionID, nid);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.find_node, sendData));
            Send(address, sendData);
        }

        public void SendAnnouncePeerQuery(IPEndPoint address, byte[] infoHash, /*byte implied_port, */int port, BString token)
        {
            var transactionID = TransactionId.NextId();
            byte[] nid = fLocalID;

            BDictionary sendData = DHTMessage.CreateAnnouncePeerQuery(transactionID, nid, infoHash, /*implied_port, */port, token);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.announce_peer, sendData));
            Send(address, sendData);
        }

        private void SendGetPeersQuery(IPEndPoint address, byte[] infoHash)
        {
            var transactionID = TransactionId.NextId();
            byte[] nid = fLocalID;

            BDictionary sendData = DHTMessage.CreateGetPeersQuery(transactionID, nid, infoHash);
            SetTransaction(transactionID, new DHTMessage(MsgType.query, QueryType.get_peers, sendData));
            Send(address, sendData);
        }

        private void Send(IPEndPoint address, BDictionary data)
        {
            try {
                byte[] dataArray = data.EncodeAsBytes();
                fSocket.BeginSendTo(dataArray, 0, dataArray.Length, SocketFlags.None, address, (ar) => { fSocket.EndReceive(ar); }, null);
            } catch (Exception ex) {
                WriteLog("Send(): " + ex.Message, false);
            }
        }

        #endregion

        #region Other

        private void WriteLog(string str, bool display = true)
        {
            if (display) {
                Console.WriteLine(str);
                Console.ResetColor();
            }

            var fswriter = new StreamWriter(new FileStream("./logFile", FileMode.Append));
            fswriter.WriteLine(str);
            fswriter.Flush();
            fswriter.Close();
        }

        private void WriteInfo(string commandName, byte[] info_hash, byte[] nid, IPEndPoint ipadd)
        {
            /*var str = $"{commandName}:{info_hash.ToHexString()}:{nid.ToHexString()}:{(ipadd ?? fDefaultIP).ToString()}";
            var fswriter = new StreamWriter(new FileStream("./logFile", FileMode.Append));
            fswriter.WriteLine(str);
            fswriter.Flush();
            fswriter.Close();*/
        }

        #endregion
    }
}
