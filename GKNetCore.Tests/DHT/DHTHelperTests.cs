using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTHelperTests
    {
        [Test]
        public void Test_GetTransactionId()
        {
            var tid = DHTHelper.GetTransactionId();
            Assert.IsNotNull(tid);
            Assert.AreEqual(2, tid.Length);
        }

        [Test]
        public void Test_GetRandomID()
        {
            var randId = DHTHelper.GetRandomID();
            Assert.IsNotNull(randId);
            Assert.AreEqual(20, randId.Length);
        }

        [Test]
        public void Test_GetRandomHashID()
        {
            var randShaId = DHTHelper.GetRandomHashID();
            Assert.IsNotNull(randShaId);
            Assert.AreEqual(20, randShaId.Length);
        }

        [Test]
        public void Test_ToHexString()
        {
            var bytes = new byte[] { 0xFF, 0xAF, 0x05, 0x77, 0xAB };
            string str = bytes.ToHexString();
            Assert.AreEqual("FFAF0577AB", str);
        }

        [Test]
        public void Test_BytesToHexString()
        {
            var bytes = new byte[] { 0xFF, 0xAF, 0x05, 0x77, 0xAB };
            string str = bytes.ToHexString();
            Assert.AreEqual("FFAF0577AB", str);
        }

        [Test]
        public void Test_GetNeighbor()
        {
            var bytes1 = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11 };
            var bytes2 = new byte[] { 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77 };
            var result = DHTHelper.GetNeighbor(bytes1, bytes2);
            Assert.AreEqual("1111111111111111111177777777777777777777", result.ToHexString());
        }
    }
}
