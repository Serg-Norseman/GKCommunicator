using System.Collections.Generic;
using System.Net;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTClientTests
    {
        private class DHTPeersHolder : IDHTPeersHolder
        {
            public DHTId ClientNodeId
            {
                get { return DHTId.CreateRandom(); }
            }

            public IList<IDHTPeer> GetPeersList()
            {
                return new List<IDHTPeer>();
            }

            public void SaveNode(DHTNode node)
            {
            }
        }

        [Test]
        public void Test_ctor()
        {
            var peersHolder = new DHTPeersHolder();

            var dhtClient = new DHTClient(new IPEndPoint(DHTClient.IPAnyAddress, DHTClient.PublicDHTPort), peersHolder, "x1");
            Assert.IsNotNull(dhtClient);
            Assert.IsNotNull(dhtClient.LocalID);
            Assert.IsNotNull(dhtClient.Socket);
            Assert.AreEqual(new IPEndPoint(DHTClient.IPAnyAddress, DHTClient.PublicDHTPort), dhtClient.LocalEndPoint);

            dhtClient.PeersFound += OnPeersFound;
            dhtClient.PeerPinged += OnPeerPinged;
            dhtClient.QueryReceived += OnQueryReceive;
            dhtClient.ResponseReceived += OnResponseReceive;
        }

        private void OnPeersFound(object sender, PeersFoundEventArgs e)
        {
        }

        private void OnPeerPinged(object sender, PeerPingedEventArgs e)
        {
        }

        private void OnQueryReceive(object sender, MessageEventArgs e)
        {
        }

        private void OnResponseReceive(object sender, MessageEventArgs e)
        {
        }

        [Test]
        public void Test_DHTTransactions_class()
        {
            var instance = new DHTTransactions();
            Assert.IsNotNull(instance);

            var tid = DHTTransactions.GetNextId();
            var msg = new DHTMessage(MessageType.Query, QueryType.Ping, null);
            instance.SetQuery(tid, msg);
            Assert.AreEqual(QueryType.Ping, instance.CheckQuery(tid));
        }
    }
}
