﻿/*
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
using BencodeNET.Objects;
using GKNet.DHT;

namespace GKNet
{
    public class ProtocolHelper
    {
        public const string NETWORK_SIGN = "GEDKEEPER NETWORK";
        public const string LOG_FILE = "./GKCommunicator.log";
        public const string LOG_LEVEL = "DEBUG";

        //public const int PublicTCPPort = DHTClient.PublicDHTPort;
        //public const int DebugTCPPort = DHTClient.PublicDHTPort;
        public const int PublicTCPPort = 11000;
        public const int DebugTCPPort = 11200;


        public static byte[] CreateSignInfoKey()
        {
            BDictionary subnetKey = new BDictionary();
            subnetKey.Add("info", ProtocolHelper.NETWORK_SIGN);
            return DHTHelper.CalculateInfoHashBytes(subnetKey);
        }

        public static BDictionary CreateHandshakeQuery()
        {
            var data = new BDictionary();
            data.Add("y", "q");
            data.Add("q", "handshake");

            var args = new BDictionary();
            args.Add("app", "GEDKeeper Communicator");
            args.Add("ver", "2.14.0");

            data.Add("a", args);

            return data;
        }

        public static BDictionary CreateHandshakeResponse()
        {
            var data = new BDictionary();
            data.Add("y", "r");
            data.Add("r", "handshake");

            return data;
        }

        public static BDictionary CreateChatMessage(string message)
        {
            var data = new BDictionary();
            data.Add("y", "q");
            data.Add("q", "chat");

            var args = new BDictionary();
            args.Add("msg", message);

            data.Add("a", args);

            data.Add("handshake", "gkn");

            return data;
        }

        public static BDictionary CreateGetPeerInfoQuery()
        {
            var data = new BDictionary();
            data.Add("y", "q");
            data.Add("q", "getpeerinfo");

            return data;
        }

        public static BDictionary CreateGetPeerInfoResponse()
        {
            var data = new BDictionary();
            data.Add("y", "r");
            data.Add("r", "getpeerinfo");

            var retvals = new BDictionary();
            var peerInfo = new PeerProfile();
            peerInfo.ResetSystem();
            peerInfo.Save(retvals);
            data.Add("rv", retvals);

            return data;
        }



        // Fatal problem:
        // if there is an address statically assigned to the corporate network,
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
                string externalIP = GetPublicAddress().ToString();
                /*externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();*/
                return externalIP;
            } catch { return null; }
        }
    }
}
