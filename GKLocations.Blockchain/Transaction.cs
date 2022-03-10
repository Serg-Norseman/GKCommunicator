/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using GKLocations.Utils;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Transaction stored in a block.
    /// </summary>
    public class Transaction : ITransaction, IHashable
    {
        public long Timestamp { get; private set; }

        public string Type { get; private set; }

        public string Content { get; private set; }

        /// <summary>
        /// Data hash.
        /// </summary>
        public string Hash { get; private set; }


        /// <summary>
        /// Create a data instance.
        /// </summary>
        public Transaction(long timestamp, string type, string content)
        {
            if (string.IsNullOrEmpty(content)) {
                throw new ArgumentNullException(nameof(content));
            }

            Timestamp = timestamp;
            Type = type;
            Content = content;

            Hash = this.GetHash();

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Transaction), "Data creation error. The data is incorrect.");
            }
        }

        /// <summary>
        /// Deserializing a object from JSON.
        /// </summary>
        public static Transaction Deserialize(string json)
        {
            var data = JsonHelper.DeserializeObject<Transaction>(json);

            if (!data.IsCorrect()) {
                throw new MethodResultException(nameof(data), "Incorrect data after deserialization.");
            }

            return data as Transaction ??
                throw new FormatException("Failed to deserialize data.");
        }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public string GetHashableContent()
        {
            var text = Type;
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

        public string GetJson()
        {
            return JsonHelper.SerializeObject(this);
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
