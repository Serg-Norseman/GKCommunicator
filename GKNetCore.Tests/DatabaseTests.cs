using System;
using GKNet.Database;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class DatabaseTests
    {
        //#if !CI_MODE

        private IDatabase fDb;

        [TestFixtureSetUp]
        public void SetUp()
        {
            fDb = new LtDatabase();
            Assert.IsNotNull(fDb);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Test_Connection()
        {
            fDb.DeleteDatabase();
            Assert.IsFalse(fDb.IsExists);

            Assert.IsFalse(fDb.IsConnected);
            fDb.Connect();
            Assert.IsTrue(fDb.IsConnected);
            Assert.Throws(typeof(DatabaseException), () => { fDb.Connect(); }); // already connected

            fDb.Disconnect();
            Assert.IsFalse(fDb.IsConnected);
            Assert.Throws(typeof(DatabaseException), () => { fDb.Disconnect(); }); // already disconnected

            Assert.IsTrue(fDb.IsExists);
        }

        [Test]
        public void Test_Parameters()
        {
            Assert.Throws(typeof(DatabaseException), () => { fDb.GetParameterValue("user_name"); }); // disconnected
            Assert.Throws(typeof(DatabaseException), () => { fDb.SetParameterValue("user_name", "fail"); }); // disconnected

            fDb.Connect();

            Assert.AreEqual(string.Empty, fDb.GetParameterValue("user_name"));

            fDb.SetParameterValue("user_name", "Kashchei");
            Assert.AreEqual("Kashchei", fDb.GetParameterValue("user_name"));

            fDb.SetParameterValue("user_name", "Baba Yaga");
            Assert.AreEqual("Baba Yaga", fDb.GetParameterValue("user_name"));

            fDb.Disconnect();
        }

        //#endif
    }
}
