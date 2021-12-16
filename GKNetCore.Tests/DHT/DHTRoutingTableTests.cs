using System.Net;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTRoutingTableTests
    {
        [Test]
        public void Test_ctor()
        {
            var table = new DHTRoutingTable(10);
            Assert.IsNotNull(table);

            Assert.AreEqual(0, table.Count);

            table.Clear();

            var randId = DHTId.CreateRandom();
            var nodes = table.GetClosest(randId.Data);
        }

        [Test]
        public void Test_FindNode()
        {
            var table = new DHTRoutingTable(10);

            var randId = DHTId.CreateRandom();
            var nodeEndPoint = new IPEndPoint(IPAddress.Any, 6881);
            var node = new DHTNode(randId, nodeEndPoint);

            table.UpdateNode(node);

            var node2 = table.FindNode(nodeEndPoint);
            Assert.IsNotNull(node2);

            node2 = table.FindNode(null);
            Assert.IsNull(node2);
        }

        [Test]
        public void Test_FindNodes()
        {
            var table = new DHTRoutingTable(10);

            var randId = DHTId.CreateRandom();
            var nodes = table.GetClosest(randId.Data);
            Assert.IsNotNull(nodes);
            Assert.AreEqual(0, nodes.Count);

            var node = new DHTNode(randId, new IPEndPoint(IPAddress.Any, 6881));
            table.UpdateNode(node);
            nodes = table.GetClosest(randId.Data);
            Assert.IsNotNull(nodes);
            Assert.AreEqual(1, nodes.Count);
        }

        [Test]
        public void Test_UpdateNode()
        {
            var table = new DHTRoutingTable(10);

            table.UpdateNode(null);

            var node = new DHTNode(DHTId.CreateRandom(), new IPEndPoint(IPAddress.Any, 0));
            table.UpdateNode(node);
        }

        [Test]
        public void Test_UpdateNode_DDoS_Protection()
        {
            var table = new DHTRoutingTable(10);

            // do not use nodes with ports below 1024
            var node = new DHTNode(DHTId.CreateRandom(), new IPEndPoint(IPAddress.Any, 111));
            var result = table.UpdateNode(node);
            Assert.IsFalse(result);
        }
    }
}
