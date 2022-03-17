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
using System.Linq;

namespace GKNet.Blockchain
{
    /// <summary>
    /// Blockchain.
    /// </summary>
    public class Chain
    {
        public const int CurrentVersion = 1;

        private List<Block> fBlocks;
        private IDataProvider fDataProvider;
        private IBlockchainNode fNode;


        public IEnumerable<Block> Blocks
        {
            get {
                return fBlocks;
            }
        }

        public int Length
        {
            get {
                return fBlocks.Count;
            }
        }

        public Block PreviousBlock
        {
            get {
                return fBlocks.Last();
            }
        }


        /// <summary>
        /// Create a new block chain instance.
        /// </summary>
        public Chain(IBlockchainNode network, IDataProvider dataProvider)
        {
            if (network == null) {
                throw new ArgumentNullException(nameof(network));
            }

            if (dataProvider == null) {
                throw new ArgumentNullException(nameof(dataProvider));
            }

            fBlocks = new List<Block>();
            fNode = network;
            fDataProvider = dataProvider;
        }

        /// <summary>
        /// Create a block chain from a data provider block list.
        /// </summary>
        private Chain(IList<Block> blocks)
        {
            if (blocks == null) {
                throw new ArgumentNullException(nameof(blocks));
            }

            fBlocks = new List<Block>();

            foreach (var block in blocks) {
                var b = new Block(block);
                fBlocks.Add(b);
            }

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "Error creating block chain. The chain is incorrect.");
            }
        }

        /// <summary>
        /// Creation of a chain of blocks from blocks of data.
        /// </summary>
        private Chain(List<Block> blocks)
        {
            if (blocks == null) {
                throw new ArgumentNullException(nameof(blocks));
            }

            foreach (var block in blocks) {
                fBlocks.Add(block);
            }

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "Error creating block chain. The chain is incorrect.");
            }
        }

        public void CreateNewBlock()
        {
            var pendingTransactions = fDataProvider.GetPendingTransactions();
            if (pendingTransactions.Count <= 0) {
                return;
            }

            var newBlock = new Block(PreviousBlock, pendingTransactions);
            AddBlock(newBlock);

            fDataProvider.ClearPendingTransactions();
        }

        public void CreateGenesisBlock()
        {
            var genesisBlock = Block.CreateGenesisBlock(GetCurrentUser());
            AddBlock(genesisBlock);
        }

        /// <summary>
        /// Create a new empty block chain.
        /// </summary>
        public void CreateNewBlockChain()
        {
            fDataProvider.ClearBlocks();
            fBlocks = new List<Block>();

            CreateGenesisBlock();
        }

        /// <summary>
        /// Get data from the local chain.
        /// </summary>
        private void LoadDataFromLocalChain(Chain localChain)
        {
            if (localChain == null) {
                throw new ArgumentNullException(nameof(localChain));
            }

            foreach (var block in localChain.fBlocks) {
                fBlocks.Add(block);
                ProcessBlockTransactions(block);
            }
        }

        /// <summary>
        /// Replace the local data chain with blocks from the global chain.
        /// </summary>
        private void ReplaceLocalChainFromGlobalChain(Chain globalChain)
        {
            if (globalChain == null) {
                throw new ArgumentNullException(nameof(globalChain));
            }

            // TODO: Develop a merge algorithm
            fDataProvider.ClearBlocks();

            foreach (var block in globalChain.fBlocks) {
                AddBlock(block);
            }
        }

        /// <summary>
        /// Get the current user of the system.
        /// </summary>
        public UserProfile GetCurrentUser()
        {
            return fNode.GetCurrentUser();
        }

        /// <summary>
        /// Check the correctness of the block chain.
        /// </summary>
        public bool CheckCorrect()
        {
            foreach (var block in fBlocks) {
                if (!block.IsCorrect()) {
                    return false;
                }
            }

            return true;
        }

        public void ReceivedGlobalBlockchain(string jsonResponse)
        {
            List<Block> blocks;
            if (string.IsNullOrEmpty(jsonResponse)) {
                blocks = null;
            } else {
                blocks = DeserializeCollectionBlocks(jsonResponse);
            }

            Chain globalChain = null;
            if (blocks != null && blocks.Count > 0) {
                globalChain = new Chain(blocks);
            }

            if (globalChain == null)
                return;

            var localChain = GetLocalChain();

            if (globalChain != null && localChain != null) {
                if (globalChain.Length > localChain.Length) {
                    ReplaceLocalChainFromGlobalChain(globalChain);
                } else {
                    LoadDataFromLocalChain(localChain);
                }
            } else if (globalChain != null) {
                ReplaceLocalChainFromGlobalChain(globalChain);
            } else if (localChain != null) {
                LoadDataFromLocalChain(localChain);
            } else {
                CreateNewBlockChain();
            }

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "Error creating block chain. The chain is incorrect.");
            }
        }

        /// <summary>
        /// Getting a chain of blocks from local storage.
        /// </summary>
        private Chain GetLocalChain()
        {
            var blocks = fDataProvider.GetBlocks();
            if (blocks.Count > 0) {
                var chain = new Chain(blocks);
                return chain;
            }

            return null;
        }

        /// <summary>
        /// Add block to local chain and database.
        /// </summary>
        public void AddBlock(Block block)
        {
            if (block == null) {
                throw new ArgumentNullException(nameof(block));
            }

            if (!block.IsCorrect()) {
                throw new MethodArgumentException(nameof(block), "The block is invalid.");
            }

            // Do not add an existing block
            if (fBlocks.Any(b => b.Hash == block.Hash)) {
                return;
            }

            fBlocks.Add(block);
            fDataProvider.AddBlock(block);

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "The correctness was violated after adding the block.");
            }
        }

        public void ProcessBlockTransactions(Block block)
        {
            if (block == null) {
                throw new ArgumentNullException(nameof(block));
            }

            if (!block.IsCorrect()) {
                throw new MethodArgumentException(nameof(block), "The block is invalid.");
            }

            var transactions = block.Transactions;
            foreach (var trx in transactions) {
                string typeUnit, typeOperator;
                trx.GetTypeParams(out typeUnit, out typeOperator);

                ITransactionSolver solver = fNode.GetSolver(typeUnit);
                if (solver != null) {
                    solver.Solve(fNode, trx);
                }
            }
        }

        /// <summary>
        /// Formation of a list of blocks based on the received json response from the host.
        /// </summary>
        private static List<Block> DeserializeCollectionBlocks(string json)
        {
            var requestResult = JsonHelper.DeserializeObject<SerializableChain>(json);

            var result = new List<Block>();
            foreach (var block in requestResult.Chain) {
                result.Add(new Block(block));
            }
            return result;
        }
    }
}
