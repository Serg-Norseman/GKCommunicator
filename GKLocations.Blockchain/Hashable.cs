/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Interface for objects that can be hashed. 
    /// </summary>
    public abstract class Hashable
    {
        /// <summary>
        /// The stored hash of the component.
        /// </summary>
        public virtual string Hash { get; set; }

        /// <summary>
        /// Get data from the object, based on which the hash will be built.
        /// </summary>
        public abstract string GetHashableContent();

        public string GetHash()
        {
            var dataBeforeHash = GetHashableContent();
            var hash = Helpers.GetHash(dataBeforeHash);
            return hash;
        }

        /// <summary>
        /// Checking the correctness of the hashed object.
        /// </summary>
        public bool IsCorrect()
        {
            return (Hash == GetHash());
        }
    }
}
