/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;

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
        ulong Index { get; }

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
    }
}
