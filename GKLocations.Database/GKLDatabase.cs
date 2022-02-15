/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SQLite;

namespace GKLocations.Database
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
            public string element { get; set; }
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

        public GKLDatabase()
        {
        }

        public void SetPath(string dbPath)
        {
            fDatabasePath = dbPath;
        }

        public void Connect()
        {
            if (fConnection != null)
                throw new GKLDatabaseException("Database already connected");

            string databaseName = GetBaseName();

            Debug.WriteLine("DatabaseName: " + databaseName);

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

        private void CreateDatabase()
        {
            fConnection.CreateTable<Location>();
            fConnection.CreateTable<LocationName>();
            fConnection.CreateTable<LocationRelation>();

            fConnection.CreateTable<LocalTransaction>();
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

        public void DeleteRecord(object obj)
        {
            fConnection.Delete(obj);
        }

        public void DeleteRecord<T>(int objId)
        {
            fConnection.Delete<T>(objId);
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
                if (!string.IsNullOrEmpty(qs.element))
                    result.Add(qs.element);
            }
            return result;
        }

        #endregion

        public IList<Location> QueryLocations()
        {
            return fConnection.Query<Location>("select * from Locations");
        }

        public IList<LocationName> QueryLocationNames()
        {
            return fConnection.Query<LocationName>("select * from LocationNames");
        }

        public IList<string> QueryLanguages()
        {
            var result = fConnection.Query<QString>("select distinct [Language] as element from LocationNames");
            return GetStringList(result);
        }

        public IList<LocationRelation> QueryLocationRelations()
        {
            return fConnection.Query<LocationRelation>("select * from LocationRelations");
        }

        public IList<LocalTransaction> QueryLocalTransactions()
        {
            return fConnection.Query<LocalTransaction>("select * from LocalTransactions");
        }
    }
}
