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
using System.Globalization;
using BSLib;

namespace GKNetLocationsPlugin.Dates
{
    public enum DateFormat
    {
        dfDD_MM_YYYY,
        dfYYYY_MM_DD,
        dfYYYY
    }


    public class GDMDateException : Exception
    {
        public GDMDateException(string message) : base(message)
        {
        }

        public GDMDateException(string message, params object[] args) : base(string.Format(message, args))
        {
        }
    }


    public class GEDCOMParserException : GDMDateException
    {
        public GEDCOMParserException(string message) : base(message)
        {
        }
    }


    public sealed class EnumTuple : IComparable<EnumTuple>
    {
        public string Key;
        public int Value;

        public EnumTuple(string key, int value)
        {
            Key = key;
            Value = value;
        }

        public int CompareTo(EnumTuple other)
        {
            // GEDCOM enums is ASCII identifiers
            return string.Compare(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }
    }


    public abstract class GDMCustomDate : IComparable<GDMCustomDate>
    {
        public const char Delimiter = ' ';
        public const char YearModifierSeparator = '/';
        public const string YearBC = "B.C.";

        public static readonly string[] GEDCOMMonthArray =
            new string[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

        public const string FROM = "FROM";
        public const string TO = "TO";

        internal static readonly EnumTuple[] GEDCOMMonthValues;

        static GDMCustomDate()
        {
            // post-arranged array
            GEDCOMMonthValues = new EnumTuple[] {
                new EnumTuple("JAN", 1), // J/G
                new EnumTuple("FEB", 2), // J/G
                new EnumTuple("MAR", 3), // J/G
                new EnumTuple("APR", 4), // J/G
                new EnumTuple("MAY", 5), // J/G
                new EnumTuple("JUN", 6), // J/G
                new EnumTuple("JUL", 7), // J/G
                new EnumTuple("AUG", 8), // J/G
                new EnumTuple("SEP", 9), // J/G
                new EnumTuple("OCT", 10), // J/G
                new EnumTuple("NOV", 11), // J/G
                new EnumTuple("DEC", 12), // J/G
            };
            // BinarySearch requires a sorted array
            Array.Sort(GEDCOMMonthValues);
        }


        public string StringValue
        {
            get { return GetStringValue(); }
            set { ParseString(value); }
        }


        protected GDMCustomDate()
        {
        }

        public virtual void Assign(GDMCustomDate source)
        {
        }

        public virtual void Clear()
        {
        }

        public virtual bool IsEmpty()
        {
            return true;
        }

        protected abstract string GetStringValue();
        public abstract string ParseString(string strValue);

        public abstract string GetDisplayStringExt(DateFormat format, bool sign, bool showCalendar);

        protected virtual void DateChanged()
        {
        }

        /// <summary>
        /// Obtaining UDN (Unified Date Number) for purposes of processing and sorting.
        /// </summary>
        /// <returns></returns>
        public abstract UDN GetUDN();

        public int CompareTo(GDMCustomDate other)
        {
            if (other != null) {
                UDN abs1 = GetUDN();
                UDN abs2 = other.GetUDN();
                return abs1.CompareTo(abs2);
            }

            return -1;
        }

        public abstract void GetDateRange(out GDMDate dateStart, out GDMDate dateEnd);

        public static GDMDatePeriod CreatePeriod(GDMDate dateFrom, GDMDate dateTo)
        {
            GDMDatePeriod result = new GDMDatePeriod();
            if (dateFrom != null) result.DateFrom.Assign(dateFrom);
            if (dateTo != null) result.DateTo.Assign(dateTo);
            return result;
        }


        #region Special parsing routines

        protected static readonly TextInfo InvariantTextInfo = CultureInfo.InvariantCulture.TextInfo;

        // Format: FROM DATE1 TO DATE2
        protected static string ParsePeriodDate(GDMDatePeriod date, string strValue)
        {
            var strTok = new GEDCOMParser(strValue, false);
            // only standard GEDCOM dates (for owner == null)
            return ParsePeriodDate(date, strTok);
        }

        // Format: FROM DATE1 TO DATE2
        protected static string ParsePeriodDate(GDMDatePeriod date, GEDCOMParser strTok)
        {
            strTok.SkipWhitespaces();

            if (strTok.RequireWord(GDMCustomDate.FROM)) {
                strTok.Next();
                ParseDate(date.DateFrom, strTok);
                strTok.SkipWhitespaces();
            }

            if (strTok.RequireWord(GDMCustomDate.TO)) {
                strTok.Next();
                ParseDate(date.DateTo, strTok);
                strTok.SkipWhitespaces();
            }

            return strTok.GetRest();
        }

        protected static string ParseDate(GDMDate date, string strValue)
        {
            var strTok = new GEDCOMParser(strValue, false);
            // only standard GEDCOM dates (for owner == null)
            return ParseDate(date, strTok);
        }

        protected static string ParseDate(GDMDate date, GEDCOMParser strTok)
        {
            short year;
            bool yearBC;
            string yearModifier;
            byte month;
            byte day;

            string result = ParseDate(strTok, out year, out yearBC, 
                                      out yearModifier, out month, out day);

            date.SetRawData(year, yearBC, yearModifier, month, day);

            return result;
        }

        // Format: [ <YEAR>[B.C.] | <MONTH> <YEAR> | <DAY> <MONTH> <YEAR> ] (see p.45-46)
        protected static string ParseDate(GEDCOMParser strTok, out short year, out bool yearBC,
                                       out string yearModifier, out byte month, out byte day)
        {
            year = GDMDate.UNKNOWN_YEAR;
            yearBC = false;
            yearModifier = string.Empty;
            month = 0;
            day = 0;

            strTok.SkipWhitespaces();

            var token = strTok.CurrentToken;

            // extract day
            token = strTok.CurrentToken;
            int dNum;
            if (token == GEDCOMToken.Number && strTok.TokenLength() <= 2 && ((dNum = strTok.GetNumber()) <= 31)) {
                day = (byte)dNum;
                token = strTok.Next();
            }

            // extract delimiter
            if (token == GEDCOMToken.Whitespace && strTok.GetSymbol() == ' ') {
                token = strTok.Next();
            }

            // extract month
            if (token == GEDCOMToken.Word) {
                // in this case, according to performance test results, BinarySearch is more efficient
                // than a simple search or even a dictionary search (why?!)
                string su = InvariantTextInfo.ToUpper(strTok.GetWord());
                int idx = BinarySearch(GDMCustomDate.GEDCOMMonthValues, su, string.CompareOrdinal);
                month = (byte)((idx < 0) ? 0 : idx);

                token = strTok.Next();
            }

            // extract delimiter
            if (token == GEDCOMToken.Whitespace && strTok.GetSymbol() == ' ') {
                token = strTok.Next();
            }

            // extract negative years
            if (token == GEDCOMToken.Symbol && strTok.GetSymbol() == '-') {
                yearBC = true;
                token = strTok.Next();
            }

            // extract year
            if (token == GEDCOMToken.Number) {
                year = (short)strTok.GetNumber();
                token = strTok.Next();

                // extract year modifier
                if (token == GEDCOMToken.Symbol && strTok.GetSymbol() == GDMCustomDate.YearModifierSeparator) {
                    token = strTok.Next();
                    if (token != GEDCOMToken.Number) {
                        // error
                    } else {
                        yearModifier = strTok.GetWord();
                    }
                    token = strTok.Next();
                }

                // extract bc/ad
                if (token == GEDCOMToken.Word && strTok.GetWord() == "B") {
                    token = strTok.Next();
                    if (token != GEDCOMToken.Symbol || strTok.GetSymbol() != '.') {
                        // error
                    }
                    token = strTok.Next();
                    if (token != GEDCOMToken.Word || strTok.GetWord() != "C") {
                        // error
                    }
                    token = strTok.Next();
                    if (token != GEDCOMToken.Symbol || strTok.GetSymbol() != '.') {
                        // error
                    }
                    strTok.Next();
                    yearBC = true;
                }
            }

            token = strTok.CurrentToken;

            if (day > 0 && month == 0 && year == GDMDate.UNKNOWN_YEAR) {
                year = day;
                day = 0;
            }

            //date.SetRawData(approximated, calendar, year, yearBC, yearModifier, month, day, dateFormat);
            string result = strTok.GetRest();
            return result;
        }

        protected static int BinarySearch(EnumTuple[] array, string key, Comparison<string> comparer)
        {
            int i = 0;
            int num = array.Length - 1;
            while (i <= num) {
                int num2 = i + (num - i >> 1);

                EnumTuple ekv = array[num2];
                int num3 = comparer(ekv.Key, key);

                if (num3 == 0) {
                    return ekv.Value;
                }
                if (num3 < 0) {
                    i = num2 + 1;
                }
                else {
                    num = num2 - 1;
                }
            }
            return ~i;
        }

        #endregion

        public static GDMDatePeriod GetIntersection(GDMCustomDate range1, GDMCustomDate range2)
        {
            if (range1 == null || range1.IsEmpty() || range2 == null || range2.IsEmpty())
                return GDMDatePeriod.Empty;

            GDMDate r1start, r1end, r2start, r2end;
            range1.GetDateRange(out r1start, out r1end);
            range2.GetDateRange(out r2start, out r2end);

            GDMDate greatestStart = r1start.IsEmpty() ? r2start : (r2start.IsEmpty() ? r1start : (r1start.CompareTo(r2start) > 0) ? r1start : r2start);
            GDMDate smallestEnd = r1end.IsEmpty() ? r2end : (r2end.IsEmpty() ? r1end : (r1end.CompareTo(r2end) < 0) ? r1end : r2end);

            // no intersection
            if (greatestStart.CompareTo(smallestEnd) > 0) {
                return GDMDatePeriod.Empty;
            }

            return CreatePeriod(greatestStart, smallestEnd);
        }
    }
}
