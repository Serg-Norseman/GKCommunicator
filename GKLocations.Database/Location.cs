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
    [Table("Locations")]
    public class Location
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        // Optional, only for settlements
        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public Location()
        {
        }
    }
}
