/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GKLocations.Blockchain;
using SQLite;

namespace GKLocations.Core.Database
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
    public class GKLDatabase : IDatabase, IDataProvider
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
            fConnection.CreateTable<DBLocationRelationRec>();

            fConnection.CreateTable<DBTransactionRec>();
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

        public int DeleteRecord<T>(object primaryKey)
        {
            return fConnection.Delete<T>(primaryKey);
        }

        /*public void DeleteRecord<T>(int objId)
        {
            fConnection.Delete<T>(objId);
        }*/

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

        public IList<ILocation> QueryLocations()
        {
            return (IList<ILocation>)fConnection.Query<DBLocationRec>("select * from Locations");
        }

        public IList<ILocationName> QueryLocationNames()
        {
            return (IList<ILocationName>)fConnection.Query<DBLocationNameRec>("select * from LocationNames");
        }

        public IList<string> QueryLanguages()
        {
            var result = fConnection.Query<QString>("select distinct [Language] as element from LocationNames");
            return GetStringList(result);
        }

        public IList<ILocationRelation> QueryLocationRelations()
        {
            return (IList<ILocationRelation>)fConnection.Query<DBLocationRelationRec>("select * from LocationRelations");
        }

        public IList<ITransaction> QueryLocalTransactions()
        {
            return (IList<ITransaction>)fConnection.Query<DBTransactionRec>("select * from LocalTransactions");
        }

        /*public void AddLocation(ILocation location)
        {
            var dtObj = new DBLocationRec(location);
            AddRecord(dtObj);
        }

        public void AddLocationName(ILocationName locationName)
        {
            var dtObj = new DBLocationNameRec(locationName);
            AddRecord(dtObj);
        }

        public void AddLocationRelation(ILocationRelation locationRelation)
        {
            var dtObj = new DBLocationRelationRec(locationRelation);
            AddRecord(dtObj);
        }

        public void AddTransaction(DateTime timestamp, TransactionType type, string data)
        {
            var dtObj = new DBTransactionRec(timestamp, type, data);
            AddRecord(dtObj);
        }*/

        public void AddBlock(SerializableBlock block)
        {
            throw new NotImplementedException();
        }

        public List<SerializableBlock> GetBlocks()
        {
            throw new NotImplementedException();
        }

        public void ClearBlocks()
        {
            throw new NotImplementedException();
        }

        public IList<QLocation> QueryLocationsEx(string lang)
        {
            return fConnection.Query<QLocation>("select locrel.OwnerGUID, locrel.RelationType, locnam.LocationGUID, locnam.Name, locnam.Language from LocationNames locnam left join LocationRelations locrel on locnam.LocationGUID = locrel.LocationGUID where locnam.Language = ?", lang); // 'ru-RU'
        }
    }
}
