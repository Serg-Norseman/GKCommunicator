﻿/*
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

using System;
using System.Collections.Generic;
using System.Net;
using BencodeNET;

namespace GKNet.DHT
{
    public enum NodeState
    {
        Unknown,
        Good,
        Questionable,
        Bad
    }

    public class DHTNode
    {
        public static readonly long NODE_EXPIRY_TIME = TimeSpan.FromMinutes(15).Ticks;
        public static readonly long NODE_LIFE_TIME = TimeSpan.FromMinutes(3).Ticks;


        public DHTId Id { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public long LastAnnouncementTime { get; set; }
        public long LastGetPeersTime { get; set; }
        public long LastUpdateTime { get; private set; }

        public NodeState State
        {
            get {
                if (LastUpdateTime == 0)
                    return NodeState.Unknown;

                long dtNowTicks = DateTime.UtcNow.Ticks;
                if (dtNowTicks - LastUpdateTime < NODE_LIFE_TIME) {
                    return NodeState.Good;
                } else {
                    return (dtNowTicks - LastUpdateTime < NODE_EXPIRY_TIME) ? NodeState.Questionable : NodeState.Bad;
                }
            }
        }


        public DHTNode(byte[] id, IPEndPoint endPoint)
        {
            Id = new DHTId(id);
            EndPoint = endPoint;
        }

        public DHTNode(DHTId id, IPEndPoint endPoint)
        {
            Id = id;
            EndPoint = endPoint;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", EndPoint, Id);
        }

        internal void Update()
        {
            LastUpdateTime = DateTime.UtcNow.Ticks;
        }


        #region Compact and parse functions

        private const int CompactNodeRecordLengthIP4 = 26; // 20 + 6
        private const int CompactNodeRecordLengthIP6 = 38; // 20 + 18

#if !IP6
        private const int CompactNodeRecordLength = CompactNodeRecordLengthIP4;
#else
        private const int CompactNodeRecordLength = CompactNodeRecordLengthIP6;
#endif

        public static List<IPEndPoint> ParseValuesList(BList data)
        {
            var result = new List<IPEndPoint>();

            foreach (var item in data) {
                var str = item as BString;
                var itemBytes = str.Value;

                if (itemBytes.Length == 6) {
                    var ip = new IPAddress(itemBytes.SubArray(0, 4));

                    var b = itemBytes[4];
                    itemBytes[4] = itemBytes[5];
                    itemBytes[5] = b;

                    var port = BitConverter.ToUInt16(itemBytes, 4);
                    var xnode = new IPEndPoint(Utilities.PrepareAddress(ip), port);
                    result.Add(xnode);
                } else if (itemBytes.Length == 18) {
                    var ip = new IPAddress(itemBytes.SubArray(0, 16));

                    var b = itemBytes[16];
                    itemBytes[16] = itemBytes[17];
                    itemBytes[17] = b;

                    var port = BitConverter.ToUInt16(itemBytes, 16);
                    var xnode = new IPEndPoint(Utilities.PrepareAddress(ip), port);
                    result.Add(xnode);
                }
            }

            return result;
        }

        public static List<DHTNode> ParseNodesList(BString nodesStr)
        {
            List<DHTNode> result = null;

            if (nodesStr != null && nodesStr.Length != 0) {
                byte[] data = nodesStr.Value;

                if (data.Length % CompactNodeRecordLengthIP4 == 0) {
                    result = ParseNodesListIP4(data);
                } else if (data.Length % CompactNodeRecordLengthIP6 == 0) {
                    result = ParseNodesListIP6(data);
                }
            }

            return result;
        }

        private static List<DHTNode> ParseNodesListIP4(byte[] data)
        {
            var result = new List<DHTNode>();
            for (int i = 0; i < data.Length; i += 26) {
                var dd = data.SubArray(i, 26);

                var b = dd[24];
                dd[24] = dd[25];
                dd[25] = b;
                var id = dd.SubArray(0, 20);
                var ip = new IPAddress(dd.SubArray(20, 4));
                var port = BitConverter.ToUInt16(dd, 24);
                var tt = new DHTNode(id, new IPEndPoint(Utilities.PrepareAddress(ip), port));
                result.Add(tt);
            }
            return result;
        }

        private static List<DHTNode> ParseNodesListIP6(byte[] data)
        {
            var result = new List<DHTNode>();
            for (int i = 0; i < data.Length; i += 38) {
                var dd = data.SubArray(i, 38);

                var b = dd[36];
                dd[36] = dd[37];
                dd[37] = b;
                var id = dd.SubArray(0, 20);
                var ip = new IPAddress(dd.SubArray(20, 16));
                var port = BitConverter.ToUInt16(dd, 36);
                var tt = new DHTNode(id, new IPEndPoint(Utilities.PrepareAddress(ip), port));
                result.Add(tt);
            }
            return result;
        }

        public static DHTId GetNeighbor(byte[] target, byte[] nid)
        {
            var result = new byte[20];
            for (int i = 0; i < 10; i++)
                result[i] = target[i];
            for (int i = 10; i < 20; i++)
                result[i] = nid[i];
            return new DHTId(result);
        }

        // TODO: ATTENTION, the quantity is not more than according to specification!
        public static BList CompactPeers(IList<IDHTPeer> peersList)
        {
            BList values = null;
            if (peersList != null && peersList.Count > 0) {
                values = new BList();
                foreach (var peer in peersList) {
                    values.Add(new BString(CompactEndPoint(peer.EndPoint)));
                }
            }
            return values;
        }

        public static byte[] CompactNodes(IList<DHTNode> nodesList)
        {
            int nodesCount = nodesList.Count;
            byte[] nodesArray = new byte[nodesCount * 26];
            for (int i = 0; i < nodesCount; i++) {
                var node = nodesList[i];
                var compact = CompactNode(node);
                Buffer.BlockCopy(compact, 0, nodesArray, i * 26, 26);
            }
            return nodesArray;
        }

        public static byte[] CompactNode(DHTNode node)
        {
            IPAddress address = node.EndPoint.Address;
            ushort port = (ushort)node.EndPoint.Port;

            var info = new byte[26];
            Buffer.BlockCopy(node.Id.Data, 0, info, 0, 20);
            Buffer.BlockCopy(address.GetAddressBytes(), 0, info, 20, 4);
            info[24] = (byte)((port >> 8) & 0xFF);
            info[25] = (byte)(port & 0xFF);
            return info;
        }

        public static byte[] CompactEndPoint(IPEndPoint endPoint)
        {
            IPAddress address = endPoint.Address;
            ushort port = (ushort)endPoint.Port;

            var info = new byte[6];
            Buffer.BlockCopy(address.GetAddressBytes(), 0, info, 0, 4);
            info[4] = (byte)((port >> 8) & 0xFF);
            info[5] = (byte)(port & 0xFF);
            return info;
        }

        #endregion
    }
}
