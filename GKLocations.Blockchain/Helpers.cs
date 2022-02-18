namespace GKLocations.Blockchain
{
    /// <summary>
    /// Auxiliary methods.
    /// </summary>
    public static class Helpers
    {
        public static string GetHash(this IHashable component, IAlgorithm algorithm = null)
        {
            if (algorithm == null) {
                algorithm = GetDefaultAlgorithm();
            }

            return algorithm.GetHash(component);
        }

        public static string GetHash(this string text, IAlgorithm algorithm = null)
        {
            if (algorithm == null) {
                algorithm = GetDefaultAlgorithm();
            }

            return algorithm.GetHash(text);
        }

        public static IAlgorithm GetDefaultAlgorithm()
        {
            return new SHA256();
        }

        /// <summary>
        /// Checking the correctness of the hashed object.
        /// </summary>
        public static bool IsCorrect(this IHashable component, IAlgorithm algorithm = null)
        {
            if (algorithm == null) {
                algorithm = GetDefaultAlgorithm();
            }

            return (component.Hash == component.GetHash(algorithm));
        }
    }
}
