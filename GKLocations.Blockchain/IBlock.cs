/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;

namespace GKLocations.Blockchain
{
    /// <summary>
    ///
    /// Will not be used: MerkleHash, Difficulty and Nonce (with PoW)
    /// </summary>
    public interface IBlock
    {
        /// <summary>
        /// Ordinal index of the block in the chain for checking chains between peers.
        /// </summary>
        long Index { get; }

        /// <summary>
        /// The version of the block specification.
        /// </summary>
        uint Version { get; }

        /// <summary>
        /// Block creation time.
        /// </summary>
        long Timestamp { get; }

        /// <summary>
        /// The hash of the previous block.
        /// </summary>
        string PreviousHash { get; }

        /// <summary>
        /// Block data.
        /// </summary>
        IList<Transaction> Transactions { get; }

        /// <summary>
        /// Block hash.
        /// </summary>
        string Hash { get; }


        string Serialize();
    }
}
