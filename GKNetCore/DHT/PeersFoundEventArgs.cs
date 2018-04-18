using System;
using System.Collections.Generic;
using System.Net;

namespace GKNet.DHT
{
    public class PeersFoundEventArgs : EventArgs
    {
        private byte[] fInfoHash;
        private List<IPEndPoint> fPeers;

        public PeersFoundEventArgs(byte[] infoHash, List<IPEndPoint> peers)
        {
            fInfoHash = infoHash;
            fPeers = peers;
        }

        public List<IPEndPoint> Peers
        {
            get { return fPeers; }
        }

        public byte[] InfoHash
        {
            get { return fInfoHash; }
        }
    }
}
