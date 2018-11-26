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

            BDictionary data = new BDictionary();
            profile.Save(data);
            profile.Load(data);
        }
    }
}
