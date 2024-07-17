/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2024 by Sergey V. Zhdanovskih.
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
using BSLib;

namespace GKNetLocationsPlugin.Dates
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

        private byte fDay;
        private byte fMonth;
        private short fYear;
        private bool fYearBC;
        private UDN fUDN;


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


        public GDMDate()
        {
            fYear = UNKNOWN_YEAR;
            fYearBC = false;
            fMonth = 0;
            fDay = 0;
        }

        public override void Clear()
        {
            base.Clear();

            fYear = UNKNOWN_YEAR;
            fYearBC = false;
            fMonth = 0;
            fDay = 0;

            DateChanged();
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

            fYear = srcDate.fYear;
            fYearBC = srcDate.fYearBC;
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
        internal void SetRawData(short year, bool yearBC, string yearModifier, byte month, byte day)
        {
            fYear = year;
            fYearBC = yearBC;
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
            var parts = new string[5];
            int pIdx = 0;

            if (fDay > 0) {
                parts[pIdx++] = fDay.ToString("D2");
            }

            if (fMonth > 0) {
                string[] months = GetMonthNames();
                parts[pIdx++] = months[fMonth - 1];
            }

            if (fYear != UNKNOWN_YEAR) {
                string yearStr = fYear.ToString("D3");

                if (fYearBC) {
                    yearStr += GDMCustomDate.YearBC;
                }

                parts[pIdx++] = yearStr;
            }

            return string.Join(" ", parts, 0, pIdx);
        }

        private static byte GetMonthNumber(string strMonth)
        {
            string su = InvariantTextInfo.ToUpper(strMonth);

            int month = ArrayHelper.IndexOf(GDMCustomDate.GEDCOMMonthArray, su);
            return (byte)(month + 1);
        }

        public void SetDate(int day, int month, int year, bool yearBC = false)
        {
            SetGregorian(day, month, year);
        }

        private void SetDateInternal(int day, int month, int year, string yearModifier, bool yearBC)
        {
            fDay = (byte)day;
            fMonth = (byte)month;
            fYear = (short)year;
            fYearBC = yearBC;

            DateChanged();
        }

        public void SetGregorian(int day, int month, int year)
        {
            SetDateInternal(day, month, year, "", false);
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
            return fUDN;
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

        public string GetDisplayString(DateFormat format, bool includeBC = false, bool showCalendar = false)
        {
            var parts = new string[5];
            int pIdx = 0;

            int year = fYear;
            int month = fMonth;
            int day = fDay;
            bool ybc = fYearBC;

            if (year > 0 || month > 0 || day > 0) {
                switch (format) {
                    case DateFormat.dfDD_MM_YYYY:
                        parts[pIdx++] = day > 0 ? day.ToString("D2", null) + "." : "__.";
                        parts[pIdx++] = month > 0 ? month.ToString("D2", null) + "." : "__.";
                        parts[pIdx++] = year > 0 ? year.ToString().PadLeft(4, '_') : "____";
                        if (includeBC && ybc) {
                            parts[pIdx++] = " BC";
                        }
                        break;

                    case DateFormat.dfYYYY_MM_DD:
                        if (includeBC && ybc) {
                            parts[pIdx++] = "BC ";
                        }
                        parts[pIdx++] = year > 0 ? year.ToString().PadLeft(4, '_') + "." : "____.";
                        parts[pIdx++] = month > 0 ? month.ToString("D2", null) + "." : "__.";
                        parts[pIdx++] = day > 0 ? day.ToString("D2", null) : "__";
                        break;

                    case DateFormat.dfYYYY:
                        if (year > 0) {
                            if (includeBC && ybc) {
                                parts[pIdx++] = "BC ";
                            }
                            parts[pIdx++] = year.ToString().PadLeft(4, '_');
                        }
                        break;
                }
            }

            return string.Concat(parts);
        }

        public override string GetDisplayStringExt(DateFormat format, bool sign, bool showCalendar)
        {
            string result = GetDisplayString(format, true, showCalendar);
            return result;
        }

        #endregion

        public override void GetDateRange(out GDMDate dateStart, out GDMDate dateEnd)
        {
            dateStart = this;
            dateEnd = this;
        }
    }
}
