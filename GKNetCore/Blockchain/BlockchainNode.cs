﻿/*
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
using System.Threading;
using System.Threading.Tasks;
using BSLib;

namespace GKNet.Blockchain
{
    /// <summary>
    /// Priorities: PoS & PoI
    /// https://en.wikipedia.org/wiki/Proof_of_stake
    /// https://ru.wikipedia.org/wiki/%D0%94%D0%BE%D0%BA%D0%B0%D0%B7%D0%B0%D1%82%D0%B5%D0%BB%D1%8C%D1%81%D1%82%D0%B2%D0%BE_%D0%B4%D0%BE%D0%BB%D0%B8_%D0%B2%D0%BB%D0%B0%D0%B4%D0%B5%D0%BD%D0%B8%D1%8F
    /// https://putukusuma.medium.com/creating-simple-cryptocurrency-part-5-peer-to-peer-p2p-with-grpc-f96913ddd7dd
    /// </summary>
    public class BlockchainNode : IBlockchainNode
    {
        private readonly Chain fChain;
        private readonly IDataProvider fDataProvider;
        private readonly IList<IBlockchainPeer> fPeers;
        private readonly Dictionary<string, ITransactionSolver> fSolvers;
        private readonly List<User> fUsers;
        private readonly Timer fTimer;


        public Chain Chain
        {
            get {
                return fChain;
            }
        }

        public IList<IBlockchainPeer> Peers
        {
            get {
                return fPeers;
            }
        }

        public IList<User> Users
        {
            get {
                return fUsers;
            }
        }


        public BlockchainNode(IDataProvider dataProvider)
        {
            fDataProvider = dataProvider;
            fChain = new Chain(this, fDataProvider);
            fPeers = new List<IBlockchainPeer>();
            fUsers = new List<User>();
            fSolvers = new Dictionary<string, ITransactionSolver>();

            RegisterSolver(new ProfileTransactionSolver());

            fChain.CreateNewBlockChain();

            fTimer = new Timer(new TimerCallback(TimerCallback));
            fTimer.Change(1 * 60 * 1000, Timeout.Infinite);
        }

        private void TimerCallback(object e)
        {
            fChain.CreateNewBlock();
        }

        public User GetCurrentUser()
        {
            return null;
        }

        public void RequestGlobalBlockchain()
        {
        }

        public bool SendBlockToHost(string ip, string method, string data)
        {
            return false;
        }

        /// <summary>
        /// Log in as a network user.
        /// </summary>
        public User LoginUser(string login, string password)
        {
            if (string.IsNullOrEmpty(login)) {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrEmpty(password)) {
                throw new ArgumentNullException(nameof(password));
            }

            var user = fUsers.SingleOrDefault(b => b.Login == login);
            if (user == null) {
                return null;
            }

            var passwordHash = password.GetHash();
            if (user.PasswordHash != passwordHash) {
                return null;
            }

            return user;
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
            //fDatabase.AddRecord(transaction);
            fChain.AddPendingTransaction(transaction);
        }

        /// <summary>
        /// Send connected peers a request for characteristics (index, hash) of the last blocks in their chains.
        /// </summary>
        public virtual void SendChainStateRequest(ulong selfLastBlockIndex, string selfLastBlockHash)
        {
            // dummy
        }

        /// <summary>
        /// Received from the connected peer a response with the characteristics (index, hash) of the last block in the chain.
        /// </summary>
        public virtual void ReceiveChainStateResponse(ulong peerLastBlockIndex, string peerLastBlockHash)
        {
            // dummy
        }

        public void SendTransaction(IBlockchainPeer peer, ITransaction transaction)
        {

        }

        public void BroadcastTransaction(ITransaction transaction)
        {
            Parallel.ForEach(fPeers, peer => {
                SendTransaction(peer, transaction);
            });
        }

        public void ReceiveTransaction(IBlockchainPeer sender, Transaction transaction)
        {
            try {
                fChain.AddPendingTransaction(transaction);
            } catch {
                // if !trx.IsCorrect(), then do nothing
            }
        }

        public void SendBlock(IBlockchainPeer peer, IBlock block)
        {

        }

        public void BroadcastBlock(IBlock block)
        {
            Parallel.ForEach(fPeers, peer => {
                SendBlock(peer, block);
            });
        }

        public void ReceiveTransaction(IBlockchainPeer sender, IBlock block)
        {
            try {
                //fChain.AddBlock();
            } catch {
                // if !block.IsCorrect(), then do nothing
            }
        }
    }
}