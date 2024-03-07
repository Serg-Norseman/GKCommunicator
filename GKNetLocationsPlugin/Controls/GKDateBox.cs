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
using System.Windows.Forms;

namespace GKNetLocationsPlugin.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GKDateBox : MaskedTextBox
    {
        private static readonly string fRegionalDatePattern;


        public string RegionalDatePattern
        {
            get { return fRegionalDatePattern; }
        }

        public string NormalizeDate
        {
            get { return GetNormalizeDate(Text, fRegionalDatePattern); }
            set { Text = GetRegionalDate(value, fRegionalDatePattern); }
        }


        static GKDateBox()
        {
            fRegionalDatePattern = GetShortDatePattern();
            //Logger.WriteInfo(string.Format("RegionalDatePattern: {0}", fRegionalDatePattern));
        }

        public GKDateBox()
        {
            Culture = CultureInfo.InvariantCulture;
            TextMaskFormat = MaskFormat.IncludePromptAndLiterals;

            Mask = GetDateMask(fRegionalDatePattern);
        }


        public static string GetDateMask(string regionalDatePattern)
        {
            // "00/00/0000"
            string result = regionalDatePattern.Replace('d', '0').Replace('m', '0').Replace('y', '0');
            return result;
        }

        public static string GetShortDatePattern()
        {
            var culture = CultureInfo.CurrentCulture; // work
            //var culture = new CultureInfo("en-US"); // debug
            //var culture = new CultureInfo("hu-HU"); // debug

            var dtf = culture.DateTimeFormat;
            var dateSeparators = dtf.DateSeparator.ToCharArray();

            // may contain a period, a dash, and a slash
            var result = dtf.ShortDatePattern.ToLowerInvariant();
            //Logger.WriteInfo(string.Format("ShortDatePattern: {0}", result));

            // normalize
            string[] parts = result.Split(dateSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++) {
                string part = parts[i];
                char firstChar = part[0];
                switch (firstChar) {
                    case 'd':
                    case 'm':
                        if (part.Length < 2) {
                            part = part.PadRight(2, firstChar);
                        }
                        break;

                    case 'y':
                        if (part.Length < 4) {
                            part = part.PadRight(4, firstChar);
                        }
                        break;
                }
                parts[i] = part;
            }
            result = string.Join("/", parts);

            return result;
        }

        /// <summary>
        /// The result of the function is a "normalized date", delimited by '.' and fixed order of parts: "dd.mm.yyyy".
        /// The pattern and regional date contain the delimiter '/'.
        /// The pattern defines the position of the parts in a regional date format.
        /// </summary>
        /// <param name="regionalDate">date similar "01/20/1970"</param>
        /// <param name="pattern">pattern similar "mm/dd/yyyy"</param>
        /// <returns>normalized date as "dd.mm.yyyy"</returns>
        public static string GetNormalizeDate(string regionalDate, string pattern)
        {
            try {
                string[] resultParts = new string[3];

                if (!string.IsNullOrEmpty(regionalDate)) {
                    string[] regionalParts = regionalDate.Split('/');
                    string[] patternParts = pattern.Split('/');

                    for (int i = 0; i < patternParts.Length; i++) {
                        string part = patternParts[i];
                        switch (part[0]) {
                            case 'd':
                                resultParts[0] = regionalParts[i];
                                break;

                            case 'm':
                                resultParts[1] = regionalParts[i];
                                break;

                            case 'y':
                                resultParts[2] = regionalParts[i];
                                break;
                        }
                    }
                }

                string result = string.Join(".", resultParts);
                return result;
            } catch (Exception ex) {
                //Logger.WriteError(string.Format("GKUtils.GetNormalizeDate({0}, {1})", regionalDate, pattern), ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// The result of the function is a "regional date", delimited by '/' and regional order of parts: "mm/dd/yyyy"
        /// or any other. The pattern and regional date contain the delimiter '/'.
        /// The pattern defines the position of the parts in a regional date format.
        /// </summary>
        /// <param name="normalizeDate">date with format "dd.mm.yyyy"</param>
        /// <param name="pattern">pattern similar "mm/dd/yyyy"</param>
        /// <returns>regional date as "mm/dd/yyyy"</returns>
        public static string GetRegionalDate(string normalizeDate, string pattern)
        {
            if (string.IsNullOrEmpty(normalizeDate))
                return string.Empty;

            try {
                string[] normalizeParts = normalizeDate.Split('.');
                string[] patternParts = pattern.Split('/');
                string[] resultParts = new string[3];

                for (int i = 0; i < patternParts.Length; i++) {
                    string part = patternParts[i];
                    switch (part[0]) {
                        case 'd':
                            resultParts[i] = normalizeParts[0];
                            break;

                        case 'm':
                            resultParts[i] = normalizeParts[1];
                            break;

                        case 'y':
                            resultParts[i] = normalizeParts[2];
                            break;
                    }
                }

                string result = string.Join("/", resultParts);
                return result;
            } catch (Exception ex) {
                //Logger.WriteError(string.Format("GKUtils.GetRegionalDate({0}, {1})", normalizeDate, pattern), ex);
                return string.Empty;
            }
        }
    }
}
