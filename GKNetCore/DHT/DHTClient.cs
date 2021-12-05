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

#define DEBUG_DHT_INTERNALS

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using BencodeNET;
using BSLib;
using GKNet.Logging;

namespace GKNet.DHT
{
    public sealed class DHTClient : UDPSocket
    {
        private static readonly long NODES_UPDATE_TIME = TimeSpan.FromMinutes(1).Ticks;
        private static readonly TimeSpan GetPeersRange = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan AnnounceLife = TimeSpan.FromMinutes(10); // in question, from articles
        private static readonly long KTABLE_REFRESH_TIME = TimeSpan.FromMinutes(1).Ticks;

        public const int PublicDHTPort = 6881;
        public const int KTableSize = 2048;

        private long fLastNodesUpdateTime;
        private long fLastRefreshTime;
        private DHTId fLocalID;
        private IList<IPAddress> fRouters;
        private DHTId fSearchInfoHash;

        private readonly string fClientVer;
        private readonly ILogger fLogger;
        private readonly IDHTPeersHolder fPeersHolder;
        private readonly DHTRoutingTable fRoutingTable;
        private readonly DHTTransactions fTransactions;

        public DHTId LocalID
        {
            get { return fLocalID; }
        }

        public DHTRoutingTable RoutingTable
        {
            get { return fRoutingTable; }
        }

        public event EventHandler<PeersFoundEventArgs> PeersFound;
        public event EventHandler<PeerPingedEventArgs> PeerPinged;

        public event EventHandler<MessageEventArgs> QueryReceived;
        public event EventHandler<MessageEventArgs> ResponseReceived;

        public DHTClient(IPEndPoint localEndPoint, IDHTPeersHolder peersHolder, string clientVer) : base(localEndPoint)
        {
            fPeersHolder = peersHolder;
            fLocalID = peersHolder.ClientNodeId;
            fClientVer = clientVer;
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "DHTClient");
            fRoutingTable = new DHTRoutingTable(KTableSize);
            fTransactions = new DHTTransactions();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                Stop();
            }
            base.Dispose(disposing);
        }

        public void Start(DHTId searchInfoHash)
        {
            Open();
            Bootstrap();
            SearchNodes(searchInfoHash);
        }

        public void Stop()
        {
            Close();
            fTransactions.Clear();
            fRoutingTable.Clear();
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
                    fRouters.Add(Utilities.PrepareAddress(addr));
                }
            }
        }

        private void SearchNodes(DHTId searchInfoHash)
        {
            fLogger.WriteDebug("Search for: {0}", searchInfoHash.ToHex());

            fSearchInfoHash = searchInfoHash;

            new Thread(() => {
                while (Connected) {
                    long nowTicks = DateTime.UtcNow.Ticks;

                    int count = 0;
                    lock (fRoutingTable) {
                        // if the routing table has not been updated for more
                        // than a minute - reset routing table
                        if (nowTicks - fLastNodesUpdateTime > NODES_UPDATE_TIME) {
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
                        // bucket = 32 - significantly increases the speed and reliability of peers discovery
                        var nodes = fRoutingTable.GetClosest(fSearchInfoHash.Data, 32);

#if DEBUG_DHT_INTERNALS
                        fLogger.WriteDebug("RoutingTable size: {0}", fRoutingTable.Count);
                        fLogger.WriteDebug("Closest list size: {0}", nodes.Count);
#endif

                        foreach (var node in nodes) {
                            if (nowTicks - node.LastGetPeersTime > GetPeersRange.Ticks) {
                                SendGetPeersQuery(node.EndPoint, fSearchInfoHash);
                                node.LastGetPeersTime = nowTicks;
                            }
                        }
                    }

                    RefreshRoutingTable();

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public void FindUnkPeer(IDHTPeer peer)
        {
            if (peer == null || peer.ID == null)
                return;

            var nodes = fRoutingTable.GetClosest(fSearchInfoHash.Data);
            foreach (var node in nodes) {
                SendFindNodeQuery(node.EndPoint, peer.ID, false);
            }
        }

        private void RefreshRoutingTable()
        {
            long nowTicks = DateTime.UtcNow.Ticks;
            if (nowTicks - fLastRefreshTime < KTABLE_REFRESH_TIME) {
                return;
            }

            var routingNodes = fRoutingTable.GetNodes();
            foreach (DHTNode node in routingNodes) {
                if (node.State != NodeState.Good) {
                    SendPingQuery(node.EndPoint);
                }
            }

            fLastRefreshTime = nowTicks;
        }

        #region Receive messages and data

        protected override void OnRecvMessage(IPEndPoint ipinfo, byte[] data)
        {
            try {
                DHTMessage msg = DHTMessage.ParseBuffer(data);
                if (msg == null) return;

                if (msg.IsSimilarTo(fClientVer)) {
                    // Received a message from a similar client
                    // many false positives, client version is not unique
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
                    OnRecvCustomQuery(ipinfo, data);
                    break;
            }
        }

        private void UpdateRoutingTable(DHTNode node)
        {
            fRoutingTable.UpdateNode(node);
            fLastNodesUpdateTime = DateTime.UtcNow.Ticks;

            fPeersHolder.SaveNode(node);
        }

        private void OnRecvResponseX(IPEndPoint ipinfo, DHTResponseMessage msg)
        {
            if (msg == null) return;

            BDictionary data = msg.Data;

            var tid = data.Get<BString>("t");

            var returnValues = data.Get<BDictionary>("r");
            if (returnValues == null) {
                // response is invalid
                return;
            }

            BString id = returnValues.Get<BString>("id");
            BString tokStr = returnValues.Get<BString>("token");
            BList valuesList = returnValues.Get<BList>("values");
            BString nodesStr = returnValues.Get<BString>("nodes");

            var remoteNode = new DHTNode(id.Value, ipinfo);
            UpdateRoutingTable(remoteNode);

            // define type of response by transactionId of query/response
            QueryType queryType = fTransactions.CheckQuery(tid);

            switch (queryType) {
                case QueryType.Ping:
                    OnRecvPingResponse(remoteNode);
                    break;

                case QueryType.FindNode:
                    OnRecvFindNodeResponse(remoteNode, nodesStr);
                    break;

                case QueryType.GetPeers:
                    OnRecvGetPeersResponse(remoteNode, nodesStr, valuesList, tokStr);
                    break;

                case QueryType.AnnouncePeer:
                    OnRecvAnnouncePeerResponse(remoteNode);
                    break;

                case QueryType.None:
                    // TransactionId bad or unknown
                    OnRecvCustomResponse(remoteNode, data);
                    break;
            }
        }

        private void OnRecvPingResponse(DHTNode remoteNode)
        {
            // id only
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Peer pinged: {0}", remoteNode.EndPoint);
#endif

            if (PeerPinged != null) {
                PeerPinged(this, new PeerPingedEventArgs(remoteNode.EndPoint, remoteNode.Id));
            }
        }

        private void OnRecvFindNodeResponse(DHTNode remoteNode, BString nodesStr)
        {
            // according to bep_0005, find_node response contain a list of nodes
            ProcessNodesStr(remoteNode, nodesStr);
        }

        private void OnRecvGetPeersResponse(DHTNode remoteNode, BString nodesStr, BList valuesList, BString token)
        {
            // according to bep_0005, get_peers response can contain a list of nodes
            ProcessNodesStr(remoteNode, nodesStr);

            if (!ProcessValuesStr(remoteNode, valuesList, token)) {
                SendAnnouncePeerQuery(remoteNode, fSearchInfoHash, 0, PublicEndPoint.Port, token);
            }
        }

        private void OnRecvAnnouncePeerResponse(DHTNode remoteNode)
        {
            // id only
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Peer announced successfully to {0}", remoteNode.EndPoint);
#endif
        }

        private void OnRecvCustomResponse(DHTNode remoteNode, BDictionary data)
        {
            if (ResponseReceived != null) {
                ResponseReceived(this, new MessageEventArgs(remoteNode.EndPoint, remoteNode.Id, data));
            }
        }

        private bool ProcessValuesStr(DHTNode remoteNode, BList valuesList, BString token)
        {
            bool result = false;
            if (valuesList != null && valuesList.Count != 0) {
                var values = DHTNode.ParseValuesList(valuesList);

                if (values.Count > 0) {
#if DEBUG_DHT_INTERNALS
                    fLogger.WriteDebug("Receive {0} values (peers) from {1}", values.Count, remoteNode.EndPoint);
#endif

                    foreach (var peer in values) {
                        fLogger.WriteDebug("Receive peer {0} from {1}", peer, remoteNode.EndPoint);

                        if (peer.Address.Equals(PublicEndPoint.Address)) {
                            result = true;
                        }
                    }

                    if (PeersFound != null) {
                        PeersFound(this, new PeersFoundEventArgs(values));
                    }
                }
            }
            return result;
        }

        private void ProcessNodesStr(DHTNode remoteNode, BString nodesStr)
        {
            var nodesList = DHTNode.ParseNodesList(nodesStr);

            if (nodesList != null && nodesList.Count > 0) {
#if DEBUG_DHT_INTERNALS
                fLogger.WriteDebug("Receive {0} nodes from {1}", nodesList.Count, remoteNode.EndPoint);
#endif

                foreach (var node in nodesList) {
                    UpdateRoutingTable(node);
                }
            }
        }

        private void OnRecvCustomQuery(IPEndPoint ipinfo, BDictionary data)
        {
            var args = data.Get<BDictionary>("a");
            if (args == null) return;

            var id = args.Get<BString>("id");
            DHTId nodeId = (id != null) ? new DHTId(id) : null;

            if (QueryReceived != null) {
                QueryReceived(this, new MessageEventArgs(ipinfo, nodeId, data));
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

            if (!Algorithms.ArraysEqual(infoHash.Value, fSearchInfoHash.Data)) {
                // skip response for another infohash query
                return;
            }

            // receive `announce_peer` query for our infohash
            var nodesList = fRoutingTable.GetClosest(infoHash.Value);
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

            var nodesList = fRoutingTable.GetClosest(target.Value);
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

            var neighbor = DHTNode.GetNeighbor(infoHash.Value, fLocalID.Data);
            var peersList = (Algorithms.ArraysEqual(infoHash.Value, fSearchInfoHash.Data)) ? fPeersHolder.GetPeersList() : null;
            var nodesList = fRoutingTable.GetClosest(infoHash.Value);
            Send(ipinfo, DHTMessage.CreateGetPeersResponse(t, neighbor, new DHTId(infoHash), peersList, nodesList));
        }

        #endregion

        #region Queries and responses

        internal void SendPingQuery(IPEndPoint address, bool async = true)
        {
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Send peer ping {0}", address);
#endif

            var transactionID = DHTTransactions.GetNextId();
            var msg = DHTMessage.CreatePingQuery(transactionID, fLocalID);
            fTransactions.SetQuery(transactionID, msg);
            Send(address, msg, async);
        }

        internal void SendFindNodeQuery(IPEndPoint address, DHTId target, bool neighbor = true)
        {
            var transactionID = DHTTransactions.GetNextId();
            DHTId targetId = (target == null) ? fLocalID : ((neighbor) ? DHTNode.GetNeighbor(target.Data, fLocalID.Data) : target);

            var msg = DHTMessage.CreateFindNodeQuery(transactionID, fLocalID, targetId);
            fTransactions.SetQuery(transactionID, msg);
            Send(address, msg);
        }

        internal void SendAnnouncePeerQuery(DHTNode remoteNode, DHTId infoHash, byte implied_port, int port, BString token)
        {
            if (remoteNode == null || token == null || token.Length == 0) {
                return;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            if (nowTicks - remoteNode.LastAnnouncementTime < AnnounceLife.Ticks) {
                return;
            }
            remoteNode.LastAnnouncementTime = nowTicks;

#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Send announce peer query {0}, {1}, {2}", remoteNode.EndPoint, implied_port, port);
#endif

            // https://www.bittorrent.org/beps/bep_0005.html
            // If implied_port (0/1) is present and non-zero, the port argument should be ignored
            // and the source port of the UDP packet should be used as the peer's port instead.

            var transactionID = DHTTransactions.GetNextId();
            var msg = DHTMessage.CreateAnnouncePeerQuery(transactionID, fLocalID, infoHash, implied_port, port, token);
            fTransactions.SetQuery(transactionID, msg);
            Send(remoteNode.EndPoint, msg);
        }

        internal void SendGetPeersQuery(IPEndPoint address, DHTId infoHash)
        {
#if DEBUG_DHT_INTERNALS
            fLogger.WriteDebug("Send get peers query {0}", address);
#endif

            var transactionID = DHTTransactions.GetNextId();
            var msg = DHTMessage.CreateGetPeersQuery(transactionID, fLocalID, infoHash);
            fTransactions.SetQuery(transactionID, msg);
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

        #endregion
    }
}
