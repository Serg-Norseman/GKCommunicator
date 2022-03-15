/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Collections.Generic;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// The base interface that the data provider must implement.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Adding a data block.
        /// </summary>
        void AddBlock(IBlock block);

        /// <summary>
        /// Get all blocks.
        /// </summary>
        IList<IBlock> GetBlocks();

        /// <summary>
        /// Clear storage. Removing all blocks.
        /// </summary>
        void ClearBlocks();

        /// <summary>
        /// Adding a transaction.
        /// </summary>
        void AddTransaction(ITransaction transaction);

        /// <summary>
        /// Get all transactions.
        /// </summary>
        IList<ITransaction> GetLocalTransactions();

        /// <summary>
        /// Clear storage. Removing all transactions.
        /// </summary>
        void ClearLocalTransactions();
    }
}
