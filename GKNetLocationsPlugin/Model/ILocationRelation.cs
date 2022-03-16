/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKNetLocationsPlugin.Model
{
    public interface ILocationRelation
    {
        string GUID { get; set; }

        string LocationGUID { get; set; }

        string OwnerGUID { get; set; }

        /// <summary>
        /// [P]olitical, [R]eligious, [G]eographic, [C]ultural
        /// </summary>
        string RelationType { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date range format ("BET 10 JUL 1805 AND 20 AUG 1917").
        /// </summary>
        string ActualDates { get; set; }
    }
}
