/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using GKLocations.Blockchain;
using SQLite;

namespace GKLocations.Core.Database
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
        public long Timestamp { get; set; }

        [NotNull]
        public string Type { get; set; }

        [NotNull]
        public string Content { get; set; }


        public DBTransactionRec()
        {
        }

        public DBTransactionRec(long timestamp, string type, string content)
        {
            Timestamp = timestamp;
            Type = type;
            Content = content;
        }
    }
}
