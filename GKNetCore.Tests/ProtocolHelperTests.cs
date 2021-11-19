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

            var hexStr = infoKey.ToHexString();
            Assert.AreEqual("3E42E4C836FD3779FF6D16DA5FA65F17DB756EB9", hexStr);
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
            var peerInfo = new UserProfile();
            peerInfo.Reset();

            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateGetPeerInfoResponse(tid, nodeId, peerInfo);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }
    }
}
