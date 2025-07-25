/*
 *  "BSLib.TeamsNet", the serverless peer-to-peer network library.
 *  Copyright (C) 2018-2025 by Sergey V. Zhdanovskih.
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

namespace BSLib.TeamsNet.DHT
{
    public abstract class PeerEventArgs : EventArgs
    {
        public IPEndPoint EndPoint { get; private set; }
        public DHTId NodeId { get; private set; }

        protected PeerEventArgs(IPEndPoint endPoint, DHTId nodeId)
        {
            EndPoint = endPoint;
            NodeId = nodeId;
        }
    }
}
