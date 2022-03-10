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
    /// User access permissions.
    /// </summary>
    public enum UserRole : byte
    {
        Reader = 0,
        Writer = 1,
    }


    /// <summary>
    /// Network user.
    /// </summary>
    public class User : IHashable
    {
        public const string ProfileTransactionType = "profile";

        /// <summary>
        /// User login.
        /// </summary>
        public string Login { get; private set; }

        /// <summary>
        /// User permissions.
        /// </summary>
        public UserRole Role { get; private set; }

        /// <summary>
        /// The hash of the user's password.
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// User hash.
        /// </summary>
        public string Hash { get; private set; }


        /// <summary>
        /// Create a new user instance.
        /// </summary>
        public User(string login, string password, UserRole role)
        {
            if (string.IsNullOrEmpty(login)) {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrEmpty(password)) {
                throw new ArgumentNullException(nameof(password));
            }

            Login = login;
            PasswordHash = password.GetHash();
            Role = role;
            Hash = this.GetHash();

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(User), "User creation error. The user is invalid.");
            }
        }

        /// <summary>
        /// Create a user instance from a block.
        /// </summary>
        public User(Transaction transaction)
        {
            if (transaction == null) {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (!transaction.IsCorrect()) {
                throw new MethodArgumentException(nameof(transaction), "The block is invalid.");
            }

            if (transaction.Content == null) {
                throw new MethodArgumentException(nameof(transaction), "Transaction content cannot be null.");
            }

            if (transaction.Type != ProfileTransactionType) {
                throw new MethodArgumentException(nameof(transaction), "Invalid transaction data type.");
            }

            var user = Deserialize(transaction.Content);

            Login = user.Login;
            PasswordHash = user.PasswordHash;
            Role = user.Role;
            Hash = user.Hash;

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(User), "Invalid user.");
            }
        }

        /// <summary>
        /// Casting an object to a string.
        /// </summary>
        public override string ToString()
        {
            return Login;
        }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public string GetHashableContent()
        {
            var text = Login;
            text += (int)Role;

            return text;
        }

        /// <summary>
        /// Get a block to add a user to the network.
        /// </summary>
        public Transaction GetData()
        {
            var jsonString = this.GetJson();
            var data = new Transaction(TimeHelper.GetUtcNow(), ProfileTransactionType, jsonString);
            return data;
        }

        /// <summary>
        /// Deserializing a object from JSON.
        /// </summary>
        public static User Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) {
                throw new ArgumentNullException(nameof(json));
            }

            var user = JsonHelper.DeserializeObject<User>(json);

            if (!user.IsCorrect()) {
                throw new MethodResultException(nameof(user), "Invalid user after deserialization.");
            }

            return user as User ??
                throw new FormatException("Failed to deserialize user.");
        }

        public string GetJson()
        {
            return JsonHelper.SerializeObject(this);
        }
    }
}
