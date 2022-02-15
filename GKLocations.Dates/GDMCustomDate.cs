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
using System.Globalization;
using System.Text;
using BSLib;
using BSLib.Calendar;

namespace GKLocations.Dates
{
    public enum GDMApproximated
    {
        daExact,
        daAbout,
        daCalculated,
        daEstimated
    }


    public enum DateFormat
    {
        dfDD_MM_YYYY,
        dfYYYY_MM_DD,
        dfYYYY
    }


    public struct MatchParams
    {
        public float NamesIndistinctThreshold;

        public bool DatesCheck;
        public int YearsInaccuracy;
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


    public class GEDCOMIntDateException : GDMDateException
    {
        public GEDCOMIntDateException(string intDate)
            : base(string.Format("The interpreted date '{0}' doesn't start with a valid ident", intDate))
        {
        }
    }


    public class GEDCOMRangeDateException : GDMDateException
    {
        public GEDCOMRangeDateException(string rangeDate)
            : base(string.Format("The range date '{0}' doesn't contain 'and' token", rangeDate))
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


    public abstract class GDMCustomDate : IComparable, IComparable<GDMCustomDate>, IEquatable<GDMCustomDate>
    {
        public static readonly string[] GEDCOMDateTypes = 
            new string[] { "", "ABT", "AFT", "BEF", "BET", "CAL", "EST", "FROM", "INT", "TO" };

        public const char Delimiter = ' ';
        public const char YearModifierSeparator = '/';
        public const string YearBC = "B.C.";

        internal static readonly string[] GEDCOMDateApproximatedArray =
            new string[] { "", "ABT", "CAL", "EST" };

        internal static readonly string[] GEDCOMDateRangeArray =
            new string[] { "AFT", "BEF", "BET", "AND" };

        public static readonly string[] GEDCOMMonthArray =
            new string[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

        public const string FROM = "FROM";
        public const string INT = "INT";
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

        public virtual float IsMatch(GDMCustomDate tag, MatchParams matchParams)
        {
            return 0.0f;
        }

        protected virtual void DateChanged()
        {
        }

        /// <summary>
        /// Obtaining UDN (Unified Date Number) for purposes of processing and sorting.
        /// </summary>
        /// <returns></returns>
        public abstract UDN GetUDN();

        /// <summary>
        /// In the historical chronology of the year 0 does not exist.
        /// Therefore, the digit 0 in the year value can be used as a sign of lack or error.
        /// ChronologicalYear - introduced for the purposes of uniform chronology years in the Gregorian calendar.
        /// Is estimated from -4714 BC to 3268 AD.
        /// </summary>
        /// <returns>chronological year</returns>
        public virtual int GetChronologicalYear()
        {
            int resultYear;

            UDN udn = GetUDN();
            if (udn.HasKnownYear()) {
                int month, day;
                CalendarConverter.jd_to_gregorian2(udn.GetUnmaskedValue(), out resultYear, out month, out day);
            } else {
                resultYear = 0;
            }

            return resultYear;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as GDMCustomDate);
        }

        public int CompareTo(GDMCustomDate other)
        {
            if (other != null) {
                UDN abs1 = GetUDN();
                UDN abs2 = other.GetUDN();
                return abs1.CompareTo(abs2);
            }

            return -1;
        }

        public override int GetHashCode()
        {
            var udn = GetUDN();
            return udn.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            GDMCustomDate otherDate = obj as GDMCustomDate;

            if (otherDate != null) {
                UDN abs1 = GetUDN();
                UDN abs2 = otherDate.GetUDN();
                return abs1.Equals(abs2);
            }

            return false;
        }

        public bool Equals(GDMCustomDate other)
        {
            UDN abs1 = GetUDN();
            UDN abs2 = other.GetUDN();
            return abs1.Equals(abs2);
        }

        public static GDMDate CreateApproximated(GDMDate date, GDMApproximated approximated)
        {
            GDMDate result = new GDMDate();
            result.Assign(date);
            result.Approximated = approximated;
            return result;
        }

        public static GDMDatePeriod CreatePeriod(GDMDate dateFrom, GDMDate dateTo)
        {
            GDMDatePeriod result = new GDMDatePeriod();
            if (dateFrom != null) result.DateFrom.Assign(dateFrom);
            if (dateTo != null) result.DateTo.Assign(dateTo);
            return result;
        }

        public static GDMDateRange CreateRange(GDMDate dateAfter, GDMDate dateBefore)
        {
            GDMDateRange result = new GDMDateRange();
            if (dateAfter != null) result.After.Assign(dateAfter);
            if (dateBefore != null) result.Before.Assign(dateBefore);
            return result;
        }


        #region Special parsing routines

        protected static readonly TextInfo InvariantTextInfo = CultureInfo.InvariantCulture.TextInfo;

        private enum GEDCOMDateType { SIMP, ABT, AFT, BEF, BET, CAL, EST, FROM, INT, TO }

        // DateValue format: INT/FROM/TO/etc..._<date>
        protected static string ParseDateValue(GDMDateValue dateValue, string str)
        {
            if (str == null) {
                return string.Empty;
            }

            var strTok = new GEDCOMParser(str, false);
            strTok.SkipWhitespaces();

            int idx = 0;
            var token = strTok.CurrentToken;
            if (token == GEDCOMToken.Word) {
                string su = strTok.GetWord();
                idx = Algorithms.BinarySearch(GDMCustomDate.GEDCOMDateTypes, su, string.CompareOrdinal);
            }
            var dateType = (idx < 0) ? GEDCOMDateType.SIMP : (GEDCOMDateType)idx;

            string result;
            GDMCustomDate date;
            switch (dateType) {
                case GEDCOMDateType.AFT:
                case GEDCOMDateType.BEF:
                case GEDCOMDateType.BET:
                    date = new GDMDateRange();
                    result = ParseRangeDate((GDMDateRange)date, strTok);
                    break;
                case GEDCOMDateType.INT:
                    date = new GDMDateInterpreted();
                    result = ParseIntDate((GDMDateInterpreted)date, strTok);
                    break;
                case GEDCOMDateType.FROM:
                case GEDCOMDateType.TO:
                    date = new GDMDatePeriod();
                    result = ParsePeriodDate((GDMDatePeriod)date, strTok);
                    break;
                default:
                    date = new GDMDate();
                    result = ParseDate((GDMDate)date, strTok);
                    break;
            }

            dateValue.SetRawData(date);
            return result;
        }

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

        // Format: AFT DATE | BEF DATE | BET AFT_DATE AND BEF_DATE
        protected static string ParseRangeDate(GDMDateRange date, string strValue)
        {
            var strTok = new GEDCOMParser(strValue, false);
            // only standard GEDCOM dates (for owner == null)
            return ParseRangeDate(date, strTok);
        }

        // Format: AFT DATE | BEF DATE | BET AFT_DATE AND BEF_DATE
        protected static string ParseRangeDate(GDMDateRange date, GEDCOMParser strTok)
        {
            strTok.SkipWhitespaces();

            var token = strTok.CurrentToken;
            if (token != GEDCOMToken.Word) {
                // error!
            }
            string su = strTok.GetWord();
            int dateType = Algorithms.BinarySearch(GDMCustomDate.GEDCOMDateRangeArray, su, string.CompareOrdinal);

            if (dateType == 0) { // "AFT"
                strTok.Next();
                ParseDate(date.After, strTok);
            } else if (dateType == 1) { // "BEF"
                strTok.Next();
                ParseDate(date.Before, strTok);
            } else if (dateType == 2) { // "BET"
                strTok.Next();
                ParseDate(date.After, strTok);
                strTok.SkipWhitespaces();

                if (!strTok.RequireWord(GDMCustomDate.GEDCOMDateRangeArray[3])) { // "AND"
                    throw new GEDCOMRangeDateException(strTok.GetFullStr());
                }

                strTok.Next();
                strTok.SkipWhitespaces();
                ParseDate(date.Before, strTok);
            }

            return strTok.GetRest();
        }

        // Format: INT DATE (phrase)
        protected static string ParseIntDate(GDMDateInterpreted date, string strValue)
        {
            var strTok = new GEDCOMParser(strValue, false);
            // only standard GEDCOM dates (for owner == null)
            return ParseIntDate(date, strTok);
        }

        // Format: INT DATE (phrase)
        protected static string ParseIntDate(GDMDateInterpreted date, GEDCOMParser strTok)
        {
            strTok.SkipWhitespaces();

            if (!strTok.RequireWord(GDMCustomDate.INT)) {
                throw new GEDCOMIntDateException(strTok.GetFullStr());
            }
            strTok.Next();
            ParseDate(date, strTok);

            strTok.SkipWhitespaces();
            var token = strTok.CurrentToken;
            if (token == GEDCOMToken.Symbol && strTok.GetSymbol() == '(') {
                var phrase = new StringBuilder();
                phrase.Append(strTok.GetWord());
                do {
                    token = strTok.Next();
                    phrase.Append(strTok.GetWord());
                } while (token != GEDCOMToken.Symbol || strTok.GetSymbol() != ')');

                date.DatePhrase = phrase.ToString();
            } else {
                date.DatePhrase = string.Empty;
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
            GDMApproximated approximated;
            short year;
            bool yearBC;
            string yearModifier;
            byte month;
            byte day;

            string result = ParseDate(strTok, out approximated, out year, out yearBC, 
                                      out yearModifier, out month, out day);

            date.SetRawData(approximated, year, yearBC, yearModifier, month, day);

            return result;
        }

        // Format: [ <YEAR>[B.C.] | <MONTH> <YEAR> | <DAY> <MONTH> <YEAR> ] (see p.45-46)
        protected static string ParseDate(GEDCOMParser strTok, out GDMApproximated approximated,
                                       out short year, out bool yearBC,
                                       out string yearModifier, out byte month, out byte day)
        {
            approximated = GDMApproximated.daExact;
            year = GDMDate.UNKNOWN_YEAR;
            yearBC = false;
            yearModifier = string.Empty;
            month = 0;
            day = 0;

            strTok.SkipWhitespaces();

            var token = strTok.CurrentToken;

            // extract approximated
            token = strTok.CurrentToken;
            if (token == GEDCOMToken.Word) {
                string su = InvariantTextInfo.ToUpper(strTok.GetWord());
                int idx = Algorithms.BinarySearch(GDMCustomDate.GEDCOMDateApproximatedArray, su, string.CompareOrdinal);
                if (idx >= 0) {
                    approximated = (GDMApproximated)idx;
                    strTok.Next();
                    strTok.SkipWhitespaces();
                }
            }

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
    }
}
