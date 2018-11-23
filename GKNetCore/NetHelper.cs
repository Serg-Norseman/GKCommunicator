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

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace GKNet
{
    public static class NetHelper
    {
        // FIXME: Fatal problem - if there is an address statically assigned to the corporate network,
        // then there is still no correct external address
        private static IPAddress GetPublicAddress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            // weed out addresses of virtual adapters (VirtualBox, VMWare, Tunngle, etc.)
            foreach (NetworkInterface network in networkInterfaces) {
                IPInterfaceProperties properties = network.GetIPProperties();
                if (properties.GatewayAddresses.Count == 0) {
                    // all the magic is in this line
                    continue;
                }

                foreach (IPAddressInformation address in properties.UnicastAddresses) {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    return address.Address;
                }
            }

            return default(IPAddress);
        }

        public static string GetPublicIPAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return null;
            }

            try {
                //var addr = GetPublicAddress();
                //string externalIP = (addr == null) ? "" : addr.ToString();
                string externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
                return externalIP;
            } catch { return null; }
        }

        // IPv4 192.168.1.1 maps as ::FFFF:192.168.1.1
        public static IPAddress MapIPv4ToIPv6(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetworkV6) {
                return address;
            }

            byte[] addr = address.GetAddressBytes();
            byte[] labels = new byte[16];
            labels[10] = 0xFF;
            labels[11] = 0xFF;
            labels[12] = addr[0];
            labels[13] = addr[1];
            labels[14] = addr[2];
            labels[15] = addr[3];
            return new IPAddress(labels, 0);
        }
    }
}
