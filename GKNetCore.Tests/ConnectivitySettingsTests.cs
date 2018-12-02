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

            Assert.AreEqual("host", conSet.ProxyHost);
            Assert.AreEqual(1111, conSet.ProxyPort);
            Assert.AreEqual("login", conSet.ProxyUsername);
            Assert.AreEqual("password", conSet.ProxyPassword);
            Assert.AreEqual(ProxyType.Socks4, conSet.ProxyType);
            Assert.AreEqual(null, conSet.WebProxy);

            Assert.AreEqual("{ProxyHost=host, ProxyPort=1111}", conSet.ToString());

            conSet.ProxyHost = "testhost";
            Assert.AreEqual("testhost", conSet.ProxyHost);
            conSet.ProxyPort = 2222;
            Assert.AreEqual(2222, conSet.ProxyPort);
            conSet.ProxyUsername = "testuser";
            Assert.AreEqual("testuser", conSet.ProxyUsername);
            conSet.ProxyPassword = "testpass";
            Assert.AreEqual("testpass", conSet.ProxyPassword);
            conSet.ProxyType = ProxyType.Socks5;
            Assert.AreEqual(ProxyType.Socks5, conSet.ProxyType);
            conSet.WebProxy = null;
            Assert.AreEqual(null, conSet.WebProxy);
        }
    }
}
