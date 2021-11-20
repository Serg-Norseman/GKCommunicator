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


        public static byte[] CreateSignInfoKey()
        {
            BDictionary subnetKey = new BDictionary();
            subnetKey.Add("info", ProtocolHelper.NETWORK_SIGN);
            return DHTHelper.CalculateInfoHashBytes(subnetKey);
        }

        public static BDictionary CreateHandshakeQuery(BString transactionID, byte[] nodeId)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "q");
            data.Add("q", "handshake");

            var args = new BDictionary();
            args.Add("id", new BString(nodeId));
            args.Add("app", "GEDKeeper Communicator");
            args.Add("ver", "2.14.0");
            data.Add("a", args);

            return data;
        }

        public static BDictionary CreateHandshakeResponse(BString transactionID, byte[] nodeId)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "r");

            var r = new BDictionary();
            r.Add("q", "handshake");
            r.Add("id", new BString(nodeId));
            r.Add("app", "GEDKeeper Communicator");
            r.Add("ver", "2.14.0");
            data.Add("r", r);

            return data;
        }

        public static BDictionary CreateChatMessage(BString transactionID, byte[] nodeId, string message)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "q");
            data.Add("q", "chat");

            var args = new BDictionary();
            args.Add("id", new BString(nodeId));
            args.Add("msg", message);
            data.Add("a", args);

            data.Add("handshake", "gkn"); // ???

            return data;
        }

        public static BDictionary CreateGetPeerInfoQuery(BString transactionID, byte[] nodeId)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "q");
            data.Add("q", "get_peer_info");

            var args = new BDictionary();
            args.Add("id", new BString(nodeId));
            data.Add("a", args);

            return data;
        }

        public static BDictionary CreateGetPeerInfoResponse(BString transactionID, byte[] nodeId, UserProfile peerProfile)
        {
            var data = new BDictionary();
            data.Add("t", transactionID);
            data.Add("y", "r");

            var r = new BDictionary();
            r.Add("q", "get_peer_info");
            r.Add("id", new BString(nodeId));
            peerProfile.Save(r);
            data.Add("r", r);

            return data;
        }
    }
}
