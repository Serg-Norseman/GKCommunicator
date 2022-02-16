/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKLocations.Common
{
    public interface ILocationName
    {
        string GUID { get; set; }

        string LocationGUID { get; set; }

        /// <summary>
        /// The longest geographical name in the world (the name of Bangkok) - consists of 176 latin letters
        /// (or 132 native letters + 7 spaces).
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Settlement's type (village, town, city[capital]).
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date range format ("BET 10 JUL 1805 AND 20 AUG 1917").
        /// </summary>
        string ActualDates { get; set; }

        /// <summary>
        /// en_US, ru_RU and etc
        /// </summary>
        string Language { get; set; }
    }
}
