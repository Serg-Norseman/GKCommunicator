/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using SQLite;

namespace GKLocations.Database
{
    /// <summary>
    /// 
    /// </summary>
    [Table("LocationNames")]
    public class LocationName
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int LocationId { get; set; }

        /// <summary>
        /// The longest geographical name in the world (the name of Bangkok) - consists of 176 latin letters
        /// (or 132 native letters + 7 spaces).
        /// </summary>
        [MaxLength(200), NotNull]
        public string Name { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date range format ("BET 10 JUL 1805 AND 20 AUG 1917").
        /// </summary>
        public string DateRange { get; set; }

        /// <summary>
        /// en_US, ru_RU and etc
        /// </summary>
        [MaxLength(5), NotNull]
        public string Language { get; set; }


        public LocationName()
        {
        }
    }
}
