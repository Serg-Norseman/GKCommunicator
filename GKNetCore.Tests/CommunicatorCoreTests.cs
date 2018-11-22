using System;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class CommunicatorCoreTests
    {
        private class ChatForm : IChatForm
        {
            public void OnJoin(Peer member)
            {
            }

            public void OnLeave(Peer member)
            {
            }

            public void OnMessageReceived(Peer sender, string message)
            {
            }

            public void OnPeersListChanged()
            {
            }
        }

        [Test]
        public void Test_ctor()
        {
            Assert.Throws(typeof(ArgumentNullException), () => { new CommunicatorCore(null); });

            var chatForm = new ChatForm();

            var core = new CommunicatorCore(chatForm);
            Assert.IsNotNull(core);
            Assert.AreEqual(false, core.IsConnected);

            var peer = core.AddPeer(IPAddress.Any, 1111);
            Assert.IsNotNull(peer);

            peer = core.FindPeer(IPAddress.Any);
            Assert.IsNotNull(peer);
        }

        [Test]
        public void Test_AddPeer_FindPeer()
        {
        }
    }
}
