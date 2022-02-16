/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using GKLocations.Common;
using SQLite;

namespace GKLocations.Database
{
    /// <summary>
    /// DTO for Transaction records of database.
    /// </summary>
    [Table("LocalTransactions")]
    public class DBTransactionRec : ITransaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public DateTime Timestamp { get; set; }

        [NotNull]
        public string Data { get; set; }


        public DBTransactionRec()
        {
        }
    }
}
