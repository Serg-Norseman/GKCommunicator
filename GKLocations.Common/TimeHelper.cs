/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;

namespace GKLocations.Utils
{
    public static class TimeHelper
    {
        private static readonly DateTime UnixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts the given date value to Unix epoch time.
        /// </summary>
        public static long DateTimeToUnixTime(DateTime dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ticks = date.Ticks - UnixTimeStart.Ticks;
            var ts = ticks / TimeSpan.TicksPerSecond;
            return ts;
        }

        /// <summary>
        /// Converts the given Unix epoch time to a <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> kind.
        /// </summary>
        public static DateTime UnixTimeToDateTime(long intDate)
        {
            var timeInTicks = intDate * TimeSpan.TicksPerSecond;
            return UnixTimeStart.AddTicks(timeInTicks);
        }

        public static long GetUtcNow()
        {
            return DateTimeToUnixTime(DateTime.UtcNow);
        }
    }
}
