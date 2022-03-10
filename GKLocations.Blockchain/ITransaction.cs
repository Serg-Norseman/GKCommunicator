/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKLocations.Blockchain
{
    public interface ITransaction
    {
        /// <summary>
        /// Transaction creation time.
        /// </summary>
        long Timestamp { get; }

        /// <summary>
        /// The type of data stored.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Data content.
        /// </summary>
        string Content { get; }
    }
}
