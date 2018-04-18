using System.Linq;
using BencodeNET.Objects;

namespace GKNet.DHT
{
    public enum MsgType
    {
        query, response, error
    }

    public enum QueryType
    {
        ping, find_node, get_peers, announce_peer, none
    }

    public class DHTMessage
    {
        public readonly MsgType Type;
        public readonly QueryType QueryType;
        public readonly BDictionary Data;

        public DHTMessage(MsgType type, QueryType queryType, BDictionary data)
        {
            Type = type;
            QueryType = queryType;
            Data = data;
        }

        public static BDictionary CreatePingQuery(BString transactionID, byte[] nodeId)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "ping");

            var args = new BDictionary();
            args.Add("id", new BString(nodeId));
            sendData.Add("a", args);

            return sendData;
        }

        public static BDictionary CreatePingResponse(BString transactionID, byte[] nid)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("y", "r");
            sendData.Add("t", transactionID);

            var r = new BDictionary();
            r.Add("id", new BString(nid));
            sendData.Add("r", r);

            return sendData;
        }

        public static BDictionary CreateFindNodeQuery(BString transactionID, byte[] nid)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "find_node");

            var args = new BDictionary();
            args.Add("id", new BString(nid));
            args.Add("target", new BString(DHTHelper.GetRandomHashID()));
            sendData.Add("a", args);

            return sendData;
        }

        public static BDictionary CreateAnnouncePeerQuery(BString transactionID, byte[] nid, byte[] infoHash,
            byte implied_port, int port, BString token)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "announce_peer");

            var args = new BDictionary();
            args.Add("id", new BString(nid));
            if (implied_port != 0) {
                args.Add("implied_port", new BNumber(implied_port));
            }
            args.Add("info_hash", new BString(infoHash));
            args.Add("port", new BNumber(port));
            args.Add("token", token);
            sendData.Add("a", args);

            return sendData;
        }

        public static BDictionary CreateAnnouncePeerResponse(BString transactionID, byte[] nid)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("y", "r");
            sendData.Add("t", transactionID);

            var r = new BDictionary();
            r.Add("id", new BString(nid));
            sendData.Add("r", r);

            return sendData;
        }

        public static BDictionary CreateGetPeersQuery(BString transactionID, byte[] nid, byte[] infoHash)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "get_peers");

            var args = new BDictionary();
            args.Add("id", new BString(nid));
            args.Add("info_hash", new BString(infoHash));
            sendData.Add("a", args);

            return sendData;
        }

        public static BDictionary CreateGetPeersResponse(BString transactionID, byte[] nid, byte[] infoHash, BList values, BString nodes)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "r");

            var r = new BDictionary();
            r.Add("id", new BString(nid));
            r.Add("token", new BString(infoHash.Take(2)));
            if (values != null) {
                r.Add("values", values);
            }
            r.Add("nodes", nodes);
            sendData.Add("r", r);

            return sendData;
        }
    }
}
