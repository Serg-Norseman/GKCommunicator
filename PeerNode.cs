using System.Net;

namespace DHTConnector
{
    public enum PeerType
    {
        Bootstrap,  // startup node
        Simple,     // normal dht peer
        Subnet      // peer was checked by SubnetKey's infohash and handshake
    }

    public class PeerNode
    {
        public byte[] ID;
        public IPEndPoint EndPoint;

        // time AnnounceExpire?

        public PeerNode(IPEndPoint endPoint)
        {
            ID = null;
            EndPoint = endPoint;
        }

        public PeerNode(byte[] id, IPEndPoint endPoint)
        {
            ID = id;
            EndPoint = endPoint;
        }
    }
}
