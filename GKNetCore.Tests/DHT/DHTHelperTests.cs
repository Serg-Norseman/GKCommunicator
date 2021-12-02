using System.Collections.Generic;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTHelperTests
    {
        [Test]
        public void Test_GetTransactionId()
        {
            var tid = DHTTransactions.GetNextId();
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

        [Test]
        public void Test_ComputeRouteDistance()
        {
            var bytes1 = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11 };
            var bytes2 = new byte[] { 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77 };
            var result1 = DHTRoutingTable.ComputeRouteDistance(bytes1, bytes2);
            Assert.AreEqual("6666666666666666666666666666666666666666", result1.ToHexString());

            bytes1 = new byte[] { 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22 };
            bytes2 = new byte[] { 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 };
            var result2 = DHTRoutingTable.ComputeRouteDistance(bytes1, bytes2);
            Assert.AreEqual("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", result2.ToHexString());

            // result1 is less than result2, result1 is first

            var list = new SortedList<byte[], DHTNode>(8, DHTRoutingTable.RouteComparer.Instance);
            list.Add(result2, null);
            list.Add(result1, null);

            Assert.AreEqual(result1, list.Keys[0]);
            Assert.AreEqual(result2, list.Keys[1]);
        }

        [Test]
        public void Test_RouteComparer()
        {
            var comparer = DHTRoutingTable.RouteComparer.Instance;

            // x is less than y => -1
            Assert.AreEqual(-1, comparer.Compare(
                new byte[] { 0x00, 0x00, 0x00, 0x22 },
                new byte[] { 0x00, 0x00, 0x00, 0x99 }));

            // x is greater than y => +1
            Assert.AreEqual(+1, comparer.Compare(
                new byte[] { 0x00, 0x00, 0x00, 0x23 },
                new byte[] { 0x00, 0x00, 0x00, 0x20 }));

            // x is greater than y => +1
            Assert.AreEqual(+1, comparer.Compare(
                new byte[] { 0x00, 0x00, 0x01, 0x00 },
                new byte[] { 0x00, 0x00, 0x00, 0x20 }));

            // x equals y => 0
            Assert.AreEqual(0, comparer.Compare(
                new byte[] { 0x00, 0x00, 0x10, 0x00 },
                new byte[] { 0x00, 0x00, 0x10, 0x00 }));
        }
    }
}
