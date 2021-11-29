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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BSLib;

namespace GKNet.DHT
{
    public class DHTRoutingTable : IEnumerable<DHTNode>
    {
        private class RouteComparer : IComparer<byte[]>
        {
            private RouteComparer() { }

            public int Compare(byte[] x, byte[] y)
            {
                if (x.Length != y.Length) {
                    return x.Length > y.Length ? -1 : 1;
                }
                var length = Math.Min(x.Length, y.Length);
                for (var i = 0; i < length; i++) {
                    if (x[i] == y[i])
                        continue;
                    return x[i] > y[i] ? -1 : 1;
                }
                return 1;
            }

            public static readonly IComparer<byte[]> Instance = new RouteComparer();
        }

        private static readonly TimeSpan fRouteLife = TimeSpan.FromMinutes(15);

        private readonly int fMaxNodeSize;
        private readonly Dictionary<EndPoint, DHTNode> fKTable;
        private readonly object fLock;

        private long fMinLastTime = DateTime.Now.Ticks;

        public int Count
        {
            get { return fKTable.Count; }
        }

        public DHTRoutingTable(int nodeSize)
        {
            fKTable = new Dictionary<EndPoint, DHTNode>();
            fMaxNodeSize = nodeSize;
            fLock = new object();
        }

        public void UpdateNodes(IEnumerable<DHTNode> nodes)
        {
            foreach (var node in nodes) {
                UpdateNode(node);
            }
        }

        public void UpdateNode(DHTNode node)
        {
            if (node == null || node.ID == null)
                return;

            if (fKTable.Count >= fMaxNodeSize && fMinLastTime + fRouteLife.Ticks < DateTime.Now.Ticks) {
                lock (fLock) {
                    ClearExpireNode();
                }
            }

            if (fKTable.Count >= fMaxNodeSize) {
                return;
            }

            DHTNode existNode;
            if (fKTable.TryGetValue(node.EndPoint, out existNode)) {
                if (Algorithms.ArraysEqual(node.ID, existNode.ID)) {
                    node = existNode;
                } else {
                    // replace to new
                    fKTable[node.EndPoint] = node;
                }
            } else {
                fKTable.Add(node.EndPoint, node);
            }

            node.LastUpdateTime = DateTime.Now.Ticks;
        }

        private void ClearExpireNode()
        {
            var minTime = DateTime.Now.Ticks;
            foreach (var item in fKTable.Values) {
                if (DateTime.Now.Ticks - item.LastUpdateTime > fRouteLife.Ticks) {
                    fKTable.Remove(item.EndPoint);
                    continue;
                }
                minTime = Math.Min(fMinLastTime, item.LastUpdateTime);
            }
            fMinLastTime = Math.Max(minTime, fMinLastTime);
        }

        public void Clear()
        {
            fKTable.Clear();
        }

        public IList<DHTNode> FindNodes(byte[] id)
        {
            DHTNode[] values;
            lock (fLock) {
                values = fKTable.Values.ToArray();
            }

            if (values.Length <= 8)
                return values;

            var list = new SortedList<byte[], DHTNode>(8, RouteComparer.Instance);
            var minTime = DateTime.MaxValue.Ticks;
            var tableFull = fKTable.Count >= fMaxNodeSize;
            foreach (var item in values) {
                if (tableFull && DateTime.Now.Ticks - item.LastUpdateTime > fRouteLife.Ticks) {
                    fKTable.Remove(item.EndPoint);
                    continue;
                }
                var distance = ComputeRouteDistance(item.ID, id);
                if (list.Count >= 8) {
                    if (RouteComparer.Instance.Compare(list.Keys[0], distance) >= 0) {
                        continue;
                    }
                    list.RemoveAt(0);
                }
                list.Add(distance, item);
                minTime = Math.Min(fMinLastTime, item.LastUpdateTime);
            }
            fMinLastTime = Math.Max(minTime, fMinLastTime);
            return list.Values;
        }

        public DHTNode FindNode(IPEndPoint endPoint)
        {
            DHTNode node;
            return (endPoint != null && fKTable.TryGetValue(endPoint, out node)) ? node : null;
        }

        #region IEnumerable

        public IEnumerator<DHTNode> GetEnumerator()
        {
            var minTime = DateTime.MaxValue.Ticks;
            var tableFull = fKTable.Count >= fMaxNodeSize;
            foreach (var item in fKTable.Values) {
                if (tableFull && DateTime.Now.Ticks - item.LastUpdateTime > fRouteLife.Ticks) {
                    fKTable.Remove(item.EndPoint);
                    continue;
                }
                minTime = Math.Min(fMinLastTime, item.LastUpdateTime);
                yield return item;
            }
            fMinLastTime = Math.Max(minTime, fMinLastTime);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private static byte[] ComputeRouteDistance(byte[] sourceId, byte[] targetId)
        {
            var result = new byte[Math.Min(sourceId.Length, targetId.Length)];
            for (var i = 0; i < result.Length; i++) {
                result[i] = (byte)(sourceId[i] ^ targetId[i]);
            }
            return result;
        }
    }
}
