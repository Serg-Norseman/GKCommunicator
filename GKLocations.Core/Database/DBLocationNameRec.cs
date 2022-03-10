/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using SQLite;

namespace GKLocations.Core.Database
{
    /// <summary>
    /// DTO for LocationName records of database.
    /// </summary>
    [Table("LocationNames")]
    public class DBLocationNameRec : ILocationName
    {
        //[PrimaryKey, AutoIncrement]
        //public int Id { get; set; }

        [PrimaryKey, MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        [MaxLength(38), NotNull]
        public string LocationGUID { get; set; }

        /// <summary>
        /// The longest geographical name in the world (the name of Bangkok) - consists of 176 latin letters
        /// (or 132 native letters + 7 spaces).
        /// </summary>
        [MaxLength(200), NotNull]
        public string Name { get; set; }

        /// <summary>
        /// Settlement's type (village, town, city[capital]).
        /// </summary>
        [MaxLength(100), NotNull]
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
        [MaxLength(5), NotNull]
        public string Language { get; set; }


        public DBLocationNameRec()
        {
        }

        public DBLocationNameRec(ILocationName source)
        {
            GUID = source.GUID;
            LocationGUID = source.LocationGUID;
            Name = source.Name;
            Type = source.Type;
            Description = source.Description;
            ActualDates = source.ActualDates;
            Language = source.Language;
        }
    }
}
