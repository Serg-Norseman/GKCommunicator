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
    public class MessageTests
    {
        [Test]
        public void Test_ctor()
        {
            var now = DateTime.Now;
            var msg = new Message(now, "test");
            Assert.IsNotNull(msg);
            Assert.AreEqual(now, msg.Timestamp);
            Assert.AreEqual("test", msg.Text);
        }
    }
}
