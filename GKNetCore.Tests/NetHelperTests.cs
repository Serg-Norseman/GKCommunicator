using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using GKNet;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class NetHelperTests
    {
        [Test]
        public void NetHelper_Tests()
        {
            IPAddress addr = IPAddress.Parse("192.168.1.1");
            var newAddr = NetHelper.MapIPv4ToIPv6(addr);
            Assert.AreEqual("::ffff:192.168.1.1", newAddr.ToString());
            Assert.AreEqual(AddressFamily.InterNetworkV6, newAddr.AddressFamily);
        }
    }
}
