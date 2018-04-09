using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DHTConnector
{
    public class UDPServer
    {
        private static byte[] GKN_INFOHASH = CreateGKNInfoHash();

        private byte[] fBuffer = new byte[65535];
        private IPEndPoint fDefaultIP = new IPEndPoint(IPAddress.Loopback, 0);
        private byte[] fLocalID = null;
        private Queue<Tuple<byte[], IPEndPoint>> fNodes = new Queue<Tuple<byte[], IPEndPoint>>();
        private BencodeParser fParser = new BencodeParser();
        private Socket fSocket = null;

        private static byte[] CreateGKNInfoHash()
        {
            BDictionary resultDict = new BDictionary();
            resultDict.Add("info", Program.NETWORK_SIGN);

            return TorrentUtil.CalculateInfoHashBytes(resultDict);
        }

        public UDPServer(int port, IPAddress addr)
        {
            fLocalID = GetRandomID();
            fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            const long IOC_IN = 0x80000000;
            const long IOC_VENDOR = 0x18000000;
            const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            fSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
            fSocket.Bind(new IPEndPoint(addr, port));
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
                //Console.WriteLine(ex.ToString());
            }
            bool notsuccess = false;
            do {
                try {
                    fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
                    notsuccess = false;
                } catch (Exception ex) {
                    Console.WriteLine(ex);
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

                // on receive request
                if (dic.Get<BString>("y") == "q") {
                    if (dic.Get<BString>("q") == "ping") {
                        SendPingResponse(ipinfo, dic.Get<BString>("t"));
                        return;
                    }

                    if (dic.Get<BString>("q") == "find_node") {
                    }

                    if (dic.Get<BString>("q") == "get_peers") {
                        OnRecvRequestGetPeers(ipinfo, dic);
                        return;
                    }

                    if (dic.Get<BString>("q") == "announce_peer") {
                        OnRecvRequestAnnouncePeer(ipinfo, dic);
                        return;
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private void OnRecvResponseX(IPEndPoint ipinfo, BDictionary data)
        {
            var respNode = data.Get<BDictionary>("r");
            var nodesStr = respNode.Get<BString>("nodes");
            if (nodesStr == null || nodesStr.Length == 0) {
                //Console.WriteLine("ping response " + ipinfo.ToString());
                return;
            }
            if (nodesStr.Value.Count % 26 != 0)
                throw new Exception("sd");
            var nodesData = nodesStr.Value.ToArray();
            var nodesList = Helpers.ParseNodesList(nodesData);
            foreach (var t in nodesList) {

                // temp limit
                int count = 0;
                lock (fNodes) {
                    count = fNodes.Count;
                }
                if (count == 100) return;

                lock (fNodes) {
                    fNodes.Enqueue(t);
                    //Console.WriteLine("find a node " + t.Item2);
                }
            }
            return;
        }

        private void OnRecvRequestAnnouncePeer(IPEndPoint ipinfo, BDictionary data)
        {
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var info_hash = args.Get<BString>("info_hash");
            int port = 0;
            if (args.ContainsKey("implied_port") && args.Get<BNumber>("implied_port") == 1) {
                port = ipinfo.Port;
            } else
                port = args.Get<BNumber>("port");

            //CanDownload.Add(new Tuple<byte[], IPEndPoint>(info_hash.Value.ToArray(), new IPEndPoint(ipinfo.Address, port)));
            Console.WriteLine("find a hash_info_candownload!!-------------------------------------");
            WriteInfo("announce_peer", info_hash.Value.ToArray(), id.Value.ToArray(), new IPEndPoint(ipinfo.Address, port));
        }

        private void OnRecvRequestGetPeers(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var a = data.Get<BDictionary>("a");
            var rid = a.Get<BString>("id");
            var info_hash = a.Get<BString>("info_hash");

            var result = new BDictionary();
            result.Add("t", t);
            result.Add("y", "r");
            var r = new BDictionary();
            var neighbor = new List<byte>();
            neighbor.AddRange(info_hash.Value.Take(10));
            neighbor.AddRange(fLocalID.Skip(10).Take(10));
            r.Add("id", new BString(neighbor));
            r.Add("token", new BString(info_hash.Value.Take(2)));
            r.Add("nodes", "");
            result.Add("r", r);

            Send(ipinfo, result);

            WriteInfo("get_peers", info_hash.Value.ToArray(), rid.Value.ToArray(), null);
            Console.WriteLine(info_hash.Value.ToArray().ToHexString() + "|" + ipinfo.ToString());
            //InfoHashList.Add(info_hash.Value.ToArray());
        }

        private void SendPingResponse(IPEndPoint address, BString transactionID)
        {
            BDictionary resultDict = new BDictionary();
            resultDict.Add("y", "r");
            resultDict.Add("t", transactionID);
            var r = new BDictionary();
            r.Add("id", new BString(fLocalID));
            resultDict.Add("r", r);

            Send(address, resultDict);
        }

        private void WriteInfo(string commandName, byte[] info_hash, byte[] nid, IPEndPoint ipadd)
        {
            var str = $"{commandName}:{info_hash.ToHexString()}:{nid.ToHexString()}:{(ipadd ?? fDefaultIP).ToString()}";
            var fswriter = new StreamWriter(new FileStream("./logFile", FileMode.Append));
            fswriter.WriteLine(str);
            fswriter.Flush();
            fswriter.Close();
        }

        public void ReJoin()
        {
            new Thread(() => {
                var hosts = new List<string>()
                        {
                            "router.bittorrent.com",
                            "dht.transmissionbt.com",
                            "router.utorrent.com"
                        }.Select(x => Dns.Resolve(x).AddressList[0]).ToList();
                while (true) {
                    int count = 0;
                    lock (fNodes) {
                        count = fNodes.Count;
                    }
                    if (count == 0) {
                        foreach (var t in hosts) {
                            SendFindNode(null, new IPEndPoint(t, 6881));
                        }
                    }
                    Thread.Sleep(3 * 1000);
                }
            }).Start();
        }

        public void SendFindNodes()
        {
            while (true) {
                Tuple<byte[], IPEndPoint> result = null;
                lock (fNodes) {
                    if (fNodes.Count > 0) {
                        result = fNodes.Dequeue();
                    }
                }
                if (result != null) {
                    SendFindNode(result.Item1, result.Item2);
                }

                //Thread.Sleep((int)((1 / 50 / 5) * 1000));
                Thread.Sleep(1000);
            }
        }

        private static byte[] GetNeighbor(byte[] target, byte[] nid)
        {
            var result = new byte[20];
            for (int i = 0; i < 10; i++)
                result[i] = target[i];
            for (int i = 10; i < 20; i++)
                result[i] = nid[i];
            return result;
        }

        public void SendAnnounceGKNPeer(IPEndPoint address)
        {
            SendAnnouncePeerRequest(address, GKN_INFOHASH, "");
        }

        public void SendAnnouncePeerRequest(IPEndPoint address, byte[] infoHash, string token)
        {
            SendAnnouncePeerRequest(address, infoHash, 1, 0, token);
        }

        public void SendAnnouncePeerRequest(IPEndPoint address, byte[] infoHash, byte implied_port, ushort port, string token)
        {
            byte[] nid = fLocalID;
            var transactionID = Helpers.GetTransactionID();

            BDictionary sendData = new BDictionary();
            sendData.Add("t", new BString(transactionID));
            sendData.Add("y", "q");
            sendData.Add("q", "announce_peer");

            var args = new BDictionary();
            args.Add("id", new BString(nid));
            args.Add("implied_port", new BNumber(implied_port));
            args.Add("info_hash", new BString(infoHash));
            args.Add("port", new BNumber(port));
            args.Add("token", new BString(token));
            sendData.Add("a", args);

            Send(address, sendData);
        }

        private void SendFindNode(byte[] data, IPEndPoint address, byte[] aaa = null, byte[] ttid = null)
        {
            byte[] nid = null;
            if (data == null) {
                nid = fLocalID;
            } else {
                nid = GetNeighbor(data, fLocalID);
            }

            var transactionID = Helpers.GetTransactionID();

            BDictionary sendData = new BDictionary();
            sendData.Add("t", new BString(transactionID));
            sendData.Add("y", "q");
            sendData.Add("q", "find_node");
            var a = new BDictionary();
            a.Add("id", new BString(nid));
            a.Add("target", new BString(Helpers.GetRandomID()));
            sendData.Add("a", a);

            Send(address, sendData);
        }

        private void Send(IPEndPoint address, BDictionary data)
        {
            try {
                var dataArray = data.EncodeAsBytes();
                fSocket.BeginSendTo(dataArray, 0, dataArray.Length, SocketFlags.None, address, (ar) => { fSocket.EndReceive(ar); }, null);
            } catch (Exception ex) {
                Console.WriteLine("Send(): " + ex.Message);
            }
        }

        static byte[] GetRandomID()
        {
            var r = new Random();
            byte[] result = new byte[20];
            r.NextBytes(result);
            return result;
        }
    }
}
