/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
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
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
//using System.Windows.Forms;

namespace GKNet
{
    public static class SysHelper
    {
        public static string GetUserName()
        {
            return Environment.UserName;
        }

        public static string GetUserCountry()
        {
            return System.Globalization.RegionInfo.CurrentRegion.ThreeLetterISORegionName;
            //return RegionInfo.CurrentRegion.DisplayName;
        }

        public static string GetTimeZone()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            var result = localZone.StandardName;
            var s = result.Split(' ');
            var offset = localZone.GetUtcOffset(DateTime.Now);
            var offsetStr = (offset.TotalMilliseconds < 0) ? offset.ToString() : "+" + offset.ToString();
            return string.Format("{0} (UTC{1})", result, offsetStr);//(s[0]);
            //return result;
        }

        public static string GetLanguages()
        {
            string result = "";
            /*foreach (InputLanguage c in InputLanguage.InstalledInputLanguages) {
                result += (result.Length != 0) ? ", " : "";
                result += (c.Culture.ThreeLetterISOLanguageName);
            }*/
            return result;
        }

        public static string GetPublicIPAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return null;
            }

            try {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();
                return externalIP;
            } catch { return null; }
        }
    }
}
