using System;
using GKLocations.Common;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Chain block.
    /// </summary>
    public class Block : IHashable
    {
        /// <summary>
        /// Hashing algorithm.
        /// </summary>
        private IAlgorithm fAlgorithm = Helpers.GetDefaultAlgorithm();

        /// <summary>
        /// The version of the block specification.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Block creation time.
        /// </summary>
        public DateTime CreatedOn { get; private set; }

        /// <summary>
        /// Block hash.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// The hash of the previous block.
        /// </summary>
        public string PreviousHash { get; private set; }

        /// <summary>
        /// Block data.
        /// </summary>
        public Data Data { get; private set; }

        /// <summary>
        /// ID of the user who created the block.
        /// </summary>
        public User User { get; private set; }


        /// <summary>
        /// Create a block instance.
        /// </summary>
        public Block(Block previousBlock, Data data, User user, IAlgorithm algorithm = null)
        {
            if (previousBlock == null) {
                throw new ArgumentNullException(nameof(previousBlock));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }

            if (algorithm != null) {
                fAlgorithm = algorithm;
            }

            if (!previousBlock.IsCorrect()) {
                throw new MethodArgumentException(nameof(previousBlock), "The previous block is invalid.");
            }

            if (!data.IsCorrect()) {
                throw new MethodArgumentException(nameof(data), "Data is incorrect.");
            }

            if (!user.IsCorrect()) {
                throw new MethodArgumentException(nameof(user), "User is incorrect.");
            }

            Version = 1; // FIXME
            CreatedOn = DateTime.UtcNow;
            PreviousHash = previousBlock.Hash;
            Data = data;
            User = user;
            Hash = this.GetHash(fAlgorithm);

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Block creation error. The block is invalid.");
            }
        }

        /// <summary>
        /// Create a new instance of the block.
        /// </summary>
        protected Block(string previousHash, Data data, User user, IAlgorithm algorithm = null)
        {
            if (algorithm != null) {
                fAlgorithm = algorithm;
            }

            Version = 1; // FIXME
            CreatedOn = DateTime.UtcNow;
            User = user;
            PreviousHash = previousHash;
            Data = data;
            Hash = this.GetHash(fAlgorithm);

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Genesis block creation error. The block is invalid.");
            }
        }

        /// <summary>
        /// Creating a chain block from a data provider block.
        /// </summary>
        public Block(SerializableBlock block)
        {
            if (block == null) {
                throw new ArgumentNullException(nameof(block));
            }

            Version = block.Version;
            CreatedOn = block.CreatedOn.ToUniversalTime();
            User = User.Deserialize(block.User);
            PreviousHash = block.PreviousHash;
            Data = Data.Deserialize(block.Data);
            Hash = block.Hash;

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Block), "Block creation from database error. The block is invalid.");
            }
        }

        /// <summary>
        /// Get the starting (genesis) block of the block chain.
        /// </summary>
        public static Block CreateGenesisBlock(User user, IAlgorithm algorithm = null)
        {
            if (algorithm == null) {
                algorithm = Helpers.GetDefaultAlgorithm();
            }

            var previousHash = algorithm.GetHash("5DBB70E1-34B3-4E74-87ED-EC9A4C5A5D41");
            var data = user.GetData();
            var genesisBlock = new Block(previousHash, data, user, algorithm);
            return genesisBlock;
        }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public string GetHashableContent()
        {
            var data = "";
            data += Version;
            data += CreatedOn.Ticks;
            data += PreviousHash;
            data += Data.Hash;
            data += User.Hash;

            return data;
        }

        /// <summary>
        /// Casting an object to a string.
        /// </summary>
        public override string ToString()
        {
            return Hash;
        }

        public string GetJson()
        {
            return JsonHelper.SerializeObject(this);
        }
    }
}
