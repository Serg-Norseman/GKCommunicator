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

//#define DEBUG_DHT_INTERNALS

using System;
using System.Collections.Generic;
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
        private static readonly TimeSpan GetPeersRange = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan AnnounceLife = TimeSpan.FromMinutes(10); // in question, from articles

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

        public DHTRoutingTable RoutingTable
        {
            get { return fRoutingTable; }
        }

        public Socket Socket
        {
            get { return fSocket; }
        }

        public event EventHandler<PeersFoundEventArgs> PeersFound;
        public event EventHandler<PeerPingedEventArgs> PeerPinged;

        public event EventHandler<DataEventArgs> DataReceived;
        public event EventHandler<MessageEventArgs> QueryReceived;
        public event EventHandler<MessageEventArgs> ResponseReceived;

        public DHTClient(IPAddress addr, int port, IDHTPeersHolder peersHolder, string clientVer)
        {
            fLocalID = peersHolder.ClientNodeId;

            fBuffer = new byte[65535];
            fClientVer = clientVer;
            fLocalIP = new IPEndPoint(addr, port);
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "DHTClient");
            fPeersHolder = peersHolder;
            fRoutingTable = new DHTRoutingTable(KTableSize);
            fSearchRunned = false;
            fTransactions = new Dictionary<int, DHTMessage>();

            fSocket = new Socket(IPAddressFamily, SocketType.Dgram, ProtocolType.Udp);
            fSocket.SetIPProtectionLevelUnrestricted();

            /*
            // FIXME: unsupported?
            #if !MONO
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
            */

            #if !IP6
            fSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            fSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            fSocket.Ttl = 255;
            #else
            fSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            #endif

            fSocket.Bind(fLocalIP);
        }

        public void Start(byte[] searchInfoHash)
        {
            fSearchRunned = true;
            BeginRecv();
            Bootstrap();
            SearchNodes(searchInfoHash);
        }

        public void Stop()
        {
            fSearchRunned = false;
            fSocket.Close();
        }

        private void Bootstrap()
        {
            var routers = new List<string>() {
                "dht.aelitis.com",
                "dht.transmissionbt.com",
                "router.bittorrent.com",
                "router.utorrent.com",
            };

            fRouters = new List<IPAddress>();
            foreach (var r in routers) {
                IPAddress addr;
                try {
                    addr = Dns.GetHostEntry(r).AddressList[0];
                } catch (Exception ex) {
                    fLogger.WriteError("Bootstrap(" + r + ")", ex);
                    addr = null;
                }

                if (addr != null) {
                    fRouters.Add(DHTHelper.PrepareAddress(addr));
                }
            }
        }

        private void SearchNodes(byte[] searchInfoHash)
        {
            fLogger.WriteDebug("Search for: {0}", searchInfoHash.ToHexString());

            fSearchInfoHash = searchInfoHash;

            new Thread(() => {
                while (fSearchRunned) {
                    long nowTicks = DateTime.Now.Ticks;

                    int count = 0;
                    lock (fRoutingTable) {
                        // if the routing table has not been updated for more
                        // than a minute - reset routing table
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
                            if (nowTicks - node.LastGetPeersTime > GetPeersRange.Ticks) {
                                SendGetPeersQuery(node.EndPoint, fSearchInfoHash);
                                node.LastGetPeersTime = nowTicks;
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        #region Receive messages and data

        private void BeginRecv()
        {
            if (!fSearchRunned)
                return;

            EndPoint remoteAddress = new IPEndPoint(IPAnyAddress, 0);
            fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
        }

        private void EndRecv(IAsyncResult result)
        {
            if (!fSearchRunned)
                return;

            try {
                EndPoint remoteAddress = new IPEndPoint(IPAnyAddress, 0);
                int count = fSocket.EndReceiveFrom(result, ref remoteAddress);
                if (count > 0) {
                    byte[] buffer = new byte[count];
                    Buffer.BlockCopy(fBuffer, 0, buffer, 0, count);
                    OnRecvMessage((IPEndPoint)remoteAddress, buffer);
                }
            } catch (Exception ex) {
                fLogger.WriteError("EndRecv.1()", ex);
            }

            bool notsuccess;
            do {
                try {
                    BeginRecv();
                    notsuccess = false;
                } catch (Exception ex) {
                    fLogger.WriteError("EndRecv.2()", ex);
                    notsuccess = true;
                }
            } while (notsuccess);
        }

        private void OnRecvMessage(IPEndPoint ipinfo, byte[] data)
        {
            try {
                if (DataReceived != null) {
                    DataReceived(this, new DataEventArgs(ipinfo, data));
                }

                DHTMessage msg = DHTMessage.ParseBuffer(data);
                if (msg == null) return;

                if (msg.IsSimilarTo(fClientVer)) {
                    // Received a message from a similar client
                    DoPeersFoundEvent(new List<IPEndPoint>() { ipinfo });
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
                fLogger.WriteError("OnRecvMessage()", ex);
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
                        if (QueryReceived != null) {
                            QueryReceived(this, new MessageEventArgs(ipinfo, idVal, data));
                        }
                    }
                    break;
            }
        }

        private void UpdateRoutingTable(DHTNode node)
        {
            fRoutingTable.UpdateNode(node);
            fLastNodesUpdateTime = DateTime.Now.Ticks;

            fPeersHolder.SaveNode(node);
        }

        private void UpdateRoutingTable(IEnumerable<DHTNode> nodes)
        {
            fRoutingTable.UpdateNodes(nodes);
            fLastNodesUpdateTime = DateTime.Now.Ticks;

            foreach (var node in nodes) {
                fPeersHolder.SaveNode(node);
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

            UpdateRoutingTable(new DHTNode(id.Value, ipinfo));

            // define type of response by transactionId of query/response
            QueryType queryType = CheckTransaction(tid);

            switch (queryType) {
                case QueryType.Ping:
                    OnRecvPingResponse(ipinfo, id.Value);
                    break;

                case QueryType.FindNode:
                    OnRecvFindNodeResponse(ipinfo, id.Value, nodesStr);
                    break;

                case QueryType.GetPeers:
                    OnRecvGetPeersResponse(ipinfo, id.Value, nodesStr, valuesList, tokStr);
                    break;

                case QueryType.AnnouncePeer:
                    OnRecvAnnouncePeerResponse(ipinfo, id.Value);
                    break;

                case QueryType.None:
                    // TransactionId bad or unknown
                    if (ResponseReceived != null) {
                        ResponseReceived(this, new MessageEventArgs(ipinfo, id.Value, data));
                    }
                    break;
            }
        }

        private void OnRecvPingResponse(IPEndPoint ipinfo, byte[] nodeId)
        {
            // id only
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Peer pinged: {0}", ipinfo);
#endif
            DoPeerPingedEvent(ipinfo, nodeId);
        }

        private void OnRecvFindNodeResponse(IPEndPoint ipinfo, byte[] nodeId, BString nodesStr)
        {
            // according to bep_0005, find_node response contain a list of nodes
            ProcessNodesStr(ipinfo, nodesStr);
        }

        private void OnRecvGetPeersResponse(IPEndPoint ipinfo, byte[] nodeId, BString nodesStr, BList valuesList, BString token)
        {
            // according to bep_0005, get_peers response can contain a list of nodes
            ProcessNodesStr(ipinfo, nodesStr);

            if (!ProcessValuesStr(ipinfo, nodeId, valuesList, token)) {
                SendAnnouncePeerQuery(ipinfo, fSearchInfoHash, 0, fPublicEndPoint.Port, token);
            }
        }

        private void OnRecvAnnouncePeerResponse(IPEndPoint ipinfo, byte[] nodeId)
        {
            // id only
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Peer announced successfully to {0}", ipinfo);
#endif
        }

        private bool ProcessValuesStr(IPEndPoint ipinfo, byte[] nodeId, BList valuesList, BString token)
        {
            bool result = false;
            if (valuesList != null && valuesList.Count != 0) {
                var values = DHTHelper.ParseValuesList(valuesList);

                if (values.Count > 0) {
#if DEBUG_DHT_INTERNALS
                    fLogger.WriteDebug("Receive {0} values (peers) from {1}", values.Count, ipinfo.ToString());
#endif

                    foreach (var peer in values) {
                        fLogger.WriteDebug("Receive peer {0} from {1}", peer, ipinfo);

                        if (peer.Address.Equals(fPublicEndPoint.Address)) {
                            result = true;
                        }
                    }

                    DoPeersFoundEvent(values);
                }
            }
            return result;
        }

        private void ProcessNodesStr(IPEndPoint ipinfo, BString nodesStr)
        {
            var nodesList = DHTHelper.ParseNodesList(nodesStr);

            if (nodesList != null && nodesList.Count > 0) {
#if DEBUG_DHT_INTERNALS
                fLogger.WriteDebug("Receive {0} nodes from {1}", nodesList.Count, ipinfo.ToString());
#endif

                UpdateRoutingTable(nodesList);
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

#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Receive `announce_peer` query from {0} [{1}] for {2}", ipinfo.ToString(), id.Value.ToHexString(), infoHash.Value.ToHexString());
#endif

            UpdateRoutingTable(new DHTNode(id.Value, ipinfo));

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

#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Receive `ping` query from {0} [{1}]", ipinfo.ToString(), id.Value.ToHexString());
#endif

            UpdateRoutingTable(new DHTNode(id.Value, ipinfo));

            Send(ipinfo, DHTMessage.CreatePingResponse(t, fLocalID));
        }

        private void OnRecvFindNodeQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var target = args.Get<BString>("target");

#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Receive `find_node` query from {0} [{1}]", ipinfo.ToString(), id.Value.ToHexString());
#endif

            UpdateRoutingTable(new DHTNode(id.Value, ipinfo));

            var nodesList = fRoutingTable.FindNodes(target.Value);
            Send(ipinfo, DHTMessage.CreateFindNodeResponse(t, fLocalID, nodesList));
        }

        private void OnRecvGetPeersQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var t = data.Get<BString>("t");
            var args = data.Get<BDictionary>("a");

            var id = args.Get<BString>("id");
            var infoHash = args.Get<BString>("info_hash");

#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Receive `get_peers` query from {0} [{1}] for {2}", ipinfo.ToString(), id.Value.ToHexString(), infoHash.Value.ToHexString());
#endif

            UpdateRoutingTable(new DHTNode(id.Value, ipinfo));

            var neighbor = DHTHelper.GetNeighbor(infoHash.Value, fLocalID);
            var peersList = (Algorithms.ArraysEqual(infoHash.Value, fSearchInfoHash)) ? fPeersHolder.GetPeersList() : null;
            var nodesList = fRoutingTable.FindNodes(infoHash.Value);
            Send(ipinfo, DHTMessage.CreateGetPeersResponse(t, neighbor, infoHash.Value, peersList, nodesList));
        }

        #endregion

        #region Transactions

        public void SetTransaction(BString transactionId, DHTMessage message)
        {
            if (transactionId != null && transactionId.Length == 2) {
                int tid = BitConverter.ToUInt16(transactionId.Value, 0);
                fTransactions[tid] = message;
            }
        }

        public QueryType CheckTransaction(BString transactionId)
        {
            QueryType result = QueryType.None;

            if (transactionId != null && transactionId.Length == 2) {
                int tid = BitConverter.ToUInt16(transactionId.Value, 0);

                DHTMessage message;
                if (fTransactions.TryGetValue(tid, out message)) {
                    result = message.QueryType;
                    fTransactions.Remove(tid);
                }
            }

            return result;
        }

        #endregion

        #region Queries and responses

        internal void SendPingQuery(IPEndPoint address, bool async = true)
        {
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Send peer ping {0}", address);
#endif

            var transactionID = DHTHelper.GetTransactionId();
            var msg = DHTMessage.CreatePingQuery(transactionID, fLocalID);
            SetTransaction(transactionID, msg);
            Send(address, msg, async);
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
            if (token == null || token.Length == 0) {
                return;
            }

            DHTNode node = fRoutingTable.FindNode(address);
            if (node != null) {
                long nowTicks = DateTime.Now.Ticks;
                if (nowTicks - node.LastAnnouncementTime < AnnounceLife.Ticks) {
                    return;
                }
                node.LastAnnouncementTime = nowTicks;
            }

#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Send announce peer query {0}, {1}, {2}", address, implied_port, port);
#endif

            // https://www.bittorrent.org/beps/bep_0005.html
            // If implied_port (0/1) is present and non-zero, the port argument should be ignored
            // and the source port of the UDP packet should be used as the peer's port instead.

            var transactionID = DHTHelper.GetTransactionId();
            var msg = DHTMessage.CreateAnnouncePeerQuery(transactionID, fLocalID, infoHash, implied_port, port, token);
            SetTransaction(transactionID, msg);
            Send(address, msg);
        }

        internal void SendGetPeersQuery(IPEndPoint address, byte[] infoHash)
        {
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Send get peers query {0}", address);
#endif

            var transactionID = DHTHelper.GetTransactionId();
            var msg = DHTMessage.CreateGetPeersQuery(transactionID, fLocalID, infoHash);
            SetTransaction(transactionID, msg);
            Send(address, msg);
        }

        public void Send(IPEndPoint address, DHTMessage msg, bool async = true)
        {
            Send(address, msg.Data, async);
        }

        public void Send(IPEndPoint address, BDictionary data, bool async = true)
        {
            try {
                // according to bep_0005
                data.Add("v", fClientVer);

                byte[] dataArray = data.EncodeAsBytes();
                Send(address, dataArray, async);
            } catch (Exception ex) {
                fLogger.WriteError("Send()", ex);
            }
        }

        public void Send(IPEndPoint address, byte[] data, bool async = true)
        {
            try {
                if (async) {
                    fSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, address, (ar) => {
                        try {
                            fSocket.EndSendTo(ar);
                        } catch (Exception ex) {
                            fLogger.WriteError("Send.1(" + address + ")", ex);
                        }
                    }, null);
                } else {
                    fSocket.SendTo(data, 0, data.Length, SocketFlags.None, address);
                }
            } catch (Exception ex) {
                fLogger.WriteError("Send()", ex);
            }
        }

        #endregion

        #region Events

        private void DoPeersFoundEvent(List<IPEndPoint> values)
        {
            if (PeersFound != null) {
                PeersFound(this, new PeersFoundEventArgs(values));
            }
        }

        private void DoPeerPingedEvent(IPEndPoint ipinfo, byte[] nodeId)
        {
            if (PeerPinged != null) {
                PeerPinged(this, new PeerPingedEventArgs(ipinfo, nodeId));
            }
        }

        #endregion
    }
}
