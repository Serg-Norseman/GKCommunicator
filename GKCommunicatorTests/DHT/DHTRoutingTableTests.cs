using System;
using System.Linq;
using GKNet.DHT;
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
            Assert.AreEqual(false, table.IsFull);

            table.Clear();

            var randId = DHTHelper.GetRandomID();
            var nodes = table.FindNodes(randId);
        }
    }
}
