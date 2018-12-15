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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GKCore.IPC
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class IPCSocketChannel
    {
        public const int ReadTimeOut = 30000;

        protected byte[] fBuffer = new byte[512 * 1024];
        protected Socket fSocket;


        public event EventHandler<DataReceiveEventArgs> DataReceive;


        protected IPCSocketChannel(string channelName, int port)
        {
        }

        public abstract bool Connect(bool keepalive = true);

        public virtual void Disconnect(bool sendCloseMessage = true)
        {
            if (fSocket != null && fSocket.Connected) {
                fSocket.Shutdown(SocketShutdown.Both);
                fSocket.Close();
                fSocket = null;
            }
        }

        #region Private methods

        internal void RaiseDataReceive(IPEndPoint peer, IPCMessage message, IPCStream stream)
        {
            if (DataReceive != null) {
                DataReceive(this, new DataReceiveEventArgs(peer, message, stream));
            }
        }

        public void Send(IPEndPoint address, string data)
        {
            try {
                byte[] dataArray = Encoding.UTF8.GetBytes(data);
                fSocket.BeginSendTo(dataArray, 0, dataArray.Length, SocketFlags.None, address, (ar) => {
                    try {
                        fSocket.EndSendTo(ar);
                    } catch (Exception ex) {
                        //fLogger.WriteError("Send.1(" + address.ToString() + "): ", ex);
                    }
                }, null);
            } catch (Exception ex) {
                //fLogger.WriteError("Send(): ", ex);
            }
        }

        #endregion
    }
}
