/*
 *  "GKCore.IPC", the interprocess communication via TCP-sockets.
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
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace GKCore.IPC
{
    internal class TCPServerConnection
    {
        private byte[] fBuffer = new byte[65535];
        private readonly IPCServerChannel fChannel;
        private readonly Socket fSocket;
        private IPCStream fStream;


        public IPEndPoint EndPoint
        {
            get { return (IPEndPoint)fSocket.RemoteEndPoint; }
        }

        public string Id { get; private set; }


        public TCPServerConnection(IPCServerChannel channel, Socket clientSocket)
        {
            Id = Guid.NewGuid().ToString();

            fChannel = channel;
            fChannel.AddConnection(this);

            fSocket = clientSocket;
            var netStream = new NetworkStream(fSocket, false);
            netStream.ReadTimeout = IPCSocketChannel.ReadTimeOut;
            fStream = new IPCStream(netStream);

            // Start listening for incoming data.  (If you want a multi threaded service, 
            // you can start this method up in a separate thread.)

            fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None, OnBytesReceived, this);
        }

        protected void OnBytesReceived(IAsyncResult result)
        {
            try {
                int count = fSocket.EndReceive(result);
                if (count > 0) {
                    //byte[] data = new byte[count];
                    //Array.Copy(fBuffer, 0, data, 0, count);
                    //var rv = fStream.ReadMessage();

                    byte[] data = new byte[count-2];
                    Array.Copy(fBuffer, 2, data, 0, count-2);
                    var rv = fStream.ReadMessage(data);

                    fChannel.RaiseDataReceive((IPEndPoint)fSocket.RemoteEndPoint, rv, fStream);
                }

                fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None, OnBytesReceived, this);
            } catch (ObjectDisposedException ex) {
                //fDuplexClient.fLogger.WriteError("TCPConnection.OnBytesReceived(): ", ex);
            } catch (SocketException ex) {
                //fDuplexClient.fLogger.WriteError("TCPConnection.OnBytesReceived(): ", ex);
            }
        }

        public void Send(byte[] data)
        {
            fSocket.Send(data);
        }

        public void Close()
        {
            if (fSocket != null && fSocket.Connected) {
                fSocket.Shutdown(SocketShutdown.Both);
                fSocket.Close();
            }
            fChannel.RemoveConnection(this);
        }
    }

    /// <summary>
    /// TCP socket based IPC server
    /// </summary>
    public class IPCServerChannel : IPCSocketChannel
    {
        private readonly List<TCPServerConnection> fConnections;
        private readonly Dictionary<string, object> fServices = new Dictionary<string, object>();
        private readonly Dictionary<string, Type> fTypes = new Dictionary<string, Type>();


        public IPCServerChannel(string channelName, int port) : base(channelName, port)
        {
            fConnections = new List<TCPServerConnection>();
        }

        public override bool Connect(bool keepalive = true)
        {
            try {
                fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                fSocket.Bind(new IPEndPoint(IPAddress.Loopback, 5000));
                fSocket.Listen(10);

                fSocket.BeginAccept(OnConnectRequest, fSocket);
            } catch (SocketException e) {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }

            return true;
        }

        public override void Disconnect(bool sendCloseMessage = true)
        {
            for (int i = 0; i < fConnections.Count; i++) {
                fConnections[i].Close();
            }
            fConnections.Clear();

            base.Disconnect();
        }

        public void RegisterService<T>(T instance)
        {
            if (!(instance is T))
                throw new InvalidOperationException("Instance must implement service interface");

            Type typeT = typeof(T);
            fServices[typeT.Name] = instance;
            fTypes[typeT.Name] = typeT;
        }

        public void DeregisterService<T>()
        {
            Type typeT = typeof(T);
            fServices.Remove(typeT.Name);
            fTypes.Remove(typeT.Name);
        }

        #region Private methods

        private void ReceiveClients(IPCStream stream)
        {
            while (ProcessMessage(stream)) {
            }
        }

        private bool ProcessMessage(IPCStream stream)
        {
            IPCMessage msg = stream.ReadMessage();

            // this was a close-connection notification
            if (msg.StatusMsg == StatusMessage.CloseConnection)
                return false;
            else if (msg.StatusMsg == StatusMessage.Ping)
                return true;

            bool processedOk = false;
            string error = "";
            object rv = null;
            // find the service
            object instance;
            if (fServices.TryGetValue(msg.Service, out instance) && instance != null) {
                // get the method
                System.Reflection.MethodInfo method = instance.GetType().GetMethod(msg.Method);

                // double check method existence against type-list for security
                // typelist will contain interfaces instead of instances
                if (fTypes[msg.Service].GetMethod(msg.Method) != null && method != null) {
                    try {
                        // invoke method
                        rv = method.Invoke(instance, msg.Parameters);
                        processedOk = true;
                    } catch (Exception e) {
                        error = e.ToString();
                    }
                } else
                    error = "Could not find method";
            } else
                error = "Could not find service";

            // return either return value or error message
            IPCMessage returnMsg;
            if (processedOk)
                returnMsg = new IPCMessage() { Return = rv };
            else
                returnMsg = new IPCMessage() { Error = error };

            stream.WriteMessage(returnMsg);

            // if there's more to come, keep reading a next message
            if (msg.StatusMsg == StatusMessage.KeepAlive)
                return true;
            else // otherwise close the connection
                return false;
        }

        internal void AddConnection(TCPServerConnection connection)
        {
            if (connection != null) {
                fConnections.Add(connection);
            }
        }

        internal void RemoveConnection(TCPServerConnection connection)
        {
            if (connection != null) {
                fConnections.Remove(connection);
            }
        }

        private void OnConnectRequest(IAsyncResult result)
        {
            Socket sock = (Socket)result.AsyncState;
            TCPServerConnection newConn = new TCPServerConnection(this, sock.EndAccept(result));
            sock.BeginAccept(OnConnectRequest, sock);
        }

        #endregion
    }
}
