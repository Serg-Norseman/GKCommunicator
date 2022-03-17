/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2022 by Sergey V. Zhdanovskih.
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

using GKNet.Blockchain;
using SQLite;

namespace GKNet.Database
{
    /// <summary>
    /// Block stored in the database.
    /// </summary>
    [Table("Blocks")]
    public class DBBlockRec
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Ordinal index of the block in the chain for checking chains between peers.
        /// </summary>
        [NotNull, Indexed]
        public long Index { get; set; }

        /// <summary>
        /// Block creation time.
        /// </summary>
        [NotNull, Indexed]
        public long Timestamp { get; set; }

        /// <summary>
        /// Block hash.
        /// </summary>
        [NotNull, Indexed]
        public string Hash { get; set; }

        /// <summary>
        /// Block data.
        /// </summary>
        [NotNull]
        public string Data { get; set; }


        public DBBlockRec()
        {
        }

        public DBBlockRec(IBlock block)
        {
            Index = block.Index;
            Timestamp = block.Timestamp;
            Hash = block.Hash;
            Data = block.Serialize();
        }

        public Block GetData()
        {
            return Block.Deserialize(Data);
        }
    }
}
