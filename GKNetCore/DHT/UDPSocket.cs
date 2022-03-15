/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
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
using BSLib;
using GKNet.Logging;

namespace GKNet.DHT
{
    public class UDPSocket : BaseObject
    {
#if !IP6
        public static readonly IPAddress IPLoopbackAddress = IPAddress.Loopback;
        public static readonly IPAddress IPAnyAddress = IPAddress.Any;
        public static readonly AddressFamily IPAddressFamily = AddressFamily.InterNetwork;
#else
        public static readonly IPAddress IPLoopbackAddress = IPAddress.IPv6Loopback;
        public static readonly IPAddress IPAnyAddress = IPAddress.IPv6Any;
        public static readonly AddressFamily IPAddressFamily = AddressFamily.InterNetworkV6;
#endif

        private byte[] fBuffer;
        private bool fConnected;
        private readonly IPEndPoint fLocalEndPoint;
        private readonly GKNet.Logging.ILogger fLogger;
        private IPEndPoint fPublicEndPoint;
        private readonly Socket fSocket;

        public bool Connected
        {
            get { return fConnected; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return fLocalEndPoint; }
        }

        public IPEndPoint PublicEndPoint
        {
            get { return fPublicEndPoint; }
            set { fPublicEndPoint = value; }
        }

        public Socket Socket
        {
            get { return fSocket; }
        }

        public UDPSocket(IPEndPoint localEndPoint)
        {
            fLocalEndPoint = localEndPoint;
            fBuffer = new byte[65535];
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "UDPSocket");

            fSocket = new Socket(IPAddressFamily, SocketType.Dgram, ProtocolType.Udp);
            fSocket.SetIPProtectionLevelUnrestricted();

            #if !MONO
            #if !IP6
            const long IOC_IN = 0x80000000;
            const long IOC_VENDOR = 0x18000000;
            const long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            byte[] optionInValue = { Convert.ToByte(false) };
            byte[] optionOutValue = new byte[4];
            fSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
            #else
            #endif
            #endif

#if !IP6
            fSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            fSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            fSocket.Ttl = 255;
#else
            fSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
#endif

            fSocket.Bind(fLocalEndPoint);

            fConnected = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                try {
                    fSocket.Shutdown(SocketShutdown.Both);
                } finally {
                    fSocket.Close();
                }
                fSocket.Dispose();
            }
            base.Dispose(disposing);
        }

        public void Open()
        {
            fConnected = true;
            BeginRecv();
        }

        public void Close()
        {
            fConnected = false;
        }

        private void BeginRecv()
        {
            if (!fConnected)
                return;

            EndPoint remoteAddress = new IPEndPoint(IPAnyAddress, 0);
            fSocket.BeginReceiveFrom(fBuffer, 0, fBuffer.Length, SocketFlags.None, ref remoteAddress, EndRecv, null);
        }

        private void EndRecv(IAsyncResult result)
        {
            try {
                EndPoint remoteAddress = new IPEndPoint(IPAnyAddress, 0);
                int count = fSocket.EndReceiveFrom(result, ref remoteAddress);
                if (count > 0) {
                    byte[] buffer = new byte[count];
                    Buffer.BlockCopy(fBuffer, 0, buffer, 0, count);
                    OnRecvMessage((IPEndPoint)remoteAddress, buffer);
                }
            } catch (Exception ex) {
                fLogger.WriteError("EndRecv.1()", ex);
            }

            if (!fConnected)
                return;

            bool notsuccess;
            do {
                try {
                    BeginRecv();
                    notsuccess = false;
                } catch (Exception ex) {
                    fLogger.WriteError("EndRecv.2()", ex);
                    notsuccess = true;
                }
            } while (notsuccess);
        }

        protected virtual void OnRecvMessage(IPEndPoint ipinfo, byte[] data)
        {
        }

        public void Send(IPEndPoint address, byte[] data, bool async = true)
        {
            try {
                if (async) {
                    fSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, address, (ar) => {
                        try {
                            fSocket.EndSendTo(ar);
                        } catch (Exception ex) {
                            fLogger.WriteError("Send.1(" + address + ")", ex);
                        }
                    }, null);
                } else {
                    fSocket.SendTo(data, 0, data.Length, SocketFlags.None, address);
                }
            } catch (Exception ex) {
                fLogger.WriteError("Send()", ex);
            }
        }
    }
}
