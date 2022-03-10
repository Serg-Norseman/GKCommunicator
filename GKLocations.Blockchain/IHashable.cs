﻿/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

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
