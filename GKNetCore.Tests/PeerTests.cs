using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using BencodeNET;
using GKNet;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class PeerTests
    {
        [Test]
        public void Test_ctor()
        {
            var peer = new Peer(IPAddress.Any, 0);
            Assert.IsNotNull(peer);
            Assert.AreEqual(PeerState.Unknown, peer.State);
            Assert.IsNotNull(peer.Profile);
            string str = peer.ToString();

            peer.Presence = PresenceStatus.Hidden;
            Assert.AreEqual(PresenceStatus.Hidden, peer.Presence);
        }
    }
}
