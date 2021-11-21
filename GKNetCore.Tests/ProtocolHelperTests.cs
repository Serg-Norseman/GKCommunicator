using System;
using System.Text;
using BSLib;
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

        [Test]
        public void Test_PGP_GenerateKeys()
        {
            string password = "password";

            string publicKey, privateKey;
            PGPUtilities.GenerateKey("John Doe", password, out publicKey, out privateKey);
        }

        [Test]
        public void Test_PGPMessagesProtection()
        {
            string password = "password";

            string publicKey, privateKey;
            PGPUtilities.GenerateKey("John Doe", password, out publicKey, out privateKey);

            string inputText = "this is my test phrase!";

            string cryptoString = PGPUtilities.PgpEncrypt(inputText, publicKey);

            string outputString = PGPUtilities.PgpDecrypt(cryptoString, privateKey, password);
            Assert.AreEqual(inputText, outputString);
        }

        [Test]
        public void Test_RSAMessagesProtection()
        {
            string password = "password";

            string publicKey, privateKey;
            Utilities.GenerateKeyPair(password, out publicKey, out privateKey);
            Assert.AreEqual(243, publicKey.Length);
            Assert.AreEqual(1260, privateKey.Length);

            byte[] pubKey = Encoding.UTF8.GetBytes(publicKey);
            Assert.AreEqual(324, Convert.ToBase64String(pubKey).Length);

            string inputText = "this is my test phrase!";

            string cryptoString = Utilities.Encrypt(inputText, publicKey);

            string outputString = Utilities.Decrypt(cryptoString, privateKey, password);
            Assert.AreEqual(inputText, outputString);
        }
    }
}
