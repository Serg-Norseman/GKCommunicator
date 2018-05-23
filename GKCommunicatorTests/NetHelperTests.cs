using System;
using System.Linq;
using System.Net.Sockets;

using GKNet.DHT;
using NUnit.Framework;
using GKNet;
using System.Net;

namespace GKCommunicatorTests
{
    [TestFixture]
    public class NetHelperTests
    {
        [Test]
        public void NetHelper_Tests()
        {
            IPAddress addr = IPAddress.Parse("192.168.1.1");
            var newAddr = NetHelper.MapToIPv6(addr);
            Assert.AreEqual("::ffff:192.168.1.1", newAddr.ToString());
            Assert.AreEqual(AddressFamily.InterNetworkV6, newAddr.AddressFamily);
        }

        [Test]
        public void DHTHelper_Tests()
        {
            var tid = DHTHelper.GetTransactionId();
            Assert.IsNotNull(tid);
            Assert.AreEqual(2, tid.Length);

            var randId = DHTHelper.GetRandomID();
            Assert.IsNotNull(randId);
            Assert.AreEqual(20, randId.Length);

            var randShaId = DHTHelper.GetRandomHashID();
            Assert.IsNotNull(randShaId);
            Assert.AreEqual(20, randShaId.Length);
        }

        [Test]
        public void UserProfile_Tests()
        {
            UserProfile profile = new UserProfile();
            
            profile.ResetSystem();
            
            Assert.IsFalse(profile.IsCountryVisible);
            profile.IsCountryVisible = true;
            Assert.IsTrue(profile.IsCountryVisible);
            
            Assert.IsFalse(profile.IsLanguagesVisible);
            profile.IsLanguagesVisible = true;
            Assert.IsTrue(profile.IsLanguagesVisible);
            
            Assert.IsFalse(profile.IsTimeZoneVisible);
            profile.IsTimeZoneVisible = true;
            Assert.IsTrue(profile.IsTimeZoneVisible);
        }
    }
}
