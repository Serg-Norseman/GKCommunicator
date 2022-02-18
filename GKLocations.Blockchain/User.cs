using System;
using GKLocations.Common;

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
        /// <summary>
        /// Hashing algorithm.
        /// </summary>
        private IAlgorithm fAlgorithm = Helpers.GetDefaultAlgorithm();


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
        public User(string login, string password, UserRole role, IAlgorithm algorithm = null)
        {
            if (string.IsNullOrEmpty(login)) {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrEmpty(password)) {
                throw new ArgumentNullException(nameof(password));
            }

            if (algorithm != null) {
                fAlgorithm = algorithm;
            }

            Login = login;
            PasswordHash = password.GetHash(fAlgorithm);
            Role = role;
            Hash = this.GetHash(fAlgorithm);

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(User), "User creation error. The user is invalid.");
            }
        }

        /// <summary>
        /// Create a user instance from a block.
        /// </summary>
        public User(Block block)
        {
            if (block == null) {
                throw new ArgumentNullException(nameof(block));
            }

            if (!block.IsCorrect()) {
                throw new MethodArgumentException(nameof(block), "The block is invalid.");
            }

            if (block.Data == null) {
                throw new MethodArgumentException(nameof(block), "Block data cannot be null.");
            }

            if (block.Data.Type != DataType.User) {
                throw new MethodArgumentException(nameof(block), "Invalid block data type.");
            }

            var user = Deserialize(block.Data.Content);

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
        public Data GetData()
        {
            var jsonString = this.GetJson();
            var data = new Data(jsonString, DataType.User, fAlgorithm);
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
