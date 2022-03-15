/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKNetLocationsPlugin.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Location : Entity, ILocation
    {
        public override string GUID { get; set; }

        // Optional, only for settlements
        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public Location()
        {
        }
    }
}
