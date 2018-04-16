using System.Net;

namespace DHTConnector
{
    public enum NodeType
    {
        Bootstrap,  // startup node
        Simple,     // normal dht peer
        Subnet      // peer was checked by SubnetKey's infohash and handshake
    }

    public class DHTNode
    {
        public byte[] ID;
        public IPEndPoint EndPoint;

        // time AnnounceExpire?

        public DHTNode(IPEndPoint endPoint)
        {
            ID = null;
            EndPoint = endPoint;
        }

        public DHTNode(byte[] id, IPEndPoint endPoint)
        {
            ID = id;
            EndPoint = endPoint;
        }
    }
}
