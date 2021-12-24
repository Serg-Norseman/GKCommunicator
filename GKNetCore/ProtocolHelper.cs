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
using System.Security.Cryptography;
using BencodeNET;
using GKNet.DHT;

namespace GKNet
{
    public static class ProtocolHelper
    {
        public const string NETWORK_SIGN = "GEDKEEPER NETWORK";
        public const string CLIENT_VER = "GC01";

        public const string LOG_FILE = "GKCommunicator.log";
        public const string LOG_LEVEL = "DEBUG"; // "DEBUG";

        public const int PublicTCPPort = 11000; // DHTClient.PublicDHTPort;

        // Confident acceptance by any host
        // (Min IP Packet Size) — (Max IP Header Size) — (UDP Header Size) = 576 — 60 — 8 = 508
        public const int MAX_UDP_DATA_SIZE_G = 508;
        // Confident acceptance without fragmentation
        // MTU — (Max IP Header Size) — (UDP Header Size) = 1500 — 60 — 8 = 1432
        public const int MAX_UDP_DATA_SIZE_UF = 1432;

        private static SHA1 sha1 = new SHA1CryptoServiceProvider();


        internal const string MSG_SIGN_CHAT = "chat";


        public static DHTId CreateSignInfoKey()
        {
            BDictionary subnetKey = new BDictionary();
            subnetKey.Add("info", ProtocolHelper.NETWORK_SIGN);

            lock (sha1) {
                return new DHTId(sha1.ComputeHash(subnetKey.EncodeAsBytes()));
            }
        }

        public static BDictionary CreateHandshakeQuery(BString transactionID, DHTId nodeId)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "q");
            data.Add("q", "handshake");

            var args = new BDictionary();
            args.Add("id", nodeId.ToBencodedString());
            args.Add("app", "GEDKeeper Communicator");
            args.Add("ver", "2.14.0");
            data.Add("a", args);

            return data;
        }

        public static BDictionary CreateHandshakeResponse(BString transactionID, DHTId nodeId, PresenceStatus presence)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "r");

            var r = new BDictionary();
            r.Add("q", "handshake");
            r.Add("id", nodeId.ToBencodedString());
            r.Add("app", "GEDKeeper Communicator");
            r.Add("ver", "2.14.0");
            r.Add("presence", new BNumber((int)presence));
            data.Add("r", r);

            return data;
        }

        public static BDictionary CreateChatMessage(BString transactionID, DHTId nodeId, string message, bool encrypted, long timestamp)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "q");
            data.Add("q", MSG_SIGN_CHAT);

            var args = new BDictionary();
            args.Add("id", nodeId.ToBencodedString());
            args.Add("msg", message);
            args.Add("enc", Convert.ToInt32(encrypted));
            args.Add("ts", timestamp);
            data.Add("a", args);

            data.Add("handshake", "gkn"); // ???

            return data;
        }

        public static BDictionary CreateChatResponse(BString transactionID, DHTId nodeId, long timestamp)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "r");

            var r = new BDictionary();
            r.Add("q", MSG_SIGN_CHAT);
            r.Add("id", nodeId.ToBencodedString());
            r.Add("ts", timestamp);
            data.Add("r", r);

            return data;
        }

        public static BDictionary CreateGetPeerInfoQuery(BString transactionID, DHTId nodeId)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "q");
            data.Add("q", "get_peer_info");

            var args = new BDictionary();
            args.Add("id", nodeId.ToBencodedString());
            data.Add("a", args);

            return data;
        }

        public static BDictionary CreateGetPeerInfoResponse(BString transactionID, DHTId nodeId, UserProfile peerProfile)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "r");

            var r = new BDictionary();
            r.Add("q", "get_peer_info");
            r.Add("id", nodeId.ToBencodedString());
            peerProfile.Save(r);
            data.Add("r", r);

            return data;
        }
    }
}
