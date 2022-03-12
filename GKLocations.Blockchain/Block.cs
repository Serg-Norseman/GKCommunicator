/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using GKLocations.Utils;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Chain block.
    /// </summary>
    public class Block : IBlock, IHashable
    {
        /// <summary>
        /// Ordinal index of the block in the chain for checking chains between peers.
        /// </summary>
        public ulong Index { get; private set; }

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
        public List<Transaction> Transactions { get; private set; }

        /// <summary>
        /// Block hash.
        /// </summary>
        public string Hash { get; private set; }


        /// <summary>
        /// Create a block instance.
        /// </summary>
        public Block(Block previousBlock, List<Transaction> transactions)
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
            Version = Chain.CurrentVersion;
            Timestamp = TimeHelper.GetUtcNow();
            PreviousHash = previousBlock.Hash;
            Transactions = transactions;
            Hash = this.GetHash();

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Block creation error. The block is invalid.");
            }
        }

        /// <summary>
        /// Create a new instance of the genesis block.
        /// </summary>
        protected Block(string previousHash, List<Transaction> transactions)
        {
            Index = 0;
            Version = Chain.CurrentVersion;
            Timestamp = TimeHelper.GetUtcNow();
            PreviousHash = previousHash;
            Transactions = transactions;
            Hash = this.GetHash();

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Genesis block creation error. The block is invalid.");
            }
        }

        /// <summary>
        /// Creating a chain block from a data provider block.
        /// </summary>
        public Block(SerializableBlock block)
        {
            if (block == null) {
                throw new ArgumentNullException(nameof(block));
            }

            Index = block.Index;
            Version = block.Version;
            Timestamp = block.Timestamp;
            PreviousHash = block.PreviousHash;
            Transactions = Helpers.DeserializeTransactions(block.Transactions);
            Hash = block.Hash;

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Block creation from database error. The block is invalid.");
            }
        }

        /// <summary>
        /// Get the starting (genesis) block of the block chain.
        /// </summary>
        public static Block CreateGenesisBlock(User user)
        {
            var previousHash = Helpers.GetHash("5DBB70E1-34B3-4E74-87ED-EC9A4C5A5D41");
            var genesisBlock = new Block(previousHash, new List<Transaction>());
            return genesisBlock;
        }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public string GetHashableContent()
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

        public string GetJson()
        {
            return JsonHelper.SerializeObject(this);
        }
    }
}
