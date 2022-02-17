/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using GKLocations.Common;
using SQLite;

namespace GKLocations.Database
{
    /// <summary>
    /// DTO for LocationRelation records of database.
    /// </summary>
    [Table("LocationRelations")]
    public class DBLocationRelationRec : ILocationRelation
    {
        //[PrimaryKey, AutoIncrement]
        //public int Id { get; set; }

        [PrimaryKey, MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        [MaxLength(38), NotNull]
        public string LocationGUID { get; set; }

        [MaxLength(38), NotNull]
        public string OwnerGUID { get; set; }

        /// <summary>
        /// [P]olitical, [R]eligious, [G]eographic, [C]ultural
        /// </summary>
        [MaxLength(1), NotNull]
        public string RelationType { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date range format ("BET 10 JUL 1805 AND 20 AUG 1917").
        /// </summary>
        public string ActualDates { get; set; }


        public DBLocationRelationRec()
        {
        }

        public DBLocationRelationRec(ILocationRelation source)
        {
            GUID = source.GUID;
            LocationGUID = source.LocationGUID;
            OwnerGUID = source.OwnerGUID;
            RelationType = source.RelationType;
            ActualDates = source.ActualDates;
        }
    }
}
