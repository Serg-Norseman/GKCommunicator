using System;
using System.Collections.Generic;
using System.Linq;
using BencodeNET;
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
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateHandshakeQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateHandshakeResponse()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateHandshakeResponse(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateChatMessage()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateChatMessage(tid, nodeId, "test");
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateGetPeerInfoQuery()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateGetPeerInfoQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateGetPeerInfoResponse()
        {
            var peerInfo = new PeerProfile();
            peerInfo.ResetSystem();

            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateGetPeerInfoResponse(tid, nodeId, peerInfo);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }
    }
}
