/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GKLocations.Utils;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Blockchain.
    /// </summary>
    public class Chain
    {
        public const int CurrentVersion = 1;

        private IDataProvider fDataProvider;
        private IBlockchainNode fNode;
        private List<Block> fBlockChain = new List<Block>();
        private List<Transaction> fData = new List<Transaction>();
        public IList<ITransaction> fPendingTransactions = new List<ITransaction>();


        public IEnumerable<Block> BlockChain
        {
            get {
                return fBlockChain;
            }
        }

        public Block PreviousBlock
        {
            get {
                return fBlockChain.Last();
            }
        }

        public IEnumerable<Transaction> Content
        {
            get {
                return fData;
            }
        }

        public int Length
        {
            get {
                return fBlockChain.Count;
            }
        }


        /// <summary>
        /// Create a new block chain instance.
        /// </summary>
        public Chain(IBlockchainNode network, IDataProvider dataProvider)
        {
            fNode = network;
            fDataProvider = dataProvider;
        }

        /// <summary>
        /// Create a block chain from a data provider block list.
        /// </summary>
        private Chain(List<SerializableBlock> blocks)
        {
            if (blocks == null) {
                throw new ArgumentNullException(nameof(blocks));
            }

            foreach (var block in blocks) {
                var b = new Block(block);
                fBlockChain.Add(b);
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
                fBlockChain.Add(block);
            }

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "Error creating block chain. The chain is incorrect.");
            }
        }

        public void CreateNewBlock()
        {
            if (fPendingTransactions.Count <= 0) {
                return;
            }
        }

        /// <summary>
        /// Get data from the local chain.
        /// </summary>
        private void LoadDataFromLocalChain(Chain localChain)
        {
            if (localChain == null) {
                throw new ArgumentNullException(nameof(localChain));
            }

            foreach (var block in localChain.fBlockChain) {
                fBlockChain.Add(block);
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

            foreach (var block in globalChain.fBlockChain) {
                AddBlock(block);
            }
        }

        /// <summary>
        /// Get the current user of the system.
        /// </summary>
        public User GetCurrentUser()
        {
            User result = fNode.GetCurrentUser();

            // FIXME
            if (result == null) {
                result = new User("user", "password", UserRole.Reader);
            }

            return result;
        }

        /// <summary>
        /// Create a new empty block chain.
        /// </summary>
        private void CreateNewBlockChain()
        {
            fDataProvider.ClearBlocks();
            fBlockChain = new List<Block>();

            var genesisBlock = Block.CreateGenesisBlock(GetCurrentUser());
            AddBlock(genesisBlock);
        }

        /// <summary>
        /// Check the correctness of the block chain.
        /// </summary>
        public bool CheckCorrect()
        {
            foreach (var block in fBlockChain) {
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
            if (fBlockChain.Any(b => b.Hash == block.Hash)) {
                return;
            }

            fBlockChain.Add(block);
            fDataProvider.AddBlock(new SerializableBlock(block));

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "The correctness was violated after adding the block.");
            }
        }

        public void AddPendingTransaction(ITransaction transaction)
        {
            fPendingTransactions.Add(new Transaction(transaction));
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
                string typeUnit = trx.GetTypeUnit();
                string typeOperator = trx.GetTypeOperator();

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

        /// <summary>
        /// Request to the host to add a block of data.
        /// </summary>
        private bool SendBlockToHosts(string ip, string method, string data)
        {
            return fNode.SendBlockToHost(ip, method, data);
        }
    }
}
