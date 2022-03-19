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

using System;
using System.Collections.Generic;
using BSLib;

namespace GKNet.Blockchain
{
    /// <summary>
    /// Chain block.
    /// </summary>
    public class Block : Hashable, IBlock
    {
        /// <summary>
        /// Ordinal index of the block in the chain for checking chains between peers.
        /// </summary>
        public long Index { get; private set; }

        /// <summary>
        /// The version of the block specification.
        /// </summary>
        public uint Version { get; private set; }

        /// <summary>
        /// Block creation time.
        /// </summary>
        public long Timestamp { get; private set; }

        /// <summary>
        /// The hash of the previous block.
        /// </summary>
        public string PreviousHash { get; private set; }

        /// <summary>
        /// Block data.
        /// </summary>
        public IList<Transaction> Transactions { get; private set; }


        /// <summary>
        /// Create a block instance.
        /// </summary>
        public Block(Block previousBlock, IList<Transaction> transactions)
        {
            if (previousBlock == null) {
                throw new ArgumentNullException(nameof(previousBlock));
            }

            if (transactions == null) {
                throw new ArgumentNullException(nameof(transactions));
            }

            if (!previousBlock.IsCorrect()) {
                throw new MethodArgumentException(nameof(previousBlock), "The previous block is invalid.");
            }

            if (!transactions.IsCorrect()) {
                throw new MethodArgumentException(nameof(transactions), "Transactions is incorrect.");
            }

            Index = previousBlock.Index + 1;
            Version = BlockchainNode.CurrentVersion;
            Timestamp = TimeHelper.GetUtcNow();
            PreviousHash = previousBlock.Hash;
            Transactions = transactions;
            Hash = GetHash();

            if (!IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Block creation error. The block is invalid.");
            }
        }

        /// <summary>
        /// Create a new instance of the genesis block.
        /// </summary>
        protected Block(string previousHash, List<Transaction> transactions)
        {
            Index = 0;
            Version = BlockchainNode.CurrentVersion;
            Timestamp = TimeHelper.GetUtcNow();
            PreviousHash = previousHash;
            Transactions = transactions;
            Hash = GetHash();

            if (!IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Genesis block creation error. The block is invalid.");
            }
        }

        /// <summary>
        /// Creating a chain block from a data provider block.
        /// </summary>
        public Block(IBlock block)
        {
            if (block == null) {
                throw new ArgumentNullException(nameof(block));
            }

            Index = block.Index;
            Version = block.Version;
            Timestamp = block.Timestamp;
            PreviousHash = block.PreviousHash;
            Transactions = block.Transactions;
            Hash = block.Hash;

            if (!IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Block creation from database error. The block is invalid.");
            }
        }

        /// <summary>
        /// Get the starting (genesis) block of the block chain.
        /// </summary>
        public static Block CreateGenesisBlock(UserProfile userProfile)
        {
            var previousHash = Helpers.GetHash("5DBB70E1-34B3-4E74-87ED-EC9A4C5A5D41");
            var genesisBlock = new Block(previousHash, new List<Transaction>());
            return genesisBlock;
        }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public override string GetHashableContent()
        {
            var data = "";
            data += Index;
            data += Version;
            data += Timestamp;
            data += PreviousHash;
            data += Transactions.GetHash();
            return data;
        }

        /// <summary>
        /// Casting an object to a string.
        /// </summary>
        public override string ToString()
        {
            return Hash;
        }

        /// <summary>
        /// Deserializing a object from JSON.
        /// </summary>
        public static Block Deserialize(string json)
        {
            var data = JsonHelper.DeserializeObject<Block>(json);

            if (!data.IsCorrect()) {
                throw new MethodResultException(nameof(data), "Incorrect data after deserialization.");
            }

            return data as Block ??
                throw new FormatException("Failed to deserialize data.");
        }

        public string Serialize()
        {
            return JsonHelper.SerializeObject(this);
        }
    }
}
