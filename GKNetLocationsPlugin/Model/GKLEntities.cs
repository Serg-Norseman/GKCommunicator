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

using System;
using GKNetLocationsPlugin.Dates;

namespace GKNetLocationsPlugin.Model
{
    public class Entity
    {
        public virtual string GUID { get; set; }

        public Entity()
        {
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class Location : Entity, ILocation
    {
        public override string GUID { get; set; }

        public string Coordinates { get; set; }


        public Location()
        {
        }
    }


    public abstract class ActualEntity : Entity
    {
        public virtual string ActualDates { get; set; }

        protected ActualEntity()
        {
        }

        public GDMDatePeriod GetActualDates()
        {
            var result = new GDMDatePeriod();
            result.ParseString(ActualDates);
            return result;
        }

        public void SetActualDates(GDMDatePeriod value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            ActualDates = value.StringValue;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class LocationName : ActualEntity, ILocationName
    {
        public override string GUID { get; set; }

        public string LocationGUID { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public override string ActualDates { get; set; }

        public string Language { get; set; }


        public LocationName()
        {
        }
    }


    public class LocationNameTranslation : Entity, ILocationNameTranslation
    {
        public override string GUID { get; set; }

        public string NameGUID { get; set; }

        public string Name { get; set; }

        public string Language { get; set; }


        public LocationNameTranslation()
        {
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class LocationRelation : ActualEntity, ILocationRelation
    {
        public override string GUID { get; set; }

        public string LocationGUID { get; set; }

        public string OwnerGUID { get; set; }

        public string RelationType { get; set; }

        public override string ActualDates { get; set; }


        public LocationRelation()
        {
        }
    }
}
