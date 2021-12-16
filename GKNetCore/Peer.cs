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
        Unknown,
        Unchecked,  /* remote/local peer found */
        Checked,    /* remote peer responded to ping or local peer found with its own port */
        Identified  /* peer returned profile */
    }

    public enum PresenceStatus
    {
        Unknown, Offline, Online, Away, Busy, Invisible
    }

    public class Peer : IDHTPeer
    {
        public IPEndPoint EndPoint { get; private set; }

        public DHTId ID
        {
            get { return Profile.NodeId; }
            set { Profile.NodeId = value; }
        }

        public bool Ban { get; set; }
        public bool IsLocal { get; set; }
        public PresenceStatus Presence { get; set; }
        public PeerProfile Profile { get; private set; }
        public PeerState State { get; set; }
        public DateTime LastPingTime { get; set; }
        public int PingTries { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public bool IsUnknown
        {
            get {
                return string.IsNullOrEmpty(Profile.UserName);
            }
        }

        public Peer(IPEndPoint peerEndPoint, PeerProfile profile)
        {
            EndPoint = peerEndPoint;
            State = PeerState.Unknown;
            Profile = (profile == null) ? new PeerProfile() : profile;
        }

        public override string ToString()
        {
            string location = (IsLocal) ? "local" : "external";
            string connInfo = string.Format("{0} ({1}, {2})", EndPoint, State, location);
            string peerName = (IsLocal || State == PeerState.Identified) ? Profile.UserName : "???";
            string result = string.Format("{0}\r\n{1}", peerName, connInfo);
            return result;
        }
    }
}
