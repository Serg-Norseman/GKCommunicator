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
using BencodeNET;
using BSLib;
using GKNet.Logging;

namespace GKNet.DHT
{
    public sealed class DHTClient
    {
        private static readonly TimeSpan NodesUpdateRange = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan AnnounceLife = TimeSpan.FromMinutes(10); // FIXME?

        #if !IP6
        public static readonly IPAddress IPLoopbackAddress = IPAddress.Loopback;
        public static readonly IPAddress IPAnyAddress = IPAddress.Any;
        public static readonly AddressFamily IPAddressFamily = AddressFamily.InterNetwork;
        #else
        public static readonly IPAddress IPLoopbackAddress = IPAddress.IPv6Loopback;
        public static readonly IPAddress IPAnyAddress = IPAddress.IPv6Any;
        public static readonly AddressFamily IPAddressFamily = AddressFamily.InterNetworkV6;
        #endif

        public const int PublicDHTPort = 6881;
        public const int KTableSize = 2048;

        private byte[] fBuffer;
        private long fLastNodesUpdateTime;
        private byte[] fLocalID;
        private IPEndPoint fPublicEndPoint;
        private IList<IPAddress> fRouters;
        private byte[] fSearchInfoHash;
        private bool fSearchRunned;

        private readonly string fClientVer;
        private readonly IPEndPoint fDefaultIP;
        private readonly IPEndPoint fLocalIP;
        private readonly ILogger fLogger;
        private readonly IDHTPeersHolder fPeersHolder;
        private readonly DHTRoutingTable fRoutingTable;
        private readonly Socket fSocket;
        private readonly Dictionary<int, DHTMessage> fTransactions;

        public byte[] LocalID
        {
            get { return fLocalID; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return fLocalIP; }
        }

        public IPEndPoint PublicEndPoint
        {
            get { return fPublicEndPoint; }
            set { fPublicEndPoint = value; }
        }

        public Socket Socket
        {
            get { return fSocket; }
        }

        public event EventHandler<PeersFoundEventArgs> PeersFound;
        public event EventHandler<PeerPingedEventArgs> PeerPinged;

        public event EventHandler<MessageEventArgs> QueryReceived;
        public event EventHandler<MessageEventArgs> ResponseReceived;

        public DHTClient(IPAddress addr, int port, IDHTPeersHolder peersHolder, string clientVer)
        {
            fBuffer = new byte[65535];
            fClientVer = clientVer;
            fDefaultIP = new IPEndPoint(IPLoopbackAddress, 0);
            fLocalID = DHTHelper.GetRandomID();
            fLocalIP = new IPEndPoint(addr, port);
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "DHTClient");
            fPeersHolder = peersHolder;
            fRoutingTable = new DHTRoutingTable(KTableSize);
            fSearchRunned = false;
            fTransactions = new Dictionary<int, DHTMessage>();

            fSocket = new Socket(IPAddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // FIXME: unsupported?
            #if !__MonoCS__
            fSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            #if !IP6
            const long IOC_IN = 0x80000000;
            const long IOC_VENDOR = 0x18000000;
            const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            fSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
            #else
            #endif
            #endif

            #if !IP6
            fSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            fSocket.Ttl = 255;
            #else
            fSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            #endif

            fSocket.Bind(fLocalIP);
        }

        public void Run()
        {
            BeginRecv();
        }

        public void Stop()
        {
            fSearchRunned = false;
            fSocket.Close();
        }

        public void JoinNetwork()
        {
            var routers = new List<string>() {
                "router.bittorrent.com",
                "dht.transmissionbt.com",
                "router.utorrent.com"
            }.Select(x => Dns.GetHostEntry(x).AddressList[0]).ToList();

            #if !IP6
            fRouters = routers;
            #else
            fRouters = new List<IPAddress>();
            foreach (var r in routers) {
                fRouters.Add(DHTHelper.PrepareAddress(r));
            }
            #endif
        }

        public void SearchNodes(byte[] searchInfoHash)
        {
            fLogger.WriteDebug("Search for: {0}", searchInfoHash.ToHexString());

            fSearchInfoHash = searchInfoHash;
            fSearchRunned = true;

            new Thread(() => {
                while (fSearchRunned) {
                    int count = 0;
                    lock (fRoutingTable) {
                        // if the routing table has not been updated for more
                        // than a minute - reset routing table
                        long nowTicks = DateTime.Now.Ticks;
                        if (nowTicks - fLastNodesUpdateTime > NodesUpdateRange.Ticks) {
                            fRoutingTable.Clear();
                            fLogger.WriteDebug("DHT reset routing table");
                        }

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

        private void BeginRecv()
        {
            EndPoint remoteAddress = new IPEndPoint(IPAnyAddress, 0);
            fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
        }

        private void EndRecv(IAsyncResult result)
        {
            try {
                EndPoint remoteAddress = new IPEndPoint(IPLoopbackAddress, 0);
                int count = fSocket.EndReceiveFrom(result, ref remoteAddress);
                if (count > 0) {
                    byte[] buffer = new byte[count];
                    Buffer.BlockCopy(fBuffer, 0, buffer, 0, count);
                    OnRecvMessage((IPEndPoint)remoteAddress, buffer);
                }
            } catch (Exception ex) {
                //fLogger.WriteError("DHTClient.EndRecv.1()", ex);
            }

            bool notsuccess = false;
            do {
                try {
                    BeginRecv();
                    notsuccess = false;
                } catch (Exception ex) {
                    //fLogger.WriteError("DHTClient.EndRecv.2()", ex);
                    notsuccess = true;
                }
            } while (notsuccess);
        }

        private void OnRecvMessage(IPEndPoint ipinfo, byte[] data)
        {
            try {
                DHTMessage msg = DHTMessage.ParseBuffer(data);
                if (msg == null) return;

                if (msg.IsSimilarTo(fClientVer)) {
                    fLogger.WriteDebug(">>>> Received a message from a similar client from {0}", ipinfo);
                }

                switch (msg.Type) {
                    case MessageType.Response:
                        OnRecvResponseX(ipinfo, msg as DHTResponseMessage);
                        break;

                    case MessageType.Query:
                        OnRecvQueryX(ipinfo, msg as DHTQueryMessage);
                        break;

                    case MessageType.Error:
                        OnRecvErrorX(ipinfo, msg as DHTErrorMessage);
                        break;
                }
            } catch (Exception ex) {
                fLogger.WriteError("DHTClient.OnRecvMessage()", ex);
            }
        }

        private void OnRecvErrorX(IPEndPoint ipinfo, DHTErrorMessage msg)
        {
            if (msg == null) return;

            fLogger.WriteDebug("DHT error receive: {0} / {1}", msg.ErrCode, msg.ErrText);
        }

        private void OnRecvQueryX(IPEndPoint ipinfo, DHTQueryMessage msg)
        {
            if (msg == null) return;

            BDictionary data = msg.Data;

            switch (msg.QueryType) {
                case QueryType.Ping:
                    OnRecvPingQuery(ipinfo, data);
                    break;

                case QueryType.FindNode:
                    OnRecvFindNodeQuery(ipinfo, data);
                    break;

                case QueryType.GetPeers:
                    OnRecvGetPeersQuery(ipinfo, data);
                    break;

                case QueryType.AnnouncePeer:
                    OnRecvAnnouncePeerQuery(ipinfo, data);
                    break;

                default:
                    var args = data.Get<BDictionary>("a");
                    if (args != null) {
                        var id = args.Get<BString>("id");
                        byte[] idVal = (id != null) ? id.Value : null;
                        RaiseQueryReceived(ipinfo, idVal, data);
                    }
                    break;
            }
        }

        private void OnRecvResponseX(IPEndPoint ipinfo, DHTResponseMessage msg)
        {
            if (msg == null) return;

            BDictionary data = msg.Data;

            var tid = data.Get<BString>("t");
            var returnValues = data.Get<BDictionary>("r");

            BString id = null;
            BString tokStr = null;
            BList valuesList = null;
            BString nodesStr = null;

            if (returnValues != null) {
                id = returnValues.Get<BString>("id");
                tokStr = returnValues.Get<BString>("token");
                valuesList = returnValues.Get<BList>("values");
                nodesStr = returnValues.Get<BString>("nodes");
            }

            if (id == null || id.Length == 0) {
                // response is invalid
                return;
            }

            fRoutingTable.UpdateNode(new DHTNode(id.Value, ipinfo));

            // according to bep_????, most types of response contain a list of nodes
            ProcessNodesStr(ipinfo, nodesStr);

            // define type of response by transactionId of query/response
            QueryType queryType = CheckTransaction(tid);

            bool canAnnounce = false;
            switch (queryType) {
                case QueryType.Ping:
                    RaisePeerPinged(ipinfo, id.Value);
                    break;

                case QueryType.FindNode:
                    break;

                case QueryType.GetPeers:
                    if (tokStr != null && tokStr.Length != 0) {
                        if (!ProcessValuesStr(ipinfo, valuesList)) {
                            canAnnounce = true;
                        }
                    }
                    break;

                case QueryType.AnnouncePeer:
                    break;

                case QueryType.None:
                    // TransactionId bad or unknown
                    RaiseResponseReceived(ipinfo, id.Value, data);
                    break;
            }

            if (canAnnounce) {
                SendAnnouncePeerQuery(ipinfo, fSearchInfoHash, 1, fPublicEndPoint.Port, tokStr);
            }
        }

        private bool ProcessValuesStr(IPEndPoint ipinfo, BList valuesList)
        {
            bool result = false;
            if (valuesList != null && valuesList.Count != 0) {
                var values = DHTHelper.ParseValuesList(valuesList);

                if (values.Count > 0) {
                    fLogger.WriteDebug("Receive {0} values (peers) from {1}", values.Count, ipinfo.ToString());
                    RaisePeersFound(fSearchInfoHash, values);
                    result = true;
                }
            }
            return result;
        }

        private void ProcessNodesStr(IPEndPoint ipinfo, BString nodesStr)
        {
            if (nodesStr != null && nodesStr.Length != 0) {
                List<DHTNode> nodesList = null;
                if (nodesStr.Value.Length % DHTHelper.CompactNodeRecordLengthIP4 == 0) {
                    nodesList = DHTHelper.ParseNodesListIP4(nodesStr.Value);
                } else if (nodesStr.Value.Length % DHTHelper.CompactNodeRecordLengthIP6 == 0) {
                    nodesList = DHTHelper.ParseNodesListIP6(nodesStr.Value);
                } else {
                    throw new Exception("sd");
                }

                fLogger.WriteDebug("Receive {0} nodes from {1}", nodesList.Count, ipinfo.ToString());

                if (nodesList.Count > 0) {
                    fRoutingTable.UpdateNodes(nodesList);
                    fLastNodesUpdateTime = DateTime.Now.Ticks;
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

            fLogger.WriteDebug("Receive `announce_peer` query from {0} [{1}] for {2}", ipinfo.ToString(), id.Value.ToHexString(), infoHash.Value.ToHexString());

            fRoutingTable.UpdateNode(new DHTNode(id.Value, ipinfo));

            if (!Algorithms.ArraysEqual(infoHash.Value, fSearchInfoHash)) {
                // skip response for another infohash query
                return;
            }

            // receive `announce_peer` query for our infohash
            var nodesList = fRoutingTable.FindNodes(infoHash.Value);
            Send(ipinfo, DHTMessage.CreateAnnouncePeerResponse(t, fLocalID, nodesList));
        }

        private void OnRecvPingQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");

            fLogger.WriteDebug("Receive `ping` query from {0} [{1}]", ipinfo.ToString(), id.Value.ToHexString());

            fRoutingTable.UpdateNode(new DHTNode(id.Value, ipinfo));

            Send(ipinfo, DHTMessage.CreatePingResponse(t, fLocalID));
        }

        private void OnRecvFindNodeQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var target = args.Get<BString>("target");

            fLogger.WriteDebug("Receive `find_node` query from {0} [{1}]", ipinfo.ToString(), id.Value.ToHexString());

            fRoutingTable.UpdateNode(new DHTNode(id.Value, ipinfo));

            var nodesList = fRoutingTable.FindNodes(target.Value);
            Send(ipinfo, DHTMessage.CreateFindNodeResponse(t, fLocalID, nodesList));
        }

        private void OnRecvGetPeersQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var infoHash = args.Get<BString>("info_hash");

            fLogger.WriteDebug("Receive `get_peers` query from {0} [{1}] for {2}", ipinfo.ToString(), id.Value.ToHexString(), infoHash.Value.ToHexString());

            fRoutingTable.UpdateNode(new DHTNode(id.Value, ipinfo));

            var neighbor = DHTHelper.GetNeighbor(infoHash.Value, fLocalID);
            var peersList = (Algorithms.ArraysEqual(infoHash.Value, fSearchInfoHash)) ? fPeersHolder.GetPeersList() : null;
            var nodesList = fRoutingTable.FindNodes(infoHash.Value);
            Send(ipinfo, DHTMessage.CreateGetPeersResponse(t, neighbor, infoHash.Value, peersList, nodesList));
        }

        #endregion

        #region Events processing

        private void RaisePeersFound(byte[] infoHash, List<IPEndPoint> peers)
        {
            if (PeersFound != null) {
                PeersFound(this, new PeersFoundEventArgs(infoHash, peers));
            }
        }

        private void RaisePeerPinged(IPEndPoint peerEndPoint, byte[] nodeId)
        {
            if (PeerPinged != null) {
                PeerPinged(this, new PeerPingedEventArgs(peerEndPoint, nodeId));
            }
        }

        private void RaiseQueryReceived(IPEndPoint peerEndPoint, byte[] nodeId, BDictionary data)
        {
            if (QueryReceived != null) {
                QueryReceived(this, new MessageEventArgs(peerEndPoint, nodeId, data));
            }
        }

        private void RaiseResponseReceived(IPEndPoint peerEndPoint, byte[] nodeId, BDictionary data)
        {
            if (ResponseReceived != null) {
                ResponseReceived(this, new MessageEventArgs(peerEndPoint, nodeId, data));
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
            QueryType result = QueryType.None;

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

        internal void SendPingQuery(IPEndPoint address)
        {
            fLogger.WriteDebug("Send peer ping {0}", address);

            var transactionID = DHTHelper.GetTransactionId();
            var msg = DHTMessage.CreatePingQuery(transactionID, fLocalID);
            SetTransaction(transactionID, msg);
            Send(address, msg);
        }

        internal void SendFindNodeQuery(IPEndPoint address, byte[] data)
        {
            var transactionID = DHTHelper.GetTransactionId();
            byte[] nid = (data == null) ? fLocalID : DHTHelper.GetNeighbor(data, fLocalID);

            var msg = DHTMessage.CreateFindNodeQuery(transactionID, nid);
            SetTransaction(transactionID, msg);
            Send(address, msg);
        }

        internal void SendAnnouncePeerQuery(IPEndPoint address, byte[] infoHash, byte implied_port, int port, BString token)
        {
            DHTNode node = fRoutingTable.FindNode(address);
            if (node != null) {
                long nowTicks = DateTime.Now.Ticks;
                if (nowTicks - node.LastAnnouncementTime < AnnounceLife.Ticks) {
                    return;
                }
                node.LastAnnouncementTime = nowTicks;
                // TODO: update announce by timer, every 30m
            }

            var transactionID = DHTHelper.GetTransactionId();
            var msg = DHTMessage.CreateAnnouncePeerQuery(transactionID, fLocalID, infoHash, implied_port, port, token);
            SetTransaction(transactionID, msg);
            Send(address, msg);
        }

        internal void SendGetPeersQuery(IPEndPoint address, byte[] infoHash)
        {
            var transactionID = DHTHelper.GetTransactionId();
            var msg = DHTMessage.CreateGetPeersQuery(transactionID, fLocalID, infoHash);
            SetTransaction(transactionID, msg);
            Send(address, msg);
        }

        public void Send(IPEndPoint address, DHTMessage msg)
        {
            Send(address, msg.Data);
        }

        public void Send(IPEndPoint address, BDictionary data)
        {
            try {
                // according to bep_0005
                data.Add("v", fClientVer);

                byte[] dataArray = data.EncodeAsBytes();
                fSocket.BeginSendTo(dataArray, 0, dataArray.Length, SocketFlags.None, address, (ar) => {
                    try {
                        fSocket.EndReceive(ar);
                    } catch (Exception ex) {
                        fLogger.WriteError("Send.1(" + address.ToString() + ")", ex);
                    }
                }, null);
            } catch (Exception ex) {
                fLogger.WriteError("Send()", ex);
            }
        }

        #endregion
    }
}
