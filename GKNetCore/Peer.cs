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

namespace GKNet
{
    public enum PeerState
    {
        Unknown, Unchecked, Checked
    }

    public class Peer
    {
        public IPAddress Address { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public PeerState State { get; set; }
        public PeerProfile Profile { get; private set; }

        public Peer(IPAddress address, int port)
        {
            Address = address;
            EndPoint = new IPEndPoint(address, port);
            State = PeerState.Unknown;
            Profile = new PeerProfile();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", EndPoint.ToString(), State.ToString());
        }
    }
}
