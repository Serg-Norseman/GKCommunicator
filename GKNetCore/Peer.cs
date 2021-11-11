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
using GKNet.DHT;

namespace GKNet
{
    public enum PeerState
    {
        Unknown, Unchecked, Checked
    }

    public enum PresenceStatus
    {
        Unknown, Offline, Hidden /*Invisible?*/, Online, Away, Busy, Idle
    }

    public class Peer : IDHTPeer
    {
        public IPAddress Address { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public bool IsLocal { get; set; }
        public PresenceStatus Presence { get; set; }
        public PeerProfile Profile { get; private set; }
        public PeerState State { get; set; }
        public DateTime LastPingTime { get; set; }

        public Peer(IPAddress address, int port)
        {
            Address = address;
            EndPoint = new IPEndPoint(address, port);
            State = PeerState.Unknown;
            Profile = new PeerProfile();
        }

        public override string ToString()
        {
            string location = (IsLocal) ? "local" : "external";
            return string.Format("{0} ({1}, {2})", EndPoint, State, location);
        }
    }
}
