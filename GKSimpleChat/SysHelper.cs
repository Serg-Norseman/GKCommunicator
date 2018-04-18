using System;
using System.Windows.Forms;

namespace GKNet.Core
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
            foreach (InputLanguage c in InputLanguage.InstalledInputLanguages) {
                result += (result.Length != 0) ? ", " : "";
                result += (c.Culture.ThreeLetterISOLanguageName);
            }
            return result;
        }
    }
}
