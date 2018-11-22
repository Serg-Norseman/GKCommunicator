using System;
using System.Collections.Generic;
using System.Linq;
using BencodeNET.Objects;
using GKNet.DHT;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class ProtocolHelperTests
    {
        [Test]
        public void Test_CreateSignInfoKey()
        {
            var infoKey = ProtocolHelper.CreateSignInfoKey();
            Assert.IsNotNull(infoKey);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateHandshakeQuery()
        {
            var msg = ProtocolHelper.CreateHandshakeQuery();
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateHandshakeResponse()
        {
            var msg = ProtocolHelper.CreateHandshakeResponse();
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateChatMessage()
        {
            var msg = ProtocolHelper.CreateChatMessage("test");
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateGetPeerInfoQuery()
        {
            var msg = ProtocolHelper.CreateGetPeerInfoQuery();
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateGetPeerInfoResponse()
        {
            var msg = ProtocolHelper.CreateGetPeerInfoResponse();
            Assert.IsNotNull(msg);
            // TODO: test contents
        }
    }
}
