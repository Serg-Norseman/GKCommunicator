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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GKCore.Proxy;

namespace GKCore.IPC
{
    public class Proxy : ProxyBase
    {
        public IPCClientChannel Client { get; private set; }

        public Proxy(IPCClientChannel client)
        {
            Client = client;
        }

        protected override object Invoke(string @namespace, string methodName, object[] arguments)
        {
            return Client.SendMessage(@namespace, methodName, arguments);
        }

        protected override TRet ConvertReturnValue<TRet>(object returnValue)
        {
            if (typeof(TRet) != typeof(int))
                return default(TRet);
            return (TRet)returnValue;
        }
    }


    /// <summary>
    /// TCP socket based IPC client
    /// </summary>
    public class IPCClientChannel : IPCSocketChannel
    {
        private Timer fPingTimer;
        private IPCStream fStream;

        public IPCClientChannel(string channelName, int port) : base(channelName, port)
        {
        }

        public override bool Connect(bool keepalive = true)
        {
            try {
                fSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                fSocket.ReceiveBufferSize = 2 * fBuffer.Length;
                fSocket.Connect(new IPEndPoint(IPAddress.Loopback, 5000));

                //EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
                //fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndClientRecv, null);

                fStream = new IPCStream(new NetworkStream(fSocket, false));
            } catch (SocketException e) {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }

            if (keepalive) {
                fPingTimer = new Timer(
                    (object state) => { SendMessage(new IPCMessage(StatusMessage.Ping)); },
                    null, IPCSocketChannel.ReadTimeOut / 2, IPCSocketChannel.ReadTimeOut / 2);
            }

            return true;
        }

        public override void Disconnect(bool sendCloseMessage = true)
        {
            if (sendCloseMessage) {
                fPingTimer.Dispose();
                fStream.WriteMessage(new IPCMessage(StatusMessage.CloseConnection));
            }

            if (fStream != null) {
                fStream.Dispose();
                fStream = null;
            }

            base.Disconnect();
        }

        public T GetServiceProxy<T>()
        {
            return (T)ProxyEmitter.CreateProxy(typeof(Proxy), typeof(T), this);
        }

        public object SendMessage(string service, string method, params object[] parameters)
        {
            return SendMessage(new IPCMessage(service, method, parameters));
        }

        protected object SendMessage(IPCMessage message)
        {
            bool closeStream = false;
            if (fStream == null) {
                if (!Connect(false))
                    throw new TimeoutException("Unable to connect");
                closeStream = true;
            } else if (message.StatusMsg == StatusMessage.None) {
                message.StatusMsg = StatusMessage.KeepAlive;
            }

            IPCMessage rv = null;
            lock (fStream) {
                fStream.WriteMessage(message);

                if (message.StatusMsg == StatusMessage.Ping)
                    return null;

                rv = fStream.ReadMessage();
            }

            if (closeStream)
                Disconnect(false);

            if (rv != null) {
                if (rv.Error != null) {
                    //throw new InvalidOperationException(rv.Error);
                    rv.Return = rv.Error;
                }

                return rv.Return;
            } else {
                return null;
            }
        }

        #region Private methods

        private void EndClientRecv(IAsyncResult result)
        {
            try {
                EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
                int count = fSocket.EndReceiveFrom(result, ref remoteAddress);
                if (count > 0) {
                    byte[] buffer = new byte[count];
                    Array.Copy(fBuffer, 0, buffer, 0, count);
                    RaiseDataReceive((IPEndPoint)remoteAddress, null, fStream);
                }
            } catch (Exception ex) {
                //fLogger.WriteError("DHTClient.EndRecv.1(): ", ex);
            }

            bool notsuccess = false;
            do {
                try {
                    EndPoint remoteAddress = new IPEndPoint(IPAddress.Loopback, 0);
                    fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndClientRecv, null);
                    notsuccess = false;
                } catch (Exception ex) {
                    //fLogger.WriteError("DHTClient.EndRecv.2(): ", ex);
                    notsuccess = true;
                }
            } while (notsuccess);
        }

        #endregion
    }
}
