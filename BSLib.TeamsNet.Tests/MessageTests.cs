using System;
using NUnit.Framework;

namespace BSLib.TeamsNet
{
    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void Test_ctor()
        {
            var now = DateTime.UtcNow;
            var msg = new Message(now, "test", string.Empty, string.Empty);
            Assert.IsNotNull(msg);
            Assert.AreEqual(now, msg.Timestamp);
            Assert.AreEqual("test", msg.Text);
        }
    }
}
