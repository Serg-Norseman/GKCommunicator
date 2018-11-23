using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using BencodeNET.Objects;
using GKNet;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public void Test_ctor()
        {
            var db = new Database();
            Assert.IsNotNull(db);
        }

        [Test]
        public void Test_Connection()
        {
            var db = new Database();

            Database.DeleteDatabase();
            Assert.IsFalse(db.IsExists);

            Assert.IsFalse(db.IsConnected);
            db.Connect();
            Assert.IsTrue(db.IsConnected);
            Assert.Throws(typeof(DatabaseException), () => { db.Connect(); }); // already connected

            db.Disconnect();
            Assert.IsFalse(db.IsConnected);
            Assert.Throws(typeof(DatabaseException), () => { db.Disconnect(); }); // already disconnected

            Assert.IsTrue(db.IsExists);
        }

        [Test]
        public void Test_Parameters()
        {
            var db = new Database();

            Assert.Throws(typeof(DatabaseException), () => { db.GetParameterValue("user_name"); }); // disconnected
            Assert.Throws(typeof(DatabaseException), () => { db.SetParameterValue("user_name", "fail"); }); // disconnected

            db.Connect();

            Assert.AreEqual(string.Empty, db.GetParameterValue("user_name"));

            db.SetParameterValue("user_name", "Kashchei");
            Assert.AreEqual("Kashchei", db.GetParameterValue("user_name"));

            db.SetParameterValue("user_name", "Baba Yaga");
            Assert.AreEqual("Baba Yaga", db.GetParameterValue("user_name"));

            db.Disconnect();
        }
    }
}
