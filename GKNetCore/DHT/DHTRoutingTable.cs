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
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GKNet.DHT
{
    public class DHTRoutingTable
    {
        internal class RouteComparer : IComparer<byte[]>
        {
            private RouteComparer() { }

            public int Compare(byte[] x, byte[] y)
            {
                if (x.Length != y.Length) {
                    throw new ArgumentException("Length of the arguments must be equal");
                }

                for (int i = 0; i < x.Length; i++) {
                    if (x[i] != y[i]) {
                        return (x[i] > y[i]) ? +1 : -1;
                    }
                }

                return 0;
            }

            public static readonly IComparer<byte[]> Instance = new RouteComparer();
        }

        private readonly int fMaxNodeSize;
        private readonly Dictionary<EndPoint, DHTNode> fKTable;
        private readonly object fLock;

        public int Count
        {
            get { return fKTable.Count; }
        }

        public bool IsFull
        {
            get {
                return (fKTable.Count >= fMaxNodeSize);
            }
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
            if (node == null || node.Id == null)
                return;

            if (IsFull) {
                ClearExpireNode();
            }

            if (IsFull) {
                return;
            }

            DHTNode existNode;
            if (fKTable.TryGetValue(node.EndPoint, out existNode)) {
                if (node.Id == existNode.Id) {
                    node = existNode;
                } else {
                    // replace to new
                    fKTable[node.EndPoint] = node;
                }
            } else {
                fKTable.Add(node.EndPoint, node);
            }

            node.Update();
        }

        private void ClearExpireNode()
        {
            lock (fLock) {
                foreach (var node in fKTable.Values) {
                    if (node.State == NodeState.Bad) {
                        fKTable.Remove(node.EndPoint);
                        continue;
                    }
                }
            }
        }

        public void Clear()
        {
            fKTable.Clear();
        }

        public IList<DHTNode> GetClosest(byte[] target)
        {
            DHTNode[] values;
            lock (fLock) {
                values = fKTable.Values.ToArray();
            }

            if (values.Length <= 8)
                return values;

            var list = new SortedList<byte[], DHTNode>(8, RouteComparer.Instance);

            foreach (var node in values) {
                if (node.State == NodeState.Bad) {
                    fKTable.Remove(node.EndPoint);
                    continue;
                }

                var distance = ComputeRouteDistance(node.Id.Data, target);
                if (list.ContainsKey(distance)) {
                    // why can there be duplicates in the list?
                    // because the table is a dictionary by EndPoint, not NodeId?
                    continue;
                }

                if (list.Count >= 8) {
                    // if distance is greater than or equal to maxdistance from list then continue
                    int lastIndex = list.Count - 1;
                    if (RouteComparer.Instance.Compare(distance, list.Keys[lastIndex]) >= 0) {
                        continue;
                    }
                    list.RemoveAt(lastIndex);
                }

                list.Add(distance, node);
            }

            return list.Values;
        }

        public DHTNode FindNode(IPEndPoint endPoint)
        {
            DHTNode node;
            return (endPoint != null && fKTable.TryGetValue(endPoint, out node)) ? node : null;
        }

        public IList<DHTNode> GetNodes()
        {
            lock (fLock) {
                return fKTable.Values.ToList();
            }
        }

        internal static byte[] ComputeRouteDistance(byte[] sourceId, byte[] targetId)
        {
            var result = new byte[Math.Min(sourceId.Length, targetId.Length)];
            for (var i = 0; i < result.Length; i++) {
                result[i] = (byte)(sourceId[i] ^ targetId[i]);
            }
            return result;
        }
    }
}
