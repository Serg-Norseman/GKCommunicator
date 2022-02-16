/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using GKLocations.Database;
using GKLocations.Core.Model;
using GKLocations.Common;

namespace GKLocations.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class GKLCore : ICore
    {
        private readonly IDatabase fDatabase;

        public GKLCore()
        {
            fDatabase = new GKLDatabase();
            fDatabase.SetPath(GetDataPath());
            fDatabase.Connect();
        }

        public void DeleteDatabase()
        {
            fDatabase.Disconnect();
            fDatabase.DeleteDatabase();
            fDatabase.Connect();
        }

        #region Path processing

        public string GetBinPath()
        {
            Assembly asm = Assembly.GetEntryAssembly();
            if (asm == null) {
                asm = Assembly.GetExecutingAssembly();
            }

            Module[] mods = asm.GetModules();
            string fn = mods[0].FullyQualifiedName;
            return Path.GetDirectoryName(fn) + Path.DirectorySeparatorChar;
        }

        public string GetAppPath()
        {
            string result = Path.GetFullPath(Path.Combine(GetBinPath(), @".." + Path.DirectorySeparatorChar));
            return result;
        }

        public string GetDataPath()
        {
            string result = Path.GetFullPath(Path.Combine(GetBinPath(), @".." + Path.DirectorySeparatorChar + "appdata" + Path.DirectorySeparatorChar));
            return result;
        }

        #endregion

        #region Language processing

        public string GetCurrentLanguage()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;
            return (currentCulture == null) ? string.Empty : currentCulture.Name;
        }

        public IList<string> GetUsedLanguages()
        {
            IList<string> result = fDatabase.QueryLanguages();

            string currentLang = GetCurrentLanguage();
            if (!string.IsNullOrEmpty(currentLang) && !result.Contains(currentLang)) {
                result.Add(currentLang);
            }

            return result;
        }

        #endregion

        private static string NewGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public Location AddLocation(double latitude = 0.0d, double longitude = 0.0d)
        {
            string locationGUID = NewGUID();

            var result = new Location() {
                GUID = locationGUID,
                Latitude = latitude,
                Longitude = longitude
            };

            // save to local db
            fDatabase.AddLocation(result);

            // save to local transaction pool
            string json = JsonHelper.SerializeObject(result);
            fDatabase.AddTransaction(DateTime.UtcNow, TransactionType.AddLocation, json);

            return result;
        }

        public LocationName AddLocationName(string locationGUID, string name, string type, string description, string actualDates, string language)
        {
            string locationNameGUID = NewGUID();

            var result = new LocationName() {
                GUID = locationNameGUID,
                LocationGUID = locationGUID,
                Name = name,
                Type = type,
                Description = description,
                ActualDates = actualDates,
                Language = language
            };

            // save to local db
            fDatabase.AddLocationName(result);

            // save to local transaction pool
            string json = JsonHelper.SerializeObject(result);
            fDatabase.AddTransaction(DateTime.UtcNow, TransactionType.AddLocationName, json);

            return result;
        }

        public LocationRelation AddLocationRelation(string locationGUID, string ownerGUID, string relationType, string actualDates)
        {
            string locationRelationGUID = NewGUID();

            var result = new LocationRelation() {
                GUID = locationRelationGUID,
                LocationGUID = locationGUID,
                OwnerGUID = ownerGUID,
                RelationType = relationType,
                ActualDates = actualDates
            };

            // save to local db
            fDatabase.AddLocationRelation(result);

            // save to local transaction pool
            string json = JsonHelper.SerializeObject(result);
            fDatabase.AddTransaction(DateTime.UtcNow, TransactionType.AddLocationRelation, json);

            return result;
        }
    }
}
