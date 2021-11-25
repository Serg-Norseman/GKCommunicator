using System.Net;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class PeerTests
    {
        [Test]
        public void Test_ctor()
        {
            var peer = new Peer(new IPEndPoint(IPAddress.Any, 1111), null);
            Assert.IsNotNull(peer);
            Assert.AreEqual(PeerState.Unknown, peer.State);
            Assert.IsNotNull(peer.Profile);
            string str = peer.ToString();

            peer.Presence = PresenceStatus.Invisible;
            Assert.AreEqual(PresenceStatus.Invisible, peer.Presence);
        }
    }
}
