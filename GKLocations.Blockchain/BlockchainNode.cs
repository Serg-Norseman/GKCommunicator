/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace GKLocations.Blockchain
{
    public class BlockchainNode : IBlockchainNode
    {
        private readonly Chain fChain;
        private readonly IDataProvider fDataProvider;
        private readonly IList<string> fPeers;
        private readonly Dictionary<string, ITransactionSolver> fSolvers;
        private readonly List<User> fUsers;


        public Chain Chain
        {
            get {
                return fChain;
            }
        }

        public IList<string> Peers
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
            fChain = new Chain(this, fDataProvider);
            fDataProvider = dataProvider;
            fPeers = new List<string>();
            fUsers = new List<User>();
            fSolvers = new Dictionary<string, ITransactionSolver>();

            RegisterSolver(new ProfileTransactionSolver());
        }

        public User GetCurrentUser()
        {
            throw new System.NotImplementedException();
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
    }
}
