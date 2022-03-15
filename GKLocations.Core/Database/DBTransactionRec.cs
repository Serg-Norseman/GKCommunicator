/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using GKLocations.Blockchain;
using SQLite;

namespace GKLocations.Core.Database
{
    /// <summary>
    /// DTO for Transaction records of database.
    /// </summary>
    [Table("LocalTransactions")]
    public class DBTransactionRec
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Indexed]
        public long Timestamp { get; set; }

        [NotNull, Indexed]
        public string Type { get; set; }

        /// <summary>
        /// Block hash.
        /// </summary>
        [NotNull, Indexed]
        public string Hash { get; set; }

        [NotNull]
        public string Data { get; set; }


        public DBTransactionRec()
        {
        }

        public DBTransactionRec(ITransaction transaction)
        {
            Timestamp = transaction.Timestamp;
            Type = transaction.Type;
            Hash = transaction.Hash;
            Data = transaction.Serialize();
        }

        public Transaction GetData()
        {
            return Transaction.Deserialize(Data);
        }
    }
}
