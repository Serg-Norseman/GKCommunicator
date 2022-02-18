namespace GKLocations.Blockchain
{
    /// <summary>
    /// Interface for objects that can be hashed. 
    /// </summary>
    public interface IHashable
    {
        /// <summary>
        /// The stored hash of the component.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        string GetHashableContent();
    }
}
