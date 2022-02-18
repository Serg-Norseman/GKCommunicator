namespace GKLocations.Blockchain
{
    /// <summary>
    /// The interface to be implemented by the hashing algorithm.
    /// </summary>
    public interface IAlgorithm
    {
        string GetHash(string data);

        string GetHash(IHashable data);
    }
}
