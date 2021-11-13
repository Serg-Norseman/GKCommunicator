using System.Collections.Generic;
using System.Net;
using BencodeNET;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTClientTests
    {
        private class DHTPeersHolder : IDHTPeersHolder
        {
            public IPAddress NATExternalIP
            {
                get;
                set;
            }

            public int NATExternalPort
            {
                get;
                set;
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

        [Test]
        public void Test_PeersFoundEventArgs()
        {
            IPEndPoint peerEndPoint = new IPEndPoint(IPAddress.Any, 1111);
            byte[] nodeId = DHTHelper.GetRandomID();
            byte[] infoHash = DHTHelper.GetRandomHashID();
            List<IPEndPoint> peers = new List<IPEndPoint>();
            var evt = new PeersFoundEventArgs(peerEndPoint, nodeId, infoHash, peers, null);
            Assert.IsNotNull(evt);
            Assert.AreEqual(infoHash, evt.InfoHash);
            Assert.AreEqual(peers, evt.Peers);
        }

        [Test]
        public void Test_PeerPingedEventArgs()
        {
            IPEndPoint peerEndPoint = new IPEndPoint(IPAddress.Any, 1111);
            byte[] nodeId = DHTHelper.GetRandomID();
            var evt = new PeerPingedEventArgs(peerEndPoint, nodeId);
            Assert.IsNotNull(evt);
            Assert.AreEqual(peerEndPoint, evt.EndPoint);
            Assert.AreEqual(nodeId, evt.NodeId);
        }

        [Test]
        public void Test_MessageEventArgs()
        {
            IPEndPoint peerEndPoint = new IPEndPoint(IPAddress.Any, 1111);
            byte[] nodeId = DHTHelper.GetRandomID();
            BDictionary data = new BDictionary();
            var evt = new MessageEventArgs(peerEndPoint, nodeId, data);
            Assert.IsNotNull(evt);
            Assert.AreEqual(peerEndPoint, evt.EndPoint);
            Assert.AreEqual(nodeId, evt.NodeId);
            Assert.AreEqual(data, evt.Data);
        }
    }
}
