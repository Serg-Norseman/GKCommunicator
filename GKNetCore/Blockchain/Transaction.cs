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

namespace GKNet.Blockchain
{
    /// <summary>
    /// Transaction stored in a block.
    /// </summary>
    public class Transaction : Hashable, ITransaction
    {
        public long Timestamp { get; private set; }

        public string Type { get; private set; }

        public string Content { get; private set; }


        /// <summary>
        /// Create a data instance.
        /// </summary>
        public Transaction(long timestamp, string type, string content)
        {
            if (string.IsNullOrEmpty(content)) {
                throw new ArgumentNullException("content");
            }

            Timestamp = timestamp;
            Type = type;
            Content = content;
            Hash = GetHash();

            if (!IsCorrect()) {
                throw new MethodResultException("Transaction", "Data creation error. The data is incorrect.");
            }
        }

        public Transaction(ITransaction transaction)
        {
            if (transaction == null) {
                throw new ArgumentNullException("transaction");
            }

            Timestamp = transaction.Timestamp;
            Type = transaction.Type;
            Content = transaction.Content;
            Hash = GetHash();

            if (!IsCorrect()) {
                throw new MethodResultException("Transaction", "Data creation error. The data is incorrect.");
            }
        }

        /// <summary>
        /// Deserializing a object from JSON.
        /// </summary>
        public static Transaction Deserialize(string json)
        {
            var data = JsonHelper.DeserializeObject<Transaction>(json);
            if (data == null) {
                throw new FormatException("Failed to deserialize data.");
            }

            if (!data.IsCorrect()) {
                throw new MethodResultException("data", "Incorrect data after deserialization.");
            }

            return data;
        }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public override string GetHashableContent()
        {
            var text = "";
            text += Timestamp;
            text += Type;
            text += Content;
            return text;
        }

        /// <summary>
        /// Casting an object to a string.
        /// </summary>
        public override string ToString()
        {
            return Content;
        }

        public string Serialize()
        {
            return JsonHelper.SerializeObject(this);
        }

        public T DeserializeContent<T>()
        {
            return JsonHelper.DeserializeObject<T>(Content);
        }

        public string GetTypeUnit()
        {
            string[] parts = Type.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return (parts.Length > 0) ? parts[0] : string.Empty;
        }

        public string GetTypeOperator()
        {
            string[] parts = Type.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return (parts.Length > 1) ? parts[1] : string.Empty;
        }
    }
}
