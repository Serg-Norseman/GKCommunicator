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
    public enum MsgType
    {
        query, response, error
    }

    public enum QueryType
    {
        ping, find_node, get_peers, announce_peer
    }

    public class UDPServer
    {
        private byte[] fBuffer = new byte[65535];
        private IPEndPoint fDefaultIP = new IPEndPoint(IPAddress.Loopback, 0);
        private IPEndPoint fLocalIP;
        private byte[] fLocalID = null;
        //private Queue<Tuple<byte[], IPEndPoint>> fNodes = new Queue<Tuple<byte[], IPEndPoint>>();
        private List<PeerNode> fCommonNodes = new List<PeerNode>();
        private BencodeParser fParser = new BencodeParser();
        private Socket fSocket = null;
        private string fSubnetKey;
        private byte[] fSNKInfoHash;

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

        public UDPServer(int port, IPAddress addr)
        {
            fLocalID = Helpers.GetRandomID();
            fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            const long IOC_IN = 0x80000000;
            const long IOC_VENDOR = 0x18000000;
            const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            fSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);

            fLocalIP = new IPEndPoint(addr, port);
            fSocket.Bind(fLocalIP);
        }

        public void Run()
        {
            EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
            fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
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

                // on receive response
                if (dic.Get<BString>("y") == "r") {
                    OnRecvResponseX(ipinfo, dic);
                    return;
                }

                // on receive query
                if (dic.Get<BString>("y") == "q") {
                    if (dic.Get<BString>("q") == "ping") {
                        WriteLog("receive ping query " + ipinfo.ToString(), false);

                        SendPingResponse(ipinfo, dic.Get<BString>("t"));
                        return;
                    }

                    if (dic.Get<BString>("q") == "find_node") {
                        WriteLog("receive find_node query");
                    }

                    if (dic.Get<BString>("q") == "get_peers") {
                        WriteLog("receive get_peers query");
                        OnRecvGetPeersQuery(ipinfo, dic);
                        return;
                    }

                    if (dic.Get<BString>("q") == "announce_peer") {
                        WriteLog("receive announce_peer query");
                        OnRecvAnnouncePeerQuery(ipinfo, dic);
                        return;
                    }
                }

                // on receive error
                if (dic.Get<BString>("y") == "e") {
                    OnRecvErrorX(ipinfo, dic);
                    return;
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
                WriteLog("-------- error receive: " + errCode + " / " + errText);
            }
        }

        private void OnRecvResponseX(IPEndPoint ipinfo, BDictionary data)
        {
            var returnValues = data.Get<BDictionary>("r");

            var tokStr = returnValues.Get<BString>("token");
            if (tokStr != null && tokStr.Length != 0) {
                WriteLog("get_peers_token_response!!!: " + ipinfo.ToString());
                SendAnnouncePeerQuery(ipinfo, fSNKInfoHash, 1, fLocalIP.Port, tokStr);
                //SendGetPeersQuery(ipinfo, fSNKInfoHash);
                //return;
            }

            var valuesStr = returnValues.Get<BString>("values");
            if (valuesStr != null && valuesStr.Length != 0) {
                // if infohash and peers for it was found
                WriteLog(">>>>> get_peers_response!!!: " + ipinfo.ToString());
                //return;
            }

            var nodesStr = returnValues.Get<BString>("nodes");
            if (nodesStr == null || nodesStr.Length == 0) {
                WriteLog("ping response: " + ipinfo.ToString());
                return;
            }

            if (nodesStr.Value.Length % 26 != 0)
                throw new Exception("sd");

            var nodesData = nodesStr.Value;
            var nodesList = Helpers.ParseNodesList(nodesData);
            WriteLog("OnRecvResponseX(" + ipinfo.ToString() + "): find nodes " + nodesList.Count);

            // bootstrap's trackers not response with values on GetPeers(infohash) ???
            lock (fCommonNodes) {
                if (fCommonNodes.Count < 20) {
                    foreach (var t in nodesList) {
                        fCommonNodes.Add(t);
                        WriteLog("find a node " + t.EndPoint.ToString());
                    }
                }
            }

            /*foreach (var t in nodesList) {
                lock (fNodes) {
                    fNodes.Enqueue(t);
                    WriteLog("find a node " + t.Item2);
                }
            }*/

            return;
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

            //CanDownload.Add(new Tuple<byte[], IPEndPoint>(info_hash.Value, new IPEndPoint(ipinfo.Address, port)));
            WriteLog("find a torrent's hashinfo");
            WriteInfo("announce_peer", info_hash.Value, id.Value, new IPEndPoint(ipinfo.Address, port));
        }

        private void OnRecvGetPeersQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");
            var rid = args.Get<BString>("id");
            var info_hash = args.Get<BString>("info_hash");

            var neighbor = Helpers.GetNeighbor(info_hash.Value, fLocalID);

            Send(ipinfo, Helpers.CreateGetPeersResponse(t, neighbor, info_hash.Value));

            WriteInfo("get_peers", info_hash.Value, rid.Value, null);
            WriteLog("receive get_peers: " + info_hash.Value.ToHexString() + "|" + ipinfo.ToString());
            //InfoHashList.Add(info_hash.Value);
        }

        private void SendPingResponse(IPEndPoint address, BString transactionID)
        {
            WriteLog("send ping response " + address.ToString(), false);

            Send(address, Helpers.CreatePingResponse(transactionID, fLocalID));
        }

        private void WriteLog(string str, bool display = true)
        {
            if (display) {
                Console.WriteLine(str);
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

        public void ReJoin()
        {
            var hosts = new List<string>()
                    {
                            "router.bittorrent.com",
                            "dht.transmissionbt.com",
                            "router.utorrent.com"
                        }.Select(x => Dns.GetHostEntry(x).AddressList[0]).ToList();

            lock (fCommonNodes) {
                foreach (var t in hosts) {
                    fCommonNodes.Add(new PeerNode(new IPEndPoint(t, 6881)));
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
            while (true) {
                /*Tuple<byte[], IPEndPoint> result = null;
                lock (fNodes) {
                    if (fNodes.Count > 0) {
                        result = fNodes.Dequeue();
                    }
                }
                if (result != null) {
                    SendFindNodeQuery(result.Item1, result.Item2);

                    SendGetPeersQuery(result.Item2, fSNKInfoHash);
                }*/

                foreach (var peerNode in fCommonNodes) {
                    //SendFindNodeQuery(null, peerNode.EndPoint);

                    //SendPingResponse(peerNode.EndPoint, new BString(Helpers.GetTransactionID()));
                    SendGetPeersQuery(peerNode.EndPoint, fSNKInfoHash);
                }

                //Thread.Sleep((int)((1 / 50 / 5) * 1000));
                Thread.Sleep(5000);
            }
        }

        private void SendFindNodeQuery(byte[] data, IPEndPoint address, byte[] aaa = null, byte[] ttid = null)
        {
            var transactionID = TransactionId.NextId();
            byte[] nid = (data == null) ? fLocalID : Helpers.GetNeighbor(data, fLocalID);
            Send(address, Helpers.CreateFindNodeQuery(transactionID, nid));
        }

        public void SendAnnouncePeerQuery(IPEndPoint address, byte[] infoHash, byte implied_port, int port, BString token)
        {
            var transactionID = TransactionId.NextId();
            byte[] nid = fLocalID;
            Send(address, Helpers.CreateAnnouncePeerQuery(transactionID, nid, infoHash, implied_port, port, token));
        }

        private void SendGetPeersQuery(IPEndPoint address, byte[] infoHash)
        {
            var transactionID = TransactionId.NextId();
            byte[] nid = fLocalID;
            Send(address, Helpers.CreateGetPeersQuery(transactionID, nid, infoHash));
        }

        private void Send(IPEndPoint address, byte[] data)
        {
            try {
                fSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, address, (ar) => { fSocket.EndReceive(ar); }, null);
            } catch (Exception ex) {
                WriteLog("Send(): " + ex.Message);
            }
        }
    }
}
