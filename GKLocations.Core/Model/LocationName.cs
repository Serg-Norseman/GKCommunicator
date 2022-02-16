/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using GKLocations.Common;

namespace GKLocations.Core.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationName : ILocationName
    {
        public string GUID { get; set; }

        public string LocationGUID { get; set; }

        /// <summary>
        /// The longest geographical name in the world (the name of Bangkok) - consists of 176 latin letters
        /// (or 132 native letters + 7 spaces).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Settlement's type (village, town, city[capital]).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date range format ("BET 10 JUL 1805 AND 20 AUG 1917").
        /// </summary>
        public string ActualDates { get; set; }

        /// <summary>
        /// en_US, ru_RU and etc
        /// </summary>
        public string Language { get; set; }


        public LocationName()
        {
        }
    }
}
