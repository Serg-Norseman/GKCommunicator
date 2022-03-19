﻿/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2022 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GKCommunicator".
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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using GKNet;
using GKNetLocationsPlugin.Database;
using GKNetLocationsPlugin.Transactions;

namespace GKNetLocationsPlugin.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class GKLCore
    {
        private readonly ICommunicatorCore fHost;
        private readonly GKLDatabase fDatabase;


        public GKLDatabase Database
        {
            get {
                return fDatabase;
            }
        }


        public GKLCore(ICommunicatorCore host)
        {
            fHost = host;

            fDatabase = new GKLDatabase(host.GetDataPath());
            fDatabase.Connect();
        }

        public void DeleteDatabase()
        {
            fDatabase.Disconnect();
            fDatabase.DeleteDatabase();
            fDatabase.Connect();
        }

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
            fHost.BlockchainNode.AddPendingTransaction(type, data);
        }

        // TODO: check the existing record in the database and in the transaction pool
        public bool ExistsRecord<T>(string recordGUID)
        {
            return true;
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
