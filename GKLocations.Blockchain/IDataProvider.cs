using System.Collections.Generic;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// The base interface that the data provider must implement.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Adding a data block.
        /// </summary>
        void AddBlock(SerializableBlock block);

        /// <summary>
        /// Get all blocks.
        /// </summary>
        List<SerializableBlock> GetBlocks();

        /// <summary>
        /// Clear storage. Removing all blocks.
        /// </summary>
        void ClearBlocks();
    }
}
