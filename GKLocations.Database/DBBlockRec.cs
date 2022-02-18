/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using GKLocations.Blockchain;
using SQLite;

namespace GKLocations.Database
{
    /// <summary>
    /// Block stored in the database.
    /// </summary>
    [Table("Blocks")]
    public class DBBlockRec : SerializableBlock
    {
        /// <summary>
        /// Identifier.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// The version of the block specification.
        /// </summary>
        public override int Version { get; set; }

        /// <summary>
        /// Block creation time.
        /// </summary>
        public override DateTime CreatedOn { get; set; }

        /// <summary>
        /// Block hash.
        /// </summary>
        public override string Hash { get; set; }

        /// <summary>
        /// The hash of the previous block.
        /// </summary>
        public override string PreviousHash { get; set; }

        /// <summary>
        /// Block data.
        /// </summary>
        public override string Data { get; set; }

        /// <summary>
        /// ID of the user who created the block.
        /// </summary>
        public override string User { get; set; }
    }
}
