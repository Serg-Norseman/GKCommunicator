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

using System.Collections.Generic;

namespace GKNet.Blockchain
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
