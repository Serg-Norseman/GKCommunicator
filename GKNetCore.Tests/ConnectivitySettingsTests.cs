using System;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class ConnectivitySettingsTests
    {
        [Test]
        public void Test_ctor()
        {
            var conSet = new ConnectivitySettings();
            Assert.IsNotNull(conSet);
        }

        [Test]
        public void Test_ctor2()
        {
            var conSet = new ConnectivitySettings(new ConnectivitySettings());
            Assert.IsNotNull(conSet);
        }

        [Test]
        public void Test_ctor3()
        {
            var conSet = new ConnectivitySettings("host", 1111, "login", "password", ProxyType.Socks4);
            Assert.IsNotNull(conSet);
        }

        [Test]
        public void Test_ctor4()
        {
            var conSet = new ConnectivitySettings("host", 1111, "login", "password", ProxyType.Socks4, null);
            Assert.IsNotNull(conSet);
            Assert.AreEqual("{ProxyHost=host, ProxyPort=1111}", conSet.ToString());
        }
    }
}
