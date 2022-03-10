/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Block serializable to Json.
    /// </summary>
    public class SerializableBlock : IBlock
    {
        /// <summary>
        /// Ordinal index of the block in the chain for checking chains between peers.
        /// </summary>
        public virtual ulong Index { get; set; }

        /// <summary>
        /// The version of the block specification.
        /// </summary>
        public virtual uint Version { get; set; }

        /// <summary>
        /// Block creation time.
        /// </summary>
        public virtual DateTime Timestamp { get; set; }

        /// <summary>
        /// The hash of the previous block.
        /// </summary>
        public virtual string PreviousHash { get; set; }

        /// <summary>
        /// Block data.
        /// </summary>
        public virtual string Transactions { get; set; }

        /// <summary>
        /// Block hash.
        /// </summary>
        public virtual string Hash { get; set; }


        public SerializableBlock()
        {
        }

        public SerializableBlock(Block block)
        {
            Index = block.Index;
            Version = block.Version;
            Timestamp = block.Timestamp;
            PreviousHash = block.PreviousHash;
            Transactions = Helpers.SerializeTransactions(block.Transactions);
            Hash = block.Hash;
        }
    }
}
