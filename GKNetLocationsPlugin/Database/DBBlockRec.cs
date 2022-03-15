/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using GKNet.Blockchain;
using SQLite;

namespace GKNetLocationsPlugin.Database
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
