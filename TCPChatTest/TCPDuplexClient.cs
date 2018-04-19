using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace TCPChatTest
{
    public class TCPDuplexClient
    {
        private int fBacklog;
        private List<TCPConnection> fConnections = new List<TCPConnection>();
        private IPAddress fLocalAddress = IPAddress.Any;
        private int fLocalPort;
        private Socket fSocket;

        public event EventHandler<DataReceiveEventArgs> DataReceive;

        // This is the method that starts the server listening.
        public void Start(int port = 8080)
        {
            fLocalPort = port;
            // Create the new socket on which we'll be listening.
            fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the address and port.
            fSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            // Start listening.
            fSocket.Listen(fBacklog);
            // Set up the callback to be notified when somebody requests
            // a new connection.
            fSocket.BeginAccept(OnConnectRequest, fSocket);
        }

        public void Disconnect()
        {
            for (int i = 0; i < fConnections.Count; i++) {
                fConnections[i].Close();
            }
            fSocket.Shutdown(SocketShutdown.Both);
            fSocket.Close();
        }

        protected internal void AddConnection(TCPConnection connection)
        {
            fConnections.Add(connection);
        }

        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            TCPConnection connection = fConnections.FirstOrDefault(c => c.fId == id);
            // и удаляем его из списка подключений
            if (connection != null)
                fConnections.Remove(connection);
        }

        // This is the method that is called when the socket recives a request
        // for a new connection.
        private void OnConnectRequest(IAsyncResult result)
        {
            // Get the socket (which should be this listener's socket) from
            // the argument.
            Socket sock = (Socket)result.AsyncState;
            // Create a new client connection, using the primary socket to
            // spawn a new socket.
            TCPConnection newConn = new TCPConnection(this, sock.EndAccept(result));
            // Tell the listener socket to start listening again.
            sock.BeginAccept(OnConnectRequest, sock);
        }

        public TCPConnection CreateConnection(IPEndPoint point)
        {
            var extSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            extSocket.Connect(point);
            return new TCPConnection(this, extSocket, false);
        }

        public void Send(IPEndPoint point, string msg)
        {
            var newConn = CreateConnection(point);
            newConn.Send(msg);
        }

        public void RaiseDataReceive(byte[] data, IPEndPoint peer)
        {
            if (DataReceive != null) {
                DataReceive(this, new DataReceiveEventArgs(data, peer));
            }
        }
    }
}