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

using SQLite;

namespace GKNet.Database
{
    [Table("Peers")]
    internal class DBPeer
    {
        [MaxLength(40), PrimaryKey]
        public string node_id { get; set; }

        /// <summary>
        /// Port is a 16-bit unsigned number, max length for 65535 = 5.
        /// For IPv4, max length (255.255.255.255:65535) = 21.
        /// For IPv6, max length (2001:0db8:85a3:0000:0000:8a2e:0370:7334:65535) = 45.
        /// </summary>
        [MaxLength(46), NotNull]
        public string last_endpoint { get; set; }

        [MaxLength(40), NotNull]
        public string user_name { get; set; }

        [MaxLength(200)]
        public string country { get; set; }

        [MaxLength(200)]
        public string timezone { get; set; }

        [MaxLength(200)]
        public string langs { get; set; }
    }
}
