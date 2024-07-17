/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
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
using System.IO;
using GKNetLocationsPlugin.Dates;
using SQLite;

namespace GKNetLocationsPlugin.Model
{
    public class GKLDatabaseException : Exception
    {
        public GKLDatabaseException(string message) : base(message)
        {
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class GKLDatabase
    {
        private class QString
        {
            public string value { get; set; }
        }

        private class QDecimal
        {
            public double value { get; set; }
        }


        private SQLiteConnection fConnection;
        private string fDatabasePath;


        static GKLDatabase()
        {
#if !NETSTANDARD && !NET461
            SQLiteLoader.Load();
#endif
        }

        public GKLDatabase(string dbPath)
        {
            fDatabasePath = dbPath;
        }

        public void Connect()
        {
            if (fConnection != null)
                throw new GKLDatabaseException("Database already connected");

            string databaseName = GetBaseName();
            fConnection = new SQLiteConnection(databaseName);

            CreateDatabase();
        }

        public void Disconnect()
        {
            if (fConnection == null)
                throw new GKLDatabaseException("Database already disconnected");

            fConnection.Close();
            fConnection.Dispose();
            fConnection = null;
        }

        /// <summary>
        /// Cleaning waste space
        /// </summary>
        public void CleanSpace()
        {
            fConnection.Execute("VACUUM;");
        }

        public string GetBaseName()
        {
            return Path.Combine(fDatabasePath, "GKLocations.db3");
        }

        public void DeleteDatabase()
        {
            string fileName = GetBaseName();
            if (File.Exists(fileName)) {
                File.Delete(fileName);
            }
        }

        private void CreateDatabase()
        {
            fConnection.CreateTable<DBLocationRec>();
            fConnection.CreateTable<DBLocationNameRec>();
            fConnection.CreateTable<DBLocationNameTranslationRec>();
            fConnection.CreateTable<DBLocationRelationRec>();
        }

        #region Records

        public void Execute(string query, params object[] args)
        {
            fConnection.Execute(query, args);
        }

        public int AddRecord(object obj)
        {
            return fConnection.Insert(obj);
        }

        public void UpdateRecord(object obj)
        {
            fConnection.Update(obj);
        }

        public int DeleteRecord<T>(object primaryKey)
        {
            return fConnection.Delete<T>(primaryKey);
        }

        public T GetRecord<T>(int objId) where T : new()
        {
            T result;
            if (objId <= 0) {
                result = default(T);
            } else {
                try {
                    result = fConnection.Get<T>(objId);
                } catch (InvalidOperationException) {
                    // record not exists
                    result = default(T);
                }
            }
            return result;
        }

        public IList<T> QueryRecords<T>(string query, params object[] args) where T : new()
        {
            return fConnection.Query<T>(query, args);
        }

        private static IList<string> GetStringList(IEnumerable<QString> queryStrings)
        {
            var result = new List<string>();
            foreach (var qs in queryStrings) {
                if (!string.IsNullOrEmpty(qs.value))
                    result.Add(qs.value);
            }
            return result;
        }

        #endregion

        #region Locations records

        public List<DBLocationRec> QueryLocations()
        {
            return fConnection.Query<DBLocationRec>("select * from Locations");
        }

        public List<DBLocationNameRec> QueryLocationNames(string locGUID)
        {
            return fConnection.Query<DBLocationNameRec>("select * from LocationNames where LocationGUID = ?", locGUID);
        }

        public IList<string> QueryLanguages()
        {
            var result = fConnection.Query<QString>("select [Language] as value from LocationNames union select [Language] as value from LocationNameTranslations");
            return GetStringList(result);
        }

        public List<DBLocationRelationRec> QueryLocationRelations(string locGUID)
        {
            return fConnection.Query<DBLocationRelationRec>("select * from LocationRelations where LocationGUID = ?", locGUID);
        }

        public IList<string> QueryLowerLocations(string locGUID)
        {
            return GetStringList(fConnection.Query<QString>(
                "WITH RECURSIVE hierarchy AS (" +
                "    SELECT lh.LocationGUID, lh.OwnerGUID FROM LocationRelations lh" +
                "    WHERE lh.OwnerGUID = ?" +
                "    UNION ALL" +
                "    SELECT lh.LocationGUID, lh.OwnerGUID FROM LocationRelations lh" +
                "    JOIN hierarchy h ON lh.OwnerGUID = h.LocationGUID" +
                ")" +
                "SELECT LocationGUID as value FROM hierarchy;", locGUID));
        }

        public IList<string> QueryHigherLocations(string locGUID)
        {
            return GetStringList(fConnection.Query<QString>(
                "WITH RECURSIVE hierarchy AS (" +
                "    SELECT lh.LocationGUID, lh.OwnerGUID FROM LocationRelations lh" +
                "    WHERE lh.LocationGUID = ?" +
                "    UNION ALL" +
                "    SELECT lh.LocationGUID, lh.OwnerGUID FROM LocationRelations lh" +
                "    JOIN hierarchy h ON lh.LocationGUID = h.OwnerGUID" +
                ")" +
                "SELECT LocationGUID as value FROM hierarchy union select l.GUID as value from Locations l " +
                "join hierarchy h on l.GUID = h.OwnerGUID;", locGUID));
        }

        public void PreloadLocations(List<DBLocationRec> locs)
        {
            foreach (var loc in locs) {
                LoadLocationExt(loc);
            }
        }

        public DBLocationRec LoadLocation(string locGUID)
        {
            var result = fConnection.Query<DBLocationRec>("select * from Locations where GUID = ?", locGUID);
            if (result.Count != 1) {
                return null;
            } else {
                var rec = result[0];
                LoadLocationExt(rec);
                return rec;
            }
        }

        public void LoadLocationExt(DBLocationRec rec)
        {
            var names = QueryLocationNames(rec.GUID);
            if (names.Count != 0) {
                rec.Names = names;
                foreach (var name in names) {
                    var dtx = new GDMDatePeriod();
                    dtx.ParseString(name.ActualDates);
                    name.ActualDatesEx = dtx;
                }
            }

            var rels = QueryLocationRelations(rec.GUID);
            if (names.Count != 0) {
                rec.Relations = rels;
                foreach (var rel in rels) {
                    var dtx = new GDMDatePeriod();
                    dtx.ParseString(rel.ActualDates);
                    rel.ActualDatesEx = dtx;
                }
            }
        }

        #endregion
    }
}
