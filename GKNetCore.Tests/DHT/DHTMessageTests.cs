using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var msg = new DHTMessage(MessageType.Query, QueryType.Ping, null);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
        }

        [Test]
        public void Test_ParseBuffer_Empty()
        {
            var msg = DHTMessage.ParseBuffer(null);
            Assert.IsNull(msg);

            msg = DHTMessage.ParseBuffer(new byte[] {});
            Assert.IsNull(msg);
        }

        [Test]
        public void Test_ParseBuffer_ErrorMsg()
        {
            var buffer = Encoding.ASCII.GetBytes("d1:eli201e23:A Generic Error Ocurrede1:t2:aa1:y1:ee");
            var msg = DHTMessage.ParseBuffer(buffer);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Error, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);

            var errMsg = msg as DHTErrorMessage;
            Assert.AreEqual(201, errMsg.ErrCode);
            Assert.AreEqual("A Generic Error Ocurred", errMsg.ErrText);
        }

        [Test]
        public void Test_ParseBuffer_PingQuery()
        {
            var buffer = Encoding.ASCII.GetBytes("d1:ad2:id20:abcdefghij0123456789e1:q4:ping1:t2:aa1:y1:qe");
            var msg = DHTMessage.ParseBuffer(buffer);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);

            //var errMsg = msg as DHTErrorMessage;
            //Assert.AreEqual(201, errMsg.ErrCode);
            //Assert.AreEqual("A Generic Error Ocurred", errMsg.ErrText);
        }

        [Test]
        public void Test_ParseBuffer_PingResponse()
        {
            var buffer = Encoding.ASCII.GetBytes("d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re");
            var msg = DHTMessage.ParseBuffer(buffer);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);

            //var errMsg = msg as DHTErrorMessage;
            //Assert.AreEqual(201, errMsg.ErrCode);
            //Assert.AreEqual("A Generic Error Ocurred", errMsg.ErrText);
        }

        [Test]
        public void Test_ParseBuffer_FindNodeQuery()
        {
            var buffer = Encoding.ASCII.GetBytes("d1:ad2:id20:abcdefghij01234567896:target20:mnopqrstuvwxyz123456e1:q9:find_node1:t2:aa1:y1:qe");
            var msg = DHTMessage.ParseBuffer(buffer);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);

            //var errMsg = msg as DHTErrorMessage;
            //Assert.AreEqual(201, errMsg.ErrCode);
            //Assert.AreEqual("A Generic Error Ocurred", errMsg.ErrText);
        }

        [Test]
        public void Test_ParseBuffer_FindNodeResponse()
        {
            var buffer = Encoding.ASCII.GetBytes("d1:rd2:id20:0123456789abcdefghij5:nodes9:def456...e1:t2:aa1:y1:re");
            var msg = DHTMessage.ParseBuffer(buffer);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);

            //var errMsg = msg as DHTErrorMessage;
            //Assert.AreEqual(201, errMsg.ErrCode);
            //Assert.AreEqual("A Generic Error Ocurred", errMsg.ErrText);
        }

        [Test]
        public void Test_GetMessageType()
        {
            Assert.AreEqual(MessageType.Response, DHTMessage.GetMessageType("r"));
            Assert.AreEqual(MessageType.Query, DHTMessage.GetMessageType("q"));
            Assert.AreEqual(MessageType.Error, DHTMessage.GetMessageType("e"));

            Assert.AreEqual(MessageType.Unknown, DHTMessage.GetMessageType("xyz"));
            Assert.AreEqual(MessageType.Unknown, DHTMessage.GetMessageType(null));
        }

        [Test]
        public void Test_GetQueryType()
        {
            Assert.AreEqual(QueryType.Ping, DHTMessage.GetQueryType("ping"));
            Assert.AreEqual(QueryType.FindNode, DHTMessage.GetQueryType("find_node"));
            Assert.AreEqual(QueryType.GetPeers, DHTMessage.GetQueryType("get_peers"));
            Assert.AreEqual(QueryType.AnnouncePeer, DHTMessage.GetQueryType("announce_peer"));

            Assert.AreEqual(QueryType.None, DHTMessage.GetQueryType("xyz"));
            Assert.AreEqual(QueryType.None, DHTMessage.GetQueryType(null));
        }


        [Test]
        public void Test_CreatePingQuery()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            DHTMessage msg = DHTMessage.CreatePingQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.Ping, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreatePingResponse()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            DHTMessage msg = DHTMessage.CreatePingResponse(tid, nodeId);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }


        [Test]
        public void Test_CreateFindNodeQuery()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            DHTMessage msg = DHTMessage.CreateFindNodeQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.FindNode, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateFindNodeResponse()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            DHTMessage msg = DHTMessage.CreateFindNodeResponse(tid, nodeId, new List<DHTNode>());
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }


        [Test]
        public void Test_CreateAnnouncePeerQuery()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var infoHash = DHTHelper.GetRandomHashID();
            DHTMessage msg = DHTMessage.CreateAnnouncePeerQuery(tid, nodeId, infoHash, 1, 1, new BString("XX"));
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.AnnouncePeer, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateAnnouncePeerResponse()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            DHTMessage msg = DHTMessage.CreateAnnouncePeerResponse(tid, nodeId, new List<DHTNode>());
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }


        [Test]
        public void Test_CreateGetPeersQuery()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var infoHash = DHTHelper.GetRandomHashID();
            DHTMessage msg = DHTMessage.CreateGetPeersQuery(tid, nodeId, infoHash);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.GetPeers, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateGetPeersResponse()
        {
            var tid = DHTHelper.GetTransactionId();
            var nodeId = DHTHelper.GetRandomID();
            var infoHash = DHTHelper.GetRandomHashID();
            var peers = new List<IDHTPeer>();
            var nodes = new List<DHTNode>();
            DHTMessage msg = DHTMessage.CreateGetPeersResponse(tid, nodeId, infoHash, peers, nodes);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }
    }
}
