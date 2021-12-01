using System.Collections.Generic;
using System.Net;
using BencodeNET;
using NUnit.Framework;

namespace GKNet.DHT
{
    /// <summary>
    /// Tests for common and simple cases.
    /// </summary>
    [TestFixture]
    public class DHTCommonTests
    {
        [Test]
        public void Test_DHTNode()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            var randId = DHTHelper.GetRandomID();
            var node = new DHTNode(randId, endPoint);
            Assert.IsNotNull(node);
            Assert.AreEqual(randId, node.Id.Data);
            Assert.AreEqual(endPoint, node.EndPoint);
            Assert.IsNotNullOrEmpty(node.ToString());

            Assert.AreEqual(0, node.LastAnnouncementTime);
            Assert.AreEqual(0, node.LastUpdateTime);
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
        public void Test_PeersFoundEventArgs()
        {
            List<IPEndPoint> peers = new List<IPEndPoint>();
            var evt = new PeersFoundEventArgs(peers);
            Assert.IsNotNull(evt);
            Assert.AreEqual(peers, evt.Peers);
        }
    }
}
