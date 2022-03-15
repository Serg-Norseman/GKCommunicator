/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2022 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
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
using System.Collections.Generic;
using BSLib;
using BSLib.Calendar;

namespace GKLocations.Core.Dates
{
    /// <summary>
    /// Class to hold simple standard GEDCOM dates.
    /// Note: Year cannot be used externally with negative values even for "BC",
    /// because these dates there is a special property.
    /// Dates of type "BC" should have a positive Year + the property YearBC.
    /// </summary>
    public class GDMDate : GDMCustomDate
    {
        public const int UNKNOWN_YEAR = -1;

        private GDMApproximated fApproximated;
        private byte fDay;
        private byte fMonth;
        private short fYear;
        private bool fYearBC;
        private string fYearModifier;
        private UDN fUDN;


        public GDMApproximated Approximated
        {
            get { return fApproximated; }
            set { fApproximated = value; }
        }

        public byte Day
        {
            get { return fDay; }
            set {
                fDay = value;
                DateChanged();
            }
        }

        public byte Month
        {
            get { return fMonth; }
            set {
                fMonth = value;
                DateChanged();
            }
        }

        public short Year
        {
            get { return fYear; }
            set {
                fYear = value;
                DateChanged();
            }
        }

        public bool YearBC
        {
            get { return fYearBC; }
            set {
                fYearBC = value;
                DateChanged();
            }
        }

        public string YearModifier
        {
            get { return fYearModifier; }
            set { fYearModifier = value; }
        }


        public GDMDate()
        {
            fApproximated = GDMApproximated.daExact;
            fYear = UNKNOWN_YEAR;
            fYearBC = false;
            fYearModifier = string.Empty;
            fMonth = 0;
            fDay = 0;
        }

        public override void Clear()
        {
            base.Clear();

            fApproximated = GDMApproximated.daExact;
            fYear = UNKNOWN_YEAR;
            fYearBC = false;
            fYearModifier = string.Empty;
            fMonth = 0;
            fDay = 0;

            DateChanged();
        }

        /// <summary>
        /// This function is intended only for checking the completeness of parts of the date 
        /// (year, month and day are defined, are not unknown).
        /// </summary>
        public bool IsValidDate()
        {
            return (fYear > 0 && fMonth > 0 && fDay > 0);
        }

        public override bool IsEmpty()
        {
            return base.IsEmpty() && fYear <= 0 && fMonth <= 0 && fDay <= 0;
        }

        public override void Assign(GDMCustomDate source)
        {
            GDMDate srcDate = source as GDMDate;
            if (srcDate == null)
                throw new ArgumentException(@"Argument is null or wrong type", "source");

            fApproximated = srcDate.fApproximated;
            fYear = srcDate.fYear;
            fYearBC = srcDate.fYearBC;
            fYearModifier = srcDate.fYearModifier;
            fMonth = srcDate.fMonth;
            fDay = srcDate.fDay;

            DateChanged();
        }

        public override string ParseString(string strValue)
        {
            string result;
            if (string.IsNullOrEmpty(strValue)) {
                Clear();
                result = string.Empty;
            } else {
                result = ParseDate(this, strValue);
            }
            return result;
        }

        /// <summary>
        /// Internal helper method for parser
        /// </summary>
        internal void SetRawData(GDMApproximated approximated,  
                                 short year, bool yearBC, string yearModifier, byte month, byte day)
        {
            fApproximated = approximated;
            fYear = year;
            fYearBC = yearBC;
            fYearModifier = yearModifier;
            fMonth = month;
            fDay = day;

            DateChanged();
        }

        #region Private methods of parsing of the input format

        public static string[] GetMonthNames()
        {
            string[] monthes = GDMCustomDate.GEDCOMMonthArray;
            return monthes;
        }

        private static string CheckGEDCOMMonth(string str)
        {
            // An empty string is a valid identifier for an unknown month
            if (string.IsNullOrEmpty(str)) return string.Empty;

            string[] monthes = GDMDate.GetMonthNames();
            str = str.ToUpperInvariant();
            for (int m = 0; m < monthes.Length; m++) {
                if (monthes[m] == str) {
                    return str;
                }
            }

            throw new GDMDateException("The string {0} is not a valid month identifier", str);
        }

        private static string IntToGEDCOMMonth(int m)
        {
            return (m == 0) ? string.Empty : GDMCustomDate.GEDCOMMonthArray[m - 1];
        }

        #endregion

        protected override string GetStringValue()
        {
            var parts = new List<string>(5);
            if (fApproximated != GDMApproximated.daExact) {
                parts.Add(GDMCustomDate.GEDCOMDateApproximatedArray[(int)fApproximated]);
            }

            if (fDay > 0) {
                parts.Add(fDay.ToString("D2"));
            }

            if (fMonth > 0) {
                string[] months = GetMonthNames();
                parts.Add(months[fMonth - 1]);
            }

            if (fYear != UNKNOWN_YEAR) {
                string yearStr = fYear.ToString("D3");
                if (!string.IsNullOrEmpty(fYearModifier)) {
                    yearStr = yearStr + "/" + fYearModifier;
                }

                if (fYearBC) {
                    yearStr += GDMCustomDate.YearBC;
                }

                parts.Add(yearStr);
            }

            return string.Join(" ", parts);
        }

        private static byte GetMonthNumber(string strMonth)
        {
            string su = InvariantTextInfo.ToUpper(strMonth);

            int month = Algorithms.IndexOf(GDMCustomDate.GEDCOMMonthArray, su);
            return (byte)(month + 1);
        }

        public void SetDate(int day, int month, int year, bool yearBC = false)
        {
            SetGregorian(day, month, year);
        }

        private void SetDateInternal(int day, string month, int year, string yearModifier, bool yearBC)
        {
            SetDateInternal(day, GetMonthNumber(month), year, yearModifier, yearBC);
        }

        private void SetDateInternal(int day, int month, int year, string yearModifier, bool yearBC)
        {
            fDay = (byte)day;
            fMonth = (byte)month;
            fYear = (short)year;
            fYearModifier = yearModifier;
            fYearBC = yearBC;

            DateChanged();
        }

        public void SetGregorian(int day, int month, int year)
        {
            SetDateInternal(day, month, year, "", false);
        }

        public void SetGregorian(int day, string month, int year, string yearModifier, bool yearBC)
        {
            SetDateInternal(day, CheckGEDCOMMonth(month), year, yearModifier, yearBC);
        }

        #region UDN processing

        protected override void DateChanged()
        {
            int year = fYear;
            if (year == UNKNOWN_YEAR) {
                year = UDN.UnknownYear;
            } else {
                if (fYearBC) year = -year;
            }

            UDNCalendarType udnCalendar = UDNCalendarType.ctGregorian;
            fUDN = new UDN(udnCalendar, year, fMonth, fDay);
        }

        public override UDN GetUDN()
        {
            return (fApproximated == GDMApproximated.daExact) ? fUDN : UDN.CreateApproximate(fUDN);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// This function transforms the string into a date. All components of
        /// the date's string must be given by numbers in order of day / month / year.
        /// This function is intended only for use with the date entry controls (fixed format of date's string).
        /// </summary>
        public static GDMDate CreateByFormattedStr(string dateStr, bool aException)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;

            if (dateStr.IndexOf("-") >= 0) dateStr = dateStr.Replace("-", ".");
            if (dateStr.IndexOf("/") >= 0) dateStr = dateStr.Replace("/", ".");
            if (dateStr.IndexOf("_") >= 0) dateStr = dateStr.Replace("_", " ");

            string[] dtParts = dateStr.Split('.');
            if (dtParts.Length < 3) {
                if (aException) {
                    throw new GDMDateException("Invalid date format '{0}'", dateStr);
                }

                return null;
            }

            string pd = dtParts[0].Trim();
            string pm = dtParts[1].Trim();
            string py = dtParts[2].Trim();

            int day = (pd == "") ? 0 : ConvertHelper.ParseInt(pd, 0);
            int month = (pm == "") ? 0 : ConvertHelper.ParseInt(pm, 0);
            int year = (py == "") ? UNKNOWN_YEAR : ConvertHelper.ParseInt(py, UNKNOWN_YEAR);

            var date = new GDMDate();
            date.SetDate(day, month, year);
            return date;
        }

        public static UDN GetUDNByFormattedStr(string dateStr, bool aException = false)
        {
            GDMDate dtx = GDMDate.CreateByFormattedStr(dateStr, aException);
            return (dtx != null) ? dtx.GetUDN() : UDN.CreateEmpty();
        }

        public string GetDisplayString(DateFormat format, bool includeBC = false, bool showCalendar = false)
        {
            string result = "";

            int year = fYear;
            int month = fMonth;
            int day = fDay;
            bool ybc = fYearBC;

            if (year > 0 || month > 0 || day > 0) {
                switch (format) {
                    case DateFormat.dfDD_MM_YYYY:
                        result += day > 0 ? ConvertHelper.AdjustNumber(day, 2) + "." : "__.";
                        result += month > 0 ? ConvertHelper.AdjustNumber(month, 2) + "." : "__.";
                        result += year > 0 ? year.ToString().PadLeft(4, '_') : "____";
                        break;

                    case DateFormat.dfYYYY_MM_DD:
                        result += year > 0 ? year.ToString().PadLeft(4, '_') + "." : "____.";
                        result += month > 0 ? ConvertHelper.AdjustNumber(month, 2) + "." : "__.";
                        result += day > 0 ? ConvertHelper.AdjustNumber(day, 2) : "__";
                        break;

                    case DateFormat.dfYYYY:
                        if (year > 0) {
                            result = year.ToString().PadLeft(4, '_');
                        }
                        break;
                }
            }

            if (includeBC && ybc) {
                switch (format) {
                    case DateFormat.dfDD_MM_YYYY:
                        result = result + " BC";
                        break;
                    case DateFormat.dfYYYY_MM_DD:
                        result = "BC " + result;
                        break;
                    case DateFormat.dfYYYY:
                        result = "BC " + result;
                        break;
                }
            }

            /*if (showCalendar) {
                result = result + GKData.DateCalendars[(int)fCalendar].Sign;
            }*/

            return result;
        }

        public override string GetDisplayStringExt(DateFormat format, bool sign, bool showCalendar)
        {
            string result = GetDisplayString(format, true, showCalendar);
            if (sign && fApproximated != GDMApproximated.daExact) {
                result = "~ " + result;
            }

            return result;
        }

        #endregion
    }
}
