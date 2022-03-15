/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Collections.Generic;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Utility class for deserializing block chain from Json.
    /// </summary>
    public class SerializableChain
    {
        public List<IBlock> Chain { get; set; }
    }
}
