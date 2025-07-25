/*
 *  "BSLib.TeamsNet", the serverless peer-to-peer network library.
 *  Copyright (C) 2018-2025 by Sergey V. Zhdanovskih.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using BencodeNET;

namespace BSLib.TeamsNet.DHT
{
    public enum MessageType
    {
        Query, Response, Error, Unknown
    }

    public enum QueryType
    {
        Ping, FindNode, GetPeers, AnnouncePeer, None
    }

    public class DHTMessage
    {
        private static readonly BencodeParser fParser = new BencodeParser();

        private string fClientVer;

        protected BDictionary fData;
        protected QueryType fQueryType;
        protected MessageType fType;


        public string ClientVer
        {
            get { return fClientVer; }
        }

        public BDictionary Data
        {
            get { return fData; }
        }

        public QueryType QueryType
        {
            get { return fQueryType; }
        }

        public MessageType Type
        {
            get { return fType; }
        }


        protected DHTMessage(BDictionary data)
        {
            fData = data;
        }

        public DHTMessage(MessageType type, QueryType queryType, BDictionary data)
        {
            fType = type;
            fQueryType = queryType;
            fData = data;
        }

        public bool IsSimilarTo(string clientVer)
        {
            return (string.Compare(fClientVer, 0, clientVer, 0, 2) == 0);
        }

        protected virtual void Parse()
        {
            var clientVer = fData.Get<BString>("v");
            fClientVer = (clientVer == null) ? string.Empty : clientVer.ToString();
        }

        public override string ToString()
        {
            return fData.EncodeAsString();
        }

        public static DHTMessage ParseBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0 || buffer[0] != 'd') {
                return null;
            }

            var data = fParser.Parse<BDictionary>(buffer);

            var bsY = data.Get<BString>("y");
            if (bsY == null)
                return null;

            string strMsgType = bsY.ToString();
            MessageType msgType = DHTMessage.GetMessageType(strMsgType);

            DHTMessage result;
            switch (msgType) {
                case MessageType.Response:
                    result = new DHTResponseMessage(msgType, QueryType.None, data);
                    break;

                case MessageType.Query:
                    result = new DHTQueryMessage(msgType, QueryType.None, data);
                    break;

                case MessageType.Error:
                    result = new DHTErrorMessage(msgType, QueryType.None, data);
                    break;

                default:
                    result = new DHTMessage(msgType, QueryType.None, data);
                    break;
            }

            result.Parse();
            return result;
        }

        public static MessageType GetMessageType(string msgType)
        {
            switch (msgType) {
                case "r":
                    // on receive response
                    return MessageType.Response;

                case "q":
                    // on receive query
                    return MessageType.Query;

                case "e":
                    // on receive error
                    return MessageType.Error;

                default:
                    return MessageType.Unknown;
            }
        }

        public static QueryType GetQueryType(string queryType)
        {
            switch (queryType) {
                case "ping":
                    return QueryType.Ping;

                case "find_node":
                    return QueryType.FindNode;

                case "get_peers":
                    return QueryType.GetPeers;

                case "announce_peer":
                    return QueryType.AnnouncePeer;

                default:
                    return QueryType.None;
            }
        }


        public static DHTMessage CreatePingQuery(BString transactionID, DHTId nodeId)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "ping");

            var args = new BDictionary();
            args.Add("id", nodeId.ToBencodedString());
            sendData.Add("a", args);

            return new DHTMessage(MessageType.Query, QueryType.Ping, sendData);
        }

        public static DHTMessage CreatePingResponse(BString transactionID, DHTId nodeId)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("y", "r");
            sendData.Add("t", transactionID);

            var r = new BDictionary();
            r.Add("id", nodeId.ToBencodedString());
            sendData.Add("r", r);

            return new DHTMessage(MessageType.Response, QueryType.None, sendData);
        }

        public static DHTMessage CreateFindNodeQuery(BString transactionID, DHTId nodeId, DHTId targetNodeId)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "find_node");

            #if IP6
            var want = new BList();
            want.Add("n6");
            sendData.Add("want", want);
            #endif

            var args = new BDictionary();
            args.Add("id", nodeId.ToBencodedString());
            args.Add("target", targetNodeId.ToBencodedString());
            sendData.Add("a", args);

            return new DHTMessage(MessageType.Query, QueryType.FindNode, sendData);
        }

        public static DHTMessage CreateFindNodeResponse(BString transactionID, DHTId nodeId, IList<DHTNode> nodesList)
        {
            var nodes = new BString(DHTNode.CompactNodes(nodesList));

            BDictionary sendData = new BDictionary();

            sendData.Add("y", "r");
            sendData.Add("t", transactionID);

            var r = new BDictionary();
            r.Add("id", nodeId.ToBencodedString());
            r.Add("nodes", nodes);
            sendData.Add("r", r);

            return new DHTMessage(MessageType.Response, QueryType.None, sendData);
        }

        public static DHTMessage CreateAnnouncePeerQuery(BString transactionID, DHTId nodeId, DHTId infoHash,
            byte implied_port, int port, BString token)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "announce_peer");

            var args = new BDictionary();
            args.Add("id", nodeId.ToBencodedString());
            args.Add("implied_port", new BNumber(implied_port));
            args.Add("info_hash", infoHash.ToBencodedString());
            args.Add("port", new BNumber(port));
            args.Add("token", token);
            sendData.Add("a", args);

            return new DHTMessage(MessageType.Query, QueryType.AnnouncePeer, sendData);
        }

        public static DHTMessage CreateAnnouncePeerResponse(BString transactionID, DHTId nodeId, IList<DHTNode> nodesList)
        {
            var nodes = new BString(DHTNode.CompactNodes(nodesList));

            BDictionary sendData = new BDictionary();

            sendData.Add("y", "r");
            sendData.Add("t", transactionID);

            var r = new BDictionary();
            r.Add("id", nodeId.ToBencodedString());
            r.Add("nodes", nodes);
            sendData.Add("r", r);

            return new DHTMessage(MessageType.Response, QueryType.None, sendData);
        }

        public static DHTMessage CreateGetPeersQuery(BString transactionID, DHTId nodeId, DHTId infoHash)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "get_peers");

            #if IP6
            var want = new BList();
            want.Add("n6");
            sendData.Add("want", want);
            #endif

            var args = new BDictionary();
            args.Add("id", nodeId.ToBencodedString());
            args.Add("info_hash", infoHash.ToBencodedString());
            sendData.Add("a", args);

            return new DHTMessage(MessageType.Query, QueryType.GetPeers, sendData);
        }

        public static DHTMessage CreateGetPeersResponse(
            BString transactionID, DHTId nodeId, DHTId infoHash,
            IList<IDHTPeer> peersList, IList<DHTNode> nodesList)
        {
            BList values = DHTNode.CompactPeers(peersList);
            var nodes = new BString(DHTNode.CompactNodes(nodesList));

            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "r");

            var r = new BDictionary();
            r.Add("id", nodeId.ToBencodedString());
            r.Add("token", new BString(infoHash.Data.SubArray(0, 2)));
            if (values != null) {
                r.Add("values", values);
            }
            r.Add("nodes", nodes);
            sendData.Add("r", r);

            return new DHTMessage(MessageType.Response, QueryType.None, sendData);
        }
    }
}
