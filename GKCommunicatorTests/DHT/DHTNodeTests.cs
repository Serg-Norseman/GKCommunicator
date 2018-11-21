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
            var randId = DHTHelper.GetRandomID();
            var node = new DHTNode(randId, new IPEndPoint(IPAddress.Any, 0));
            Assert.IsNotNull(node);
        }
    }
}
