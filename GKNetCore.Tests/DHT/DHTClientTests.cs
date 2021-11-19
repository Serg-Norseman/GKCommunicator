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
            public byte[] ClientNodeId
            {
                get { return DHTHelper.GetRandomID(); }
            }

            public IList<IDHTPeer> GetPeersList()
            {
                return new List<IDHTPeer>();
            }
        }

        [Test]
        public void Test_ctor()
        {
            var peersHolder = new DHTPeersHolder();

            var dhtClient = new DHTClient(DHTClient.IPAnyAddress, DHTClient.PublicDHTPort, peersHolder, "x1");
            Assert.IsNotNull(dhtClient);
            Assert.IsNotNull(dhtClient.LocalID);
            Assert.IsNotNull(dhtClient.Socket);
            Assert.AreEqual(new IPEndPoint(DHTClient.IPAnyAddress, DHTClient.PublicDHTPort), dhtClient.LocalEndPoint);

            dhtClient.PeersFound += OnPeersFound;
            dhtClient.PeerPinged += OnPeerPinged;
            dhtClient.QueryReceived += OnQueryReceive;
            dhtClient.ResponseReceived += OnResponseReceive;

            var tid = DHTHelper.GetTransactionId();
            var msg = new DHTMessage(MessageType.Query, QueryType.Ping, null);
            dhtClient.SetTransaction(tid, msg);
            Assert.AreEqual(QueryType.Ping, dhtClient.CheckTransaction(tid));
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
    }
}
