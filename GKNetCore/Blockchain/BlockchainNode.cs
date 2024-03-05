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
using System.Threading;
using System.Threading.Tasks;
using BSLib;

namespace GKNet.Blockchain
{
    /// <summary>
    /// 
    /// </summary>
    public partial class BlockchainNode : IBlockchainNode
    {
        public const int CurrentVersion = 1;

        private readonly ICommunicatorCore fCommunicatorCore;
        private readonly IDataProvider fDataProvider;
        private readonly Dictionary<string, ITransactionSolver> fSolvers;
        private readonly Timer fTimer;


        public ICommunicatorCore CommunicatorCore
        {
            get {
                return fCommunicatorCore;
            }
        }

        public IList<IBlockchainPeer> Peers
        {
            get {
                return (IList<IBlockchainPeer>)fCommunicatorCore.Peers;
            }
        }


        public BlockchainNode(ICommunicatorCore communicatorCore, IDataProvider dataProvider)
        {
            fCommunicatorCore = communicatorCore;
            fDataProvider = dataProvider;

            fSolvers = new Dictionary<string, ITransactionSolver>();

            RegisterSolver(new ProfileTransactionSolver());

            var lastBlock = fDataProvider.GetLastBlock();
            if (lastBlock == null) {
                CreateGenesisBlock();
            }

            fTimer = new Timer(TimerCallback);
            fTimer.Change(10 * 60 * 1000, Timeout.Infinite);

            InitConsensus();
        }

        private void TimerCallback(object e)
        {
            CreateNewBlock();
        }

        public void CreateNewBlock()
        {
            var lastBlock = fDataProvider.GetLastBlock();
            if (lastBlock == null) {
                CreateGenesisBlock();
                return;
            }

            var pendingTransactions = fDataProvider.GetPendingTransactions();
            if (pendingTransactions.Count <= 0) {
                return;
            }

            // TODO: implement a selection of transactions that pass verification and do not exceed the maximum block size

            var newBlock = new Block(lastBlock, pendingTransactions);
            AddBlock(newBlock);

            fDataProvider.ClearPendingTransactions();

            BroadcastBlock(newBlock);
        }

        public void CreateGenesisBlock()
        {
            var genesisBlock = Block.CreateGenesisBlock(GetCurrentUser());
            AddBlock(genesisBlock);
        }

        /// <summary>
        /// Check the correctness of the block chain.
        /// </summary>
        public bool CheckCorrect()
        {
            // get full local chain
            var blocks = fDataProvider.GetBlocks();

            foreach (var block in blocks) {
                if (!block.IsCorrect()) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Add block to local chain and database.
        /// </summary>
        public void AddBlock(Block block)
        {
            if (block == null) {
                throw new ArgumentNullException("block");
            }

            if (!block.IsCorrect()) {
                throw new MethodArgumentException("block", "The block is invalid.");
            }

            // Do not add an existing block
            if (fDataProvider.FindBlock(block.Hash) != null) {
                return;
            }

            fDataProvider.AddBlock(block);

            if (!CheckCorrect()) {
                throw new MethodResultException("BlockchainNode", "The correctness was violated after adding the block.");
            }
        }

        public void ProcessBlockTransactions(Block block)
        {
            if (block == null) {
                throw new ArgumentNullException("block");
            }

            if (!block.IsCorrect()) {
                throw new MethodArgumentException("block", "The block is invalid.");
            }

            var transactions = block.Transactions;
            foreach (var trx in transactions) {
                // The solver is responsible for processing the data from the transaction for specific purposes
                string typeUnit = trx.GetTypeUnit();
                ITransactionSolver solver = GetSolver(typeUnit);
                if (solver != null) {
                    solver.Solve(this, trx);
                }

                // TODO: remove the transaction from the pool of pending transactions, if it exists
            }
        }

        public bool VerifyTransactions(Transaction transaction)
        {
            if (transaction == null) {
                throw new ArgumentNullException("transaction");
            }

            if (!transaction.IsCorrect()) {
                throw new MethodArgumentException("transaction", "The transaction is invalid.");
            }

            try {
                string typeUnit = transaction.GetTypeUnit();
                ITransactionSolver solver = GetSolver(typeUnit);
                return (solver != null) && solver.Verify(transaction);
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Get the current user of the system.
        /// </summary>
        public UserProfile GetCurrentUser()
        {
            return fCommunicatorCore.Profile;
        }

        public void RegisterSolver(ITransactionSolver solver)
        {
            if (solver == null)
                throw new ArgumentNullException("solver");

            string sign = solver.Sign;
            fSolvers[sign] = solver;
        }

        public ITransactionSolver GetSolver(string sign)
        {
            ITransactionSolver result;
            return (fSolvers.TryGetValue(sign, out result)) ? result : null;
        }

        public void AddPendingTransaction(string type, object data)
        {
            string json = JsonHelper.SerializeObject(data);
            var transaction = new Transaction(TimeHelper.DateTimeToUnixTime(DateTime.UtcNow), type, json);
            AddPendingTransaction(transaction);
        }

        public void AddPendingTransaction(Transaction transaction)
        {
            fDataProvider.AddPendingTransaction(transaction);
        }

        public void AddPeerProfile(PeerProfile profile)
        {
            AddPendingTransaction(ProfileTransactionSolver.ProfileTransactionType, profile);
        }

        public void RequestGlobalBlockchain()
        {
            var lastBlock = fDataProvider.GetLastBlock();
            if (lastBlock == null) {
                return;
            }

            foreach (var peer in Peers) {
                SendChainStateRequest(peer, lastBlock.Index, lastBlock.Hash);
            }
        }

        /// <summary>
        /// Send connected peers a request for characteristics (index, hash) of the last blocks in their chains.
        /// </summary>
        private void SendChainStateRequest(IBlockchainPeer peer, long selfLastBlockIndex, string selfLastBlockHash)
        {
            // dummy
        }

        /// <summary>
        /// Received from the connected peer a response with the characteristics (index, hash) of the last block in the chain.
        /// </summary>
        public void ReceiveChainStateRequest(IBlockchainPeer peer, long peerLastBlockIndex, string peerLastBlockHash)
        {
            var lastBlock = fDataProvider.GetLastBlock();
            if (lastBlock != null && peerLastBlockIndex < lastBlock.Index) {
                if (peerLastBlockIndex > 0) {
                    // The chain has the specified common block?
                    var peerLastBlock = fDataProvider.FindBlock(peerLastBlockHash);
                    if (peerLastBlock != null && peerLastBlock.Index == peerLastBlockIndex) {
                        var blocks = fDataProvider.GetBlocks(peerLastBlockIndex + 1);
                        SendBlocks(peer, blocks);
                    }
                } else {
                    // A peer only has a genesis block
                    var blocks = fDataProvider.GetBlocks(0);
                    SendBlocks(peer, blocks);
                }
            }
        }

        public bool SendBlocks(IBlockchainPeer peer, IList<Block> blocks)
        {
            return false;
        }

        /// <summary>
        /// Replenishment of the blockchain with a list of blocks received from a peer.
        /// </summary>
        public void ReceiveBlocksResponse(IBlockchainPeer peer, string json)
        {
            var newBlocks = Helpers.DeserializeBlocks(json);

            if (!newBlocks.IsCorrect()) {
                throw new MethodResultException("newBlocks", "Error receive block chain. The chain is incorrect.");
            }

            var lastBlock = fDataProvider.GetLastBlock();
            if (lastBlock != null && lastBlock.Index == 0) {
                // A node only has a genesis block
                // Therefore, the response will come with the full chain, including the genesis block
                fDataProvider.ClearBlocks();
            }

            foreach (var blk in newBlocks) {
                fDataProvider.AddBlock(blk);
                ProcessBlockTransactions(blk);
            }
        }

        public void SendTransaction(IBlockchainPeer peer, ITransaction transaction)
        {

        }

        /// <summary>
        /// The function sends a pending transaction to all other known nodes.
        /// </summary>
        public void BroadcastTransaction(ITransaction transaction)
        {
            Parallel.ForEach(Peers, peer => {
                SendTransaction(peer, transaction);
            });
        }

        /// <summary>
        /// The function accepts a pending transaction from another node.
        /// </summary>
        public void ReceiveTransaction(IBlockchainPeer sender, Transaction transaction)
        {
            try {
                // TODO: check that the transaction is not in the pool (if received earlier from another node and has not yet been processed)
                // or blockchain (if an old, long-processed transaction has been received)

                // TODO: transaction validation should take into account the scenario when the receive order is violated and the transaction
                // tries to modify data not yet added by a transaction that is not in the pool (leave pending)

                if (VerifyTransactions(transaction)) {
                    AddPendingTransaction(transaction);

                    // TODO: what should be the mechanism of protection against flooding here?!
                    //BroadcastTransaction(transaction);
                }
            } catch {
                // if !trx.IsCorrect(), then do nothing
            }
        }

        public void SendBlock(IBlockchainPeer peer, IBlock block)
        {

        }

        public void BroadcastBlock(IBlock block)
        {
            Parallel.ForEach(Peers, peer => {
                SendBlock(peer, block);
            });
        }

        public void ReceiveBlock(IBlockchainPeer sender, IBlock block)
        {
            try {
                // TODO: full block check

                // TODO: implement remembering the last block of the main chain for fork verification
                // TODO: implement a pool of orphan blocks in case of receiving a block for which its parent block was not previously received

                //fChain.AddBlock();

                // TODO: what should be the mechanism of protection against flooding here?!
                //BroadcastBlock(block);
            } catch {
                // if !block.IsCorrect(), then do nothing
            }
        }
    }
}
