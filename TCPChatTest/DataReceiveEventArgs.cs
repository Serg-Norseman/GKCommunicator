using System;
using System.Net;

namespace TCPChatTest
{
    public class DataReceiveEventArgs : EventArgs
    {
        private byte[] fData;
        private IPEndPoint fPeer;

        public DataReceiveEventArgs(byte[] data, IPEndPoint peer)
        {
            fData = data;
            fPeer = peer;
        }

        public IPEndPoint Peer
        {
            get { return fPeer; }
        }

        public byte[] Data
        {
            get { return fData; }
        }
    }
}