using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPChatTest
{
    public class TCPConnection
    {
        protected internal string fId;

        private byte[] fBuffer = new byte[65535];
        private TCPDuplexClient fDuplexClient;
        private Encoding fEncoding = Encoding.UTF8;
        private Socket fSocket;

        public TCPConnection(TCPDuplexClient client, Socket socket, bool receive = true)
        {
            fId = Guid.NewGuid().ToString();
            fDuplexClient = client;
            fDuplexClient.AddConnection(this);

            fSocket = socket;

            // Start listening for incoming data.  (If you want a multi-
            // threaded service, you can start this method up in a separate
            // thread.)

            if (!receive) return;
            BeginReceive();
        }

        // Call this method to set this connection's socket up to receive data.
        private void BeginReceive()
        {
            fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None,
                    new AsyncCallback(OnBytesReceived), this);
        }

        // This is the method that is called whenever the socket receives
        // incoming bytes.
        protected void OnBytesReceived(IAsyncResult result)
        {
            // End the data receiving that the socket has done and get
            // the number of bytes read.
            int nBytesRec = fSocket.EndReceive(result);
            // If no bytes were received, the connection is closed (at
            // least as far as we're concerned).
            if (nBytesRec <= 0) {
                fSocket.Close();
                return;
            }

            // Convert the data we have to a string.
            string strReceived = fEncoding.GetString(fBuffer, 0, nBytesRec);

            IPEndPoint endPoint = (IPEndPoint)fSocket.RemoteEndPoint;
            //endPoint = IPEndPoint.
            fDuplexClient.RaiseDataReceive(fEncoding.GetBytes(strReceived), new IPEndPoint(IPAddress.Any, 0));

            // ...Now, do whatever works best with the string data.
            // You could, for example, look at each character in the string
            // one-at-a-time and check for characters like the "end of text"
            // character ('\u0003') from a client indicating that they've finished
            // sending the current message.  It's totally up to you how you want
            // the protocol to work.

            // Whenever you decide the connection should be closed, call 
            // sock.Close() and don't call sock.BeginReceive() again.  But as long 
            // as you want to keep processing incoming data...

            // Set up again to get the next chunk of data.
            fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None,
                new AsyncCallback(OnBytesReceived), this);
        }

        public void Send(string msg)
        {
            fSocket.Send(Encoding.UTF8.GetBytes(msg));
        }

        public void Close()
        {
            fDuplexClient.RemoveConnection(fId);

            fSocket.Shutdown(SocketShutdown.Both);
            fSocket.Close();
        }
    }
}