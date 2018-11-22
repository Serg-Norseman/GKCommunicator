using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTClientTests
    {
        private class DHTPeersHolder : IDHTPeersHolder
        {
            public IList<IDHTPeer> GetPeersList()
            {
                return new List<IDHTPeer>();
            }
        }

        [Test]
        public void Test_ctor()
        {
            var peersHolder = new DHTPeersHolder();
            
            var dhtClient = new DHTClient(IPAddress.Any, 0, peersHolder, "x1");
            Assert.IsNotNull(dhtClient);
            Assert.IsNotNull(dhtClient.LocalID);
        }
    }
}
