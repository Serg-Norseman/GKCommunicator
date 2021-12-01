/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
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
using BencodeNET;
using GKNet.DHT;

namespace GKNet
{
    public class UserProfile : PeerProfile
    {
        private string fPasswordHash;
        private string fPrivateKey;


        public bool IsCountryVisible { get; set; }
        public bool IsLanguagesVisible { get; set; }
        public bool IsTimeZoneVisible { get; set; }
        public bool IsEmailVisible { get; set; }

        public string PasswordHash
        {
            get { return fPasswordHash; }
            set { fPasswordHash = value; }
        }

        public string PrivateKey
        {
            get { return fPrivateKey; }
            set { fPrivateKey = value; }
        }

        public bool IsIdentified
        {
            get {
                return !string.IsNullOrEmpty(fPasswordHash) && !string.IsNullOrEmpty(fPrivateKey) && !string.IsNullOrEmpty(fPublicKey);
            }
        }


        public void Reset()
        {
            NodeId = DHTHelper.GetRandomID();

            UserName = Environment.UserName;

            Country = System.Globalization.RegionInfo.CurrentRegion.ThreeLetterISORegionName;
            //return RegionInfo.CurrentRegion.DisplayName;

            TimeZone localZone = System.TimeZone.CurrentTimeZone;
            var result = localZone.StandardName;
            var s = result.Split(' ');
            var offset = localZone.GetUtcOffset(DateTime.Now);
            var offsetStr = (offset.TotalMilliseconds < 0) ? offset.ToString() : "+" + offset.ToString();
            TimeZone = string.Format("{0} (UTC{1})", result, offsetStr); // (s[0]);

            string langs = INVISIBLE_PROFILE_VALUE;
            /*foreach (InputLanguage c in InputLanguage.InstalledInputLanguages) {
                langs += (langs.Length != 0) ? ", " : "";
                langs += (c.Culture.ThreeLetterISOLanguageName);
            }*/
            Languages = langs;

            Email = INVISIBLE_PROFILE_VALUE;

            PublicKey = string.Empty;
        }

        public override void Save(BDictionary data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            data.Add("uname", UserName);
            data.Add("uctry", (IsCountryVisible) ? Country : INVISIBLE_PROFILE_VALUE);
            data.Add("utz", (IsTimeZoneVisible) ? TimeZone : INVISIBLE_PROFILE_VALUE);
            data.Add("ulangs", (IsLanguagesVisible) ? Languages : INVISIBLE_PROFILE_VALUE);
            data.Add("uemail", (IsEmailVisible) ? Email : INVISIBLE_PROFILE_VALUE);
            data.Add("upublkey", PublicKey);
        }

        public void Identify(string password)
        {
            fPasswordHash = Utilities.HashPassword(password);
            Utilities.GenerateKeyPair(password, out fPublicKey, out fPrivateKey);
        }

        public bool Authenticate(string password)
        {
            return Utilities.VerifyPassword(password, fPasswordHash);
        }
    }
}
