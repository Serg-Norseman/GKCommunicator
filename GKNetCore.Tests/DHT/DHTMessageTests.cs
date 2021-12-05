using System.Collections.Generic;
using System.Text;
using BencodeNET;
using NUnit.Framework;

namespace GKNet.DHT
{
    [TestFixture]
    public class DHTMessageTests
    {
        private static DHTMessage ParseMessage(string bencodedString)
        {
            var buffer = Encoding.UTF8.GetBytes(bencodedString);
            return DHTMessage.ParseBuffer(buffer);
        }

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
            byte[] buffer = null;
            var msg = DHTMessage.ParseBuffer(buffer);
            Assert.IsNull(msg);

            buffer = new byte[] {};
            msg = DHTMessage.ParseBuffer(buffer);
            Assert.IsNull(msg);
        }

        [Test]
        public void Test_ParseBuffer_ErrorMsg()
        {
            var msg = ParseMessage("d1:eli201e23:A Generic Error Ocurrede1:t2:aa1:y1:ee");
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
            var msg = ParseMessage("d1:ad2:id20:abcdefghij0123456789e1:q4:ping1:t2:aa1:y1:qe");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            Assert.AreEqual(QueryType.Ping, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_PingResponse()
        {
            var msg = ParseMessage("d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_FindNodeQuery()
        {
            var msg = ParseMessage("d1:ad2:id20:abcdefghij01234567896:target20:mnopqrstuvwxyz123456e1:q9:find_node1:t2:aa1:y1:qe");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            Assert.AreEqual(QueryType.FindNode, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_FindNodeResponse()
        {
            var msg = ParseMessage("d1:rd2:id20:0123456789abcdefghij5:nodes9:def456...e1:t2:aa1:y1:re");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_GetPeersQuery()
        {
            var msg = ParseMessage("d1:ad2:id20:abcdefghij01234567899:info_hash20:mnopqrstuvwxyz123456e1:q9:get_peers1:t2:aa1:y1:qe");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            Assert.AreEqual(QueryType.GetPeers, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_GetPeersResponse1()
        {
            var msg = ParseMessage("d1:rd2:id20:abcdefghij01234567895:token8:aoeusnth6:valuesl15:axje.uidhtnmbrlee1:ti0e1:y1:re");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_GetPeersResponse2()
        {
            var msg = ParseMessage("d1:rd2:id20:abcdefghij01234567895:nodes9:def456...5:token8:aoeusnthe1:ti0e1:y1:re");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_AnnouncePeerQuery()
        {
            var msg = ParseMessage("d1:ad2:id20:abcdefghij01234567899:info_hash20:mnopqrstuvwxyz1234564:porti6881e5:token8:aoeusnthe1:q13:announce_peer1:t2:aa1:y1:qe");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            Assert.AreEqual(QueryType.AnnouncePeer, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_ParseBuffer_AnnouncePeerResponse()
        {
            var msg = ParseMessage("d1:rd2:id20:mnopqrstuvwxyz123456e1:t2:aa1:y1:re");
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            Assert.AreEqual(string.Empty, msg.ClientVer);
            // TODO: test contents
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
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            DHTMessage msg = DHTMessage.CreatePingQuery(tid, nodeId);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.Ping, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreatePingResponse()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            DHTMessage msg = DHTMessage.CreatePingResponse(tid, nodeId);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }


        [Test]
        public void Test_CreateFindNodeQuery()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            DHTMessage msg = DHTMessage.CreateFindNodeQuery(tid, nodeId, nodeId);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.FindNode, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateFindNodeResponse()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            DHTMessage msg = DHTMessage.CreateFindNodeResponse(tid, nodeId, new List<DHTNode>());
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }


        [Test]
        public void Test_CreateAnnouncePeerQuery()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            var infoHash = DHTId.CreateRandom();
            DHTMessage msg = DHTMessage.CreateAnnouncePeerQuery(tid, nodeId, infoHash, 1, 1, new BString("XX"));
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.AnnouncePeer, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateAnnouncePeerResponse()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            DHTMessage msg = DHTMessage.CreateAnnouncePeerResponse(tid, nodeId, new List<DHTNode>());
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }


        [Test]
        public void Test_CreateGetPeersQuery()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            var infoHash = DHTId.CreateRandom();
            DHTMessage msg = DHTMessage.CreateGetPeersQuery(tid, nodeId, infoHash);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Query, msg.Type);
            Assert.AreEqual(QueryType.GetPeers, msg.QueryType);
            // TODO: test contents
        }

        [Test]
        public void Test_CreateGetPeersResponse()
        {
            var tid = DHTTransactions.GetNextId();
            var nodeId = DHTId.CreateRandom();
            var infoHash = DHTId.CreateRandom();
            var peers = new List<IDHTPeer>();
            var nodes = new List<DHTNode>();
            DHTMessage msg = DHTMessage.CreateGetPeersResponse(tid, nodeId, infoHash, peers, nodes);
            Assert.IsNotNull(msg);
            Assert.AreEqual(MessageType.Response, msg.Type);
            // TODO: test contents
        }
    }
}
