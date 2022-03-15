﻿/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using SQLite;

namespace GKNetLocationsPlugin.Database
{
    /// <summary>
    /// DTO for Location records of database.
    /// </summary>
    [Table("Locations")]
    public class DBLocationRec : ILocation
    {
        //[PrimaryKey, AutoIncrement]
        //public int Id { get; set; }

        [PrimaryKey, MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        // Optional, only for settlements
        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public DBLocationRec()
        {
        }

        public DBLocationRec(ILocation source)
        {
            GUID = source.GUID;
            Latitude = source.Latitude;
            Longitude = source.Longitude;
        }
    }
}