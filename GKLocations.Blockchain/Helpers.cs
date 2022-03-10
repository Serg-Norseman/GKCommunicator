/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using GKLocations.Utils;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Auxiliary methods.
    /// </summary>
    public static class Helpers
    {
        private static SHA256 fSHA256 = SHA256.Create();

        public static string GetHash(this IHashable component)
        {
            var dataBeforeHash = component.GetHashableContent();
            var hash = GetHash(dataBeforeHash);
            return hash;
        }

        public static string GetHash(this string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var hashByte = fSHA256.ComputeHash(bytes);
            var hash = BitConverter.ToString(hashByte);

            var formattedHash = hash.Replace("-", "").ToLower();
            return formattedHash;
        }

        /// <summary>
        /// Checking the correctness of the hashed object.
        /// </summary>
        public static bool IsCorrect(this IHashable component)
        {
            return (component.Hash == component.GetHash());
        }

        /// <summary>
        /// Checking the correctness of the hashed object.
        /// </summary>
        public static bool IsCorrect(this List<Transaction> transactions)
        {
            foreach (var trx in transactions) {
                if (trx.Hash != trx.GetHash())
                    return false;
            }
            return true;
        }

        public static string GetHash(this List<Transaction> transactions)
        {
            var data = JsonHelper.SerializeObject(transactions);
            return data.GetHash();
        }

        public static string SerializeTransactions(List<Transaction> transactions)
        {
            return JsonHelper.SerializeObject(transactions);
        }

        public static List<Transaction> DeserializeTransactions(string json)
        {
            return JsonHelper.DeserializeObject<List<Transaction>>(json);
        }
    }
}
