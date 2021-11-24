using GKNet.DHT;
using NUnit.Framework;

namespace GKNet
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
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateHandshakeQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateHandshakeResponse()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateHandshakeResponse(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateChatMessage()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateChatMessage(tid, nodeId, "test", false);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateGetPeerInfoQuery()
        {
            var tid = DHTTransactions.GetNextId();
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

            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTHelper.GetRandomID();
            var msg = ProtocolHelper.CreateGetPeerInfoResponse(tid, nodeId, peerInfo);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }
    }
}
