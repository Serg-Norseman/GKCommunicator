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

using BencodeNET.Objects;
using GKNet.DHT;

namespace GKNet.Protocol
{
    public class ProtocolHelper
    {
        public const string NETWORK_SIGN = "GEDKEEPER NETWORK";

        public static byte[] CreateSignInfoKey()
        {
            BDictionary subnetKey = new BDictionary();
            subnetKey.Add("info", ProtocolHelper.NETWORK_SIGN);
            return DHTHelper.CalculateInfoHashBytes(subnetKey);
        }

        public static byte[] CreateHandshakeQuery()
        {
            var data = new BDictionary();
            data.Add("y", "q");
            data.Add("q", "handshake");

            var args = new BDictionary();
            args.Add("app", "GEDKeeper Communicator");
            args.Add("ver", "2.14.0");

            data.Add("a", args);

            return data.EncodeAsBytes();
        }
    }
}
