/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2022 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GKCommunicator".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using GKNetLocationsPlugin.Model;
using SQLite;

namespace GKNetLocationsPlugin.Database
{
    /// <summary>
    /// DTO for LocationName records of database.
    /// </summary>
    [Table("LocationNames")]
    public class DBLocationNameRec : ILocationName
    {
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
