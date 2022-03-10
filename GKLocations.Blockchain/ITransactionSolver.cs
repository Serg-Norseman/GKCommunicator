/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKLocations.Blockchain
{
    public interface ITransactionSolver
    {
        string Sign { get; }

        void Solve(IBlockchainNode node, Transaction transaction);
    }
}
