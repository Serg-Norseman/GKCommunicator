/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKLocations.Blockchain
{
    public class ProfileTransactionSolver : ITransactionSolver
    {
        public string Sign
        {
            get {
                return "profile";
            }
        }

        public void Solve(IBlockchainNode node, Transaction transaction)
        {
            var user = new User(transaction);
            node.Users.Add(user);
        }

        public bool Verify(Transaction transaction)
        {
            return true;
        }
    }
}
