/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
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

using System.Collections.Generic;
using GKNetLocationsPlugin.Dates;
using SQLite;

namespace GKNetLocationsPlugin.Model
{

    public interface ILocationElement
    {
        GDMDatePeriod ActualDatesEx { get; }
    }


    /// <summary>
    /// DTO for Location records of database.
    /// </summary>
    [Table("Locations")]
    public class DBLocationRec : ILocation
    {
        [PrimaryKey, MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        public string Coordinates { get; set; }


        [Ignore]
        public List<DBLocationNameRec> Names { get; set; }

        [Ignore]
        public List<DBLocationRelationRec> Relations { get; set; }


        public DBLocationRec()
        {
            Names = new List<DBLocationNameRec>();
            Relations = new List<DBLocationRelationRec>();
        }

        public DBLocationRec(ILocation source)
        {
            GUID = source.GUID;
            Coordinates = source.Coordinates;
        }

        public bool ValidateNames()
        {
            GDMCustomDate prevDate = null;
            for (int i = 0; i < Names.Count; i++) {
                var locName = Names[i];

                var interDate = GDMCustomDate.GetIntersection(prevDate, locName.ActualDatesEx);
                if (!interDate.IsEmpty()) {
                    return false;
                }

                prevDate = locName.ActualDatesEx;
            }
            return true;
        }

        public bool ValidateLinks()
        {
            GDMCustomDate prevDate = null;
            for (int i = 0; i < Relations.Count; i++) {
                var locLink = Relations[i];

                var interDate = GDMCustomDate.GetIntersection(prevDate, locLink.ActualDatesEx);
                if (!interDate.IsEmpty()) {
                    return false;
                }

                prevDate = locLink.ActualDatesEx;
            }
            return true;
        }

        public void SortNames()
        {
            Names.Sort(ElementsCompare);
        }

        public void SortTopLevels()
        {
            Relations.Sort(ElementsCompare);
        }

        private static int ElementsCompare(ILocationElement cp1, ILocationElement cp2)
        {
            UDN udn1 = cp1.ActualDatesEx.GetUDN();
            UDN udn2 = cp2.ActualDatesEx.GetUDN();
            return udn1.CompareTo(udn2);
        }
    }


    /// <summary>
    /// DTO for LocationName records of database.
    /// </summary>
    [Table("LocationNames")]
    public class DBLocationNameRec : ILocationName, ILocationElement
    {
        [PrimaryKey, MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        [MaxLength(38), NotNull]
        public string LocationGUID { get; set; }

        [MaxLength(200), NotNull]
        public string Name { get; set; }

        [MaxLength(100), NotNull]
        public string Type { get; set; }

        [MaxLength(100), NotNull]
        public string ActualDates { get; set; }

        [Ignore]
        public GDMDatePeriod ActualDatesEx { get; set; }

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
            ActualDates = source.ActualDates;
            Language = source.Language;
        }
    }


    [Table("LocationNameTranslations")]
    public class DBLocationNameTranslationRec : ILocationNameTranslation
    {
        [PrimaryKey, MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        [MaxLength(38), NotNull]
        public string NameGUID { get; set; }

        [MaxLength(200), NotNull]
        public string Name { get; set; }

        [MaxLength(5), NotNull]
        public string Language { get; set; }


        public DBLocationNameTranslationRec()
        {
        }

        public DBLocationNameTranslationRec(ILocationNameTranslation source)
        {
            GUID = source.GUID;
            NameGUID = source.NameGUID;
            Name = source.Name;
            Language = source.Language;
        }
    }


    /// <summary>
    /// DTO for LocationRelation records of database.
    /// </summary>
    [Table("LocationRelations")]
    public class DBLocationRelationRec : ILocationRelation, ILocationElement
    {
        [PrimaryKey, MaxLength(38), NotNull, Unique]
        public string GUID { get; set; }

        [MaxLength(38), NotNull]
        public string LocationGUID { get; set; }

        [MaxLength(38), NotNull]
        public string OwnerGUID { get; set; }

        [MaxLength(1), NotNull]
        public string RelationType { get; set; }

        [MaxLength(100), NotNull]
        public string ActualDates { get; set; }

        [Ignore]
        public GDMDatePeriod ActualDatesEx { get; set; }


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
