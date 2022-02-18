using System;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Block serializable to Json.
    /// </summary>
    public class SerializableBlock
    {
        /// <summary>
        /// The version of the block specification.
        /// </summary>
        public virtual int Version { get; set; }

        /// <summary>
        /// Block creation time.
        /// </summary>
        public virtual DateTime CreatedOn { get; set; }

        /// <summary>
        /// Block hash.
        /// </summary>
        public virtual string Hash { get; set; }

        /// <summary>
        /// The hash of the previous block.
        /// </summary>
        public virtual string PreviousHash { get; set; }

        /// <summary>
        /// Block data.
        /// </summary>
        public virtual string Data { get; set; }

        /// <summary>
        /// ID of the user who created the block.
        /// </summary>
        public virtual string User { get; set; }


        public SerializableBlock()
        {
        }

        public SerializableBlock(Block block)
        {
            Version = block.Version; // FIXME
            CreatedOn = block.CreatedOn;
            PreviousHash = block.PreviousHash;
            Data = block.Data.GetJson();
            User = block.User.GetJson();
            Hash = block.Hash;
        }
    }
}
