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
using System.Security.Cryptography;
using System.Text;

namespace GKNet.Blockchain
{
    /// <summary>
    /// Auxiliary methods.
    /// </summary>
    public static class Helpers
    {
        private static SHA256 fSHA256 = SHA256.Create();

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
        public static bool IsCorrect(this IList<Transaction> transactions)
        {
            foreach (var trx in transactions) {
                if (trx.Hash != trx.GetHash())
                    return false;
            }
            return true;
        }

        public static string GetHash(this IList<Transaction> transactions)
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
