/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using GKNetLocationsPlugin.Dates;

namespace GKNetLocationsPlugin.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationRelation : Entity, ILocationRelation
    {
        public override string GUID { get; set; }

        public string LocationGUID { get; set; }

        public string OwnerGUID { get; set; }

        /// <summary>
        /// [P]olitical, [R]eligious, [G]eographic, [C]ultural
        /// </summary>
        public string RelationType { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date range format ("BET 10 JUL 1805 AND 20 AUG 1917").
        /// </summary>
        public string ActualDates { get; set; }


        public LocationRelation()
        {
        }

        public GDMDateValue GetActualDates()
        {
            var result = new GDMDateValue();
            result.ParseString(ActualDates);
            return result;
        }

        public void SetActualDates(GDMDateValue value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            ActualDates = value.StringValue;
        }
    }
}
