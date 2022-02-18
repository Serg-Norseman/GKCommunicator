using System.Collections.Generic;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Utility class for deserializing block chain from Json.
    /// </summary>
    public class SerializableChain
    {
        public List<SerializableBlock> Chain { get; set; }
    }
}
