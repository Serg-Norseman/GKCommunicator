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
    [Table("LocationRelations")]
    public class LocationRelation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int LocationId { get; set; }

        [NotNull]
        public int OwnerId { get; set; }

        /// <summary>
        /// [P]olitical, [R]eligious, [G]eographic, [C]ultural
        /// </summary>
        [MaxLength(1), NotNull]
        public string RelationType { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date range format ("BET 10 JUL 1805 AND 20 AUG 1917").
        /// </summary>
        public string ActualDates { get; set; }


        public LocationRelation()
        {
        }
    }
}
