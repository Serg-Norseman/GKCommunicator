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

namespace GKNet.DHT
{
    public enum NodeState
    {
        Unknown,
        Good,
        Questionable,
        Bad
    }

    public class DHTNode
    {
        public static readonly long NODE_EXPIRY_TIME = TimeSpan.FromMinutes(15).Ticks;
        public static readonly long NODE_LIFE_TIME = TimeSpan.FromMinutes(3).Ticks;


        public DHTNodeId Id { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public long LastAnnouncementTime { get; set; }
        public long LastGetPeersTime { get; set; }
        public long LastUpdateTime { get; private set; }

        public NodeState State
        {
            get {
                if (LastUpdateTime == 0)
                    return NodeState.Unknown;

                long dtNowTicks = DateTime.UtcNow.Ticks;
                if (dtNowTicks - LastUpdateTime < NODE_LIFE_TIME) {
                    return NodeState.Good;
                } else {
                    return (dtNowTicks - LastUpdateTime < NODE_EXPIRY_TIME) ? NodeState.Questionable : NodeState.Bad;
                }
            }
        }


        public DHTNode(byte[] id, IPEndPoint endPoint)
        {
            Id = new DHTNodeId(id);
            EndPoint = endPoint;
        }

        public DHTNode(DHTNodeId id, IPEndPoint endPoint)
        {
            Id = id;
            EndPoint = endPoint;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", EndPoint.ToString(), Id.ToHex());
        }

        internal void Update()
        {
            LastUpdateTime = DateTime.UtcNow.Ticks;
        }
    }
}
