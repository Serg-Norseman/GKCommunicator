using System;
using System.Collections.Generic;
using System.Linq;
using BencodeNET.Objects;
using GKNet.DHT;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTMessageTests
    {
        [Test]
        public void Test_ctor()
        {
            var msg = new DHTMessage(MsgType.query, QueryType.ping, null);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MsgType.query, msg.Type);
        }

        [Test]
        public void Test_CreatePingQuery()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            BDictionary msg = DHTMessage.CreatePingQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreatePingResponse()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            BDictionary msg = DHTMessage.CreatePingResponse(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateFindNodeQuery()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            BDictionary msg = DHTMessage.CreateFindNodeQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateFindNodeResponse()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            BDictionary msg = DHTMessage.CreateFindNodeResponse(tid, nodeId, new List<DHTNode>());
            Assert.IsNotNull(msg);
            // TODO: test contents
        }
    }
}
