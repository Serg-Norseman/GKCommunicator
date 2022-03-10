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
using GKLocations.Blockchain;
using GKLocations.Core.Database;
using GKLocations.Core.Model;
using GKLocations.Utils;

namespace GKLocations.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class GKLCore : ICore
    {
        private readonly IBlockchainNode fBlockchainNode;
        private readonly IDatabase fDatabase;

        public GKLCore()
        {
            fDatabase = new GKLDatabase();
            fDatabase.SetPath(GetDataPath());
            fDatabase.Connect();

            fBlockchainNode = new BlockchainNode(fDatabase);
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

        private void AddPendingTransaction(string type, object data)
        {
            string json = JsonHelper.SerializeObject(data);
            fDatabase.AddRecord(new DBTransactionRec(TimeHelper.DateTimeToUnixTime(DateTime.UtcNow), type, json));
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
            fDatabase.AddRecord(new DBLocationRec(result));

            // save to local transaction pool
            AddPendingTransaction(TransactionType.Location_Create, result);

            return result;
        }

        public Location UpdateLocation(string locationGUID, double latitude = 0.0d, double longitude = 0.0d)
        {
            var result = new Location() {
                GUID = locationGUID,
                Latitude = latitude,
                Longitude = longitude
            };

            // save to local db
            fDatabase.UpdateRecord(new DBLocationRec(result));

            // save to local transaction pool
            AddPendingTransaction(TransactionType.Location_Update, result);

            return result;
        }

        public void DeleteLocation(string locationGUID)
        {
            var result = new Entity() {
                GUID = locationGUID,
            };

            // save to local db
            fDatabase.DeleteRecord<DBLocationRec>(locationGUID);

            // save to local transaction pool
            AddPendingTransaction(TransactionType.Location_Delete, result);
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
            fDatabase.AddRecord(new DBLocationNameRec(result));

            // save to local transaction pool
            AddPendingTransaction(TransactionType.LocationName_Create, result);

            return result;
        }

        public LocationName UpdateLocationName(string locationNameGUID, string locationGUID, string name, string type, string description, string actualDates, string language)
        {
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
            fDatabase.UpdateRecord(new DBLocationNameRec(result));

            // save to local transaction pool
            AddPendingTransaction(TransactionType.LocationName_Update, result);

            return result;
        }

        public void DeleteLocationName(string locationNameGUID)
        {
            var result = new Entity() {
                GUID = locationNameGUID,
            };

            // save to local db
            fDatabase.DeleteRecord<DBLocationNameRec>(locationNameGUID);

            // save to local transaction pool
            AddPendingTransaction(TransactionType.LocationName_Delete, result);
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
            fDatabase.AddRecord(new DBLocationRelationRec(result));

            // save to local transaction pool
            AddPendingTransaction(TransactionType.LocationRelation_Create, result);

            return result;
        }

        public LocationRelation UpdateLocationRelation(string locationRelationGUID, string locationGUID, string ownerGUID, string relationType, string actualDates)
        {
            var result = new LocationRelation() {
                GUID = locationRelationGUID,
                LocationGUID = locationGUID,
                OwnerGUID = ownerGUID,
                RelationType = relationType,
                ActualDates = actualDates
            };

            // save to local db
            fDatabase.UpdateRecord(new DBLocationRelationRec(result));

            // save to local transaction pool
            AddPendingTransaction(TransactionType.LocationRelation_Update, result);

            return result;
        }

        public void DeleteLocationRelation(string locationRelationGUID)
        {
            var result = new Entity() {
                GUID = locationRelationGUID,
            };

            // save to local db
            fDatabase.DeleteRecord<DBLocationRelationRec>(locationRelationGUID);

            // save to local transaction pool
            AddPendingTransaction(TransactionType.LocationRelation_Delete, result);
        }
    }
}
