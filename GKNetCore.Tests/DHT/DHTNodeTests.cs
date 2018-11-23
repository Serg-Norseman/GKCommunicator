using System;
using System.Linq;
using System.Net;
using GKNet.DHT;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTNodeTests
    {
        [Test]
        public void Test_ctor()
        {
            var node = new DHTNode(new IPEndPoint(IPAddress.Any, 0));
            Assert.IsNotNull(node);
            Assert.IsNull(node.ID);
        }

        [Test]
        public void Test_ctor2()
        {
            var randId = DHTHelper.GetRandomID();
            var node = new DHTNode(randId, new IPEndPoint(IPAddress.Any, 0));
            Assert.IsNotNull(node);
            Assert.AreEqual(randId, node.ID);
        }

        [Test]
        public void Test_ToString()
        {
            var randId = DHTHelper.GetRandomID();
            var node = new DHTNode(randId, new IPEndPoint(IPAddress.Any, 0));
            Assert.IsNotNullOrEmpty(node.ToString());
        }
    }
}
