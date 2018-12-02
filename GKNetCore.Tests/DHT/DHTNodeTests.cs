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
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            var node = new DHTNode(endPoint);
            Assert.IsNotNull(node);
            Assert.IsNull(node.ID);
            Assert.AreEqual(endPoint, node.EndPoint);
        }

        [Test]
        public void Test_ctor2()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            var randId = DHTHelper.GetRandomID();
            var node = new DHTNode(randId, endPoint);
            Assert.IsNotNull(node);
            Assert.AreEqual(randId, node.ID);
            Assert.AreEqual(endPoint, node.EndPoint);

            Assert.AreEqual(0, node.LastAnnouncementTime);
            Assert.AreEqual(0, node.LastUpdateTime);
        }

        [Test]
        public void Test_ToString()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            var randId = DHTHelper.GetRandomID();
            var node = new DHTNode(randId, endPoint);
            Assert.IsNotNullOrEmpty(node.ToString());
        }
    }
}
