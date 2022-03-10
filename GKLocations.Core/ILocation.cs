/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKLocations.Core
{
    public interface ILocation
    {
        string GUID { get; set; }

        // Optional, only for settlements
        double Latitude { get; set; }
        double Longitude { get; set; }
    }
}
