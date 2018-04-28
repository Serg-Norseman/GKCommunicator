/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GKNet.TCP
{
    public class TCPDuplexClient
    {
        private int fBacklog;
        private List<TCPConnection> fConnections = new List<TCPConnection>();
        private IPAddress fLocalAddress = IPAddress.Any;
        private int fLocalPort;
        internal readonly ILogger fLogger;
        private Socket fSocket;

        public event EventHandler<DataReceiveEventArgs> DataReceive;

        public TCPDuplexClient(ILogger logger)
        {
            fLogger = logger;
        }

        // This is the method that starts the server listening.
        public void Connect(int port)
        {
            fBacklog = 100;
            fLocalPort = port;
            // Create the new socket on which we'll be listening.
            fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // global with NAT traversal
            fSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
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
            fConnections.Clear();

            if (fSocket != null && fSocket.Connected) {
                fSocket.Shutdown(SocketShutdown.Both);
                fSocket.Close();
            }
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

        public TCPConnection GetConnection(IPEndPoint point, bool canCreate = true)
        {
            var result = fConnections.FirstOrDefault((x) => x.EndPoint.Equals(point));
            if (result == null && canCreate) {
                result = CreateConnection(point);
            }
            return result;
        }

        public TCPConnection CreateConnection(IPEndPoint point)
        {
            try {
                var extSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                extSocket.Connect(point);
                return new TCPConnection(this, extSocket, false);
            } catch (Exception ex) {
                fLogger.WriteLog("TCPDuplexClient.CreateConnection(): " + ex.Message);
                return null;
            }
        }

        /*public void Send(IPEndPoint point, string msg)
        {
            var newConn = CreateConnection(point);
            newConn.Send(Encoding.UTF8.GetBytes(msg));
        }

        public void Send(IPEndPoint point, byte[] data)
        {
            var newConn = CreateConnection(point);
            newConn.Send(data);
        }*/

        public void RaiseDataReceive(byte[] data, IPEndPoint peer)
        {
            if (DataReceive != null) {
                DataReceive(this, new DataReceiveEventArgs(data, peer));
            }
        }
    }
}