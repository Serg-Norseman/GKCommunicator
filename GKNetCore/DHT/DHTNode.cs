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

using System.Net;

namespace GKNet.DHT
{
    public enum NodeType
    {
        Bootstrap,  // startup node
        Simple,     // normal dht peer
        Subnet      // peer was checked by SubnetKey's infohash and handshake
    }

    public class DHTNode
    {
        public byte[] ID { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public long LastAnnouncementTime { get; set; }
        public long LastGetPeersTime { get; set; }
        public long LastUpdateTime { get; set; }

        public string RouteId
        {
            get { return EndPoint == null ? string.Empty : EndPoint.ToString(); }
        }

        public NodeType Type { get; set; }


        public DHTNode(IPEndPoint endPoint)
        {
            ID = null;
            EndPoint = endPoint;
        }

        public DHTNode(byte[] id, IPEndPoint endPoint)
        {
            ID = id;
            EndPoint = endPoint;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", EndPoint.ToString(), ID.ToHexString());
        }
    }
}
