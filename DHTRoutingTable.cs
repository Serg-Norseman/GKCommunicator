using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DHTConnector
{
    public class DHTRoutingTable : IEnumerable<DHTNode>
    {
        private class Route
        {
            public DHTNode Node { get; set; }
            public long LastTime { get; set; }
            public string RouteId => Node == null ? string.Empty : Node.EndPoint.Address + ":" + Node.EndPoint.Port;
        }

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
        private readonly ConcurrentDictionary<string, Route> fKTable;
        private long fMinLastTime = DateTime.Now.Ticks;

        private static byte[] ComputeRouteDistance(byte[] sourceId, byte[] targetId)
        {
            var result = new byte[Math.Min(sourceId.Length, targetId.Length)];
            for (var i = 0; i < result.Length; i++) {
                result[i] = (byte)(sourceId[i] ^ targetId[i]);
            }
            return result;
        }

        public DHTRoutingTable(int nodeSize)
        {
            this.fKTable = new ConcurrentDictionary<string, Route>();
            this.fMaxNodeSize = nodeSize;
        }

        public int Count => fKTable.Count;

        public bool IsFull => fKTable.Count >= fMaxNodeSize && fMinLastTime + fRouteLife.Ticks > DateTime.Now.Ticks;

        public void AddNode(DHTNode node)
        {
            if (node.ID == null || fKTable.Count >= fMaxNodeSize)
                return;
            var route = new Route() {
                Node = node,
                LastTime = DateTime.Now.Ticks
            };
            fKTable.TryAdd(route.RouteId, route);
        }

        public void AddNodes(IEnumerable<DHTNode> nodes)
        {
            foreach (var node in nodes) {
                AddNode(node);
            }
        }

        public void AddOrUpdateNode(DHTNode node)
        {
            if (node.ID == null)
                return;
            if (fKTable.Count >= fMaxNodeSize && fMinLastTime + fRouteLife.Ticks < DateTime.Now.Ticks) {
                lock (this) {
                    if (fMinLastTime + fRouteLife.Ticks < DateTime.Now.Ticks)
                        ClearExpireNode();
                }
            }
            if (fKTable.Count >= fMaxNodeSize)
                return;
            var route = new Route() {
                Node = node,
                LastTime = DateTime.Now.Ticks
            };
            fKTable.AddOrUpdate(route.RouteId, route, (k, n) => {
                n.Node = route.Node;
                n.LastTime = DateTime.Now.Ticks;
                return n;
            });
        }

        private void ClearExpireNode()
        {
            var minTime = DateTime.Now.Ticks;
            foreach (var item in fKTable.Values) {
                if (DateTime.Now.Ticks - item.LastTime > fRouteLife.Ticks) {
                    fKTable.TryRemove(item.RouteId, out Route remove);
                    continue;
                }
                minTime = Math.Min(fMinLastTime, item.LastTime);
            }
            fMinLastTime = Math.Max(minTime, fMinLastTime);
        }

        public IList<DHTNode> FindNodes(byte[] id)
        {
            if (fKTable.Count <= 8)
                return fKTable.Values.Take(8).Select(route => route.Node).ToArray();
            var list = new SortedList<byte[], DHTNode>(8, RouteComparer.Instance);
            var minTime = DateTime.MaxValue.Ticks;
            var tableFull = fKTable.Count >= fMaxNodeSize;
            foreach (var item in fKTable.Values) {
                if (tableFull && DateTime.Now.Ticks - item.LastTime > fRouteLife.Ticks) {
                    fKTable.TryRemove(item.RouteId, out Route route);
                    continue;
                }
                var distance = ComputeRouteDistance(item.Node.ID, id);
                if (list.Count >= 8) {
                    if (RouteComparer.Instance.Compare(list.Keys[0], distance) >= 0) {
                        continue;
                    }
                    list.RemoveAt(0);
                }
                list.Add(distance, item.Node);
                minTime = Math.Min(fMinLastTime, item.LastTime);
            }
            fMinLastTime = Math.Max(minTime, fMinLastTime);
            return list.Values;
        }

        #region IEnumerable

        public IEnumerator<DHTNode> GetEnumerator()
        {
            var minTime = DateTime.MaxValue.Ticks;
            var tableFull = fKTable.Count >= fMaxNodeSize;
            foreach (var item in fKTable.Values) {
                if (tableFull && DateTime.Now.Ticks - item.LastTime > fRouteLife.Ticks) {
                    fKTable.TryRemove(item.RouteId, out Route route);
                    continue;
                }
                minTime = Math.Min(fMinLastTime, item.LastTime);
                yield return item.Node;
            }
            fMinLastTime = Math.Max(minTime, fMinLastTime);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}