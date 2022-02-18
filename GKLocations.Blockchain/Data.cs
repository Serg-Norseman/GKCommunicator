using System;
using GKLocations.Common;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// The type of data stored in the block.
    /// </summary>
    public enum DataType : byte
    {
        Content = 0,
        User = 1,
        Node = 2
    }


    /// <summary>
    /// Data stored in a block.
    /// </summary>
    public class Data : IHashable
    {
        /// <summary>
        /// Hashing algorithm.
        /// </summary>
        private IAlgorithm fAlgorithm = Helpers.GetDefaultAlgorithm();

        /// <summary>
        /// Block content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// The type of data stored.
        /// </summary>
        public DataType Type { get; private set; }

        /// <summary>
        /// Data hash.
        /// </summary>
        public string Hash { get; private set; }


        /// <summary>
        /// Create a data instance.
        /// </summary>
        public Data(string content, DataType type, IAlgorithm algorithm = null)
        {
            if (string.IsNullOrEmpty(content)) {
                throw new ArgumentNullException(nameof(content));
            }

            if (algorithm != null) {
                fAlgorithm = algorithm;
            }

            Content = content;
            Type = type;

            Hash = this.GetHash(fAlgorithm);

            if (!this.IsCorrect()) {
                throw new MethodResultException(nameof(Data), "Data creation error. The data is incorrect.");
            }
        }

        /// <summary>
        /// Deserializing a object from JSON.
        /// </summary>
        public static Data Deserialize(string json)
        {
            var data = JsonHelper.DeserializeObject<Data>(json);

            if (!data.IsCorrect()) {
                throw new MethodResultException(nameof(data), "Incorrect data after deserialization.");
            }

            return data as Data ??
                throw new FormatException("Failed to deserialize data.");
        }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public string GetHashableContent()
        {
            var text = Content;
            text += (int)Type;
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
    }
}
