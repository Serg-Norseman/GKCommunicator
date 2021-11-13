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

using System.Collections.Generic;
using System.Net;
using BencodeNET;

namespace GKNet.DHT
{
    public class PeersFoundEventArgs : PeerEventArgs
    {
        private readonly byte[] fInfoHash;
        private readonly List<IPEndPoint> fPeers;
        private readonly BString fToken;

        public byte[] InfoHash
        {
            get { return fInfoHash; }
        }

        public List<IPEndPoint> Peers
        {
            get { return fPeers; }
        }

        public BString Token
        {
            get { return fToken; }
        }

        public PeersFoundEventArgs(IPEndPoint peerEndPoint, byte[] nodeId, byte[] infoHash, List<IPEndPoint> peers, BString token) : base(peerEndPoint, nodeId)
        {
            fInfoHash = infoHash;
            fPeers = peers;
            fToken = token;
        }
    }
}
