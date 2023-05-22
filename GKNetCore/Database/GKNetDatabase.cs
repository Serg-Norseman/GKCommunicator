/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2022 by Sergey V. Zhdanovskih.
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using GKNet.Blockchain;
using GKNet.DHT;
using GKNet.Logging;
using SQLite;

namespace GKNet.Database
{
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class GKNetDatabase : IDataProvider
    {
        private SQLiteConnection fConnection;
        private readonly ILogger fLogger;

        public bool IsConnected
        {
            get { return (fConnection != null); }
        }

        public bool IsExists
        {
            get {
                string baseName = GetBaseName();
                return File.Exists(baseName);
            }
        }

        static GKNetDatabase()
        {
#if !NETSTANDARD && !NET461 && !MONO
            SQLiteLoader.Load();
#endif
        }

        public GKNetDatabase()
        {
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "GKNetDatabase");
        }

        private string GetBaseName()
        {
            return Path.Combine(Utilities.GetAppDataPath(), "gkcommunicator.db");
        }

        public void Connect()
        {
            if (IsConnected)
                throw new DatabaseException("Database already connected");

            if (!IsExists) {
                CreateDatabase();
            }

            string baseName = GetBaseName();
            fConnection = new SQLiteConnection(baseName);
        }

        public void Disconnect()
        {
            if (!IsConnected)
                throw new DatabaseException("Database already disconnected");

            fConnection.Close();
            fConnection.Dispose();
            fConnection = null;
        }

        public void DeleteDatabase()
        {
            if (IsConnected) Disconnect();

            string baseName = GetBaseName();
            try {
                if (File.Exists(baseName)) {
                    File.Delete(baseName);
                }
            } catch (Exception ex) {
                fLogger.WriteError("DeleteDatabase()", ex);
            }
        }

        public void CreateDatabase()
        {
            string baseName = GetBaseName();

            using (var connection = new SQLiteConnection(baseName/*, SQLiteOpenFlags.Create*/)) {
                connection.CreateTable<DBParameter>();
                connection.CreateTable<DBNode>();
                connection.CreateTable<DBPeer>();
                connection.CreateTable<DBMessage>();

                connection.CreateTable<DBTransactionRec>();
                connection.CreateTable<DBBlockRec>();
            }
        }

        #region Parameters

        public string GetParameterValue(string paramName)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            string query = string.Format("select value from Settings where parameter = '{0}'", paramName);
            var list = fConnection.Query<QString>(query);
            return (list != null && list.Count != 0) ? list[0].value : string.Empty;
        }

        public void SetParameterValue(string paramName, string paramValue)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            if (string.IsNullOrEmpty(paramValue))
                return;

            var param = new DBParameter() {
                parameter = paramName,
                value = paramValue
            };
            fConnection.InsertOrReplace(param);
        }

        public bool GetParameterBool(string paramName)
        {
            string val = GetParameterValue(paramName);
            if (string.IsNullOrEmpty(val)) {
                return false;
            } else {
                return bool.Parse(val);
            }
        }

        public void SetParameterBool(string paramName, bool paramValue)
        {
            SetParameterValue(paramName, paramValue.ToString());
        }

        #endregion

        #region User profile

        public PresenceStatus LoadPresence()
        {
            string strVal = GetParameterValue("user_presence");
            return (string.IsNullOrEmpty(strVal)) ? PresenceStatus.Online : (PresenceStatus)int.Parse(strVal);
        }

        public void SavePresence(PresenceStatus status)
        {
            var intVal = (int)status;
            SetParameterValue("user_presence", intVal.ToString());
        }

        public void LoadProfile(UserProfile profile)
        {
            bool initialized = GetParameterBool("profile_initialized");
            if (initialized) {
                profile.NodeId = DHTId.Parse(GetParameterValue("user_node_id"));

                profile.UserName = GetParameterValue("user_name");
                profile.Country = GetParameterValue("user_country");
                profile.TimeZone = GetParameterValue("user_timezone");
                profile.Languages = GetParameterValue("user_languages");
                profile.Email = GetParameterValue("user_email");

                profile.IsCountryVisible = GetParameterBool("user_country_visible");
                profile.IsTimeZoneVisible = GetParameterBool("user_timezone_visible");
                profile.IsLanguagesVisible = GetParameterBool("user_languages_visible");
                profile.IsEmailVisible = GetParameterBool("user_email_visible");

                profile.PasswordHash = GetParameterValue("user_password");
                profile.PublicKey = GetParameterValue("user_public_key");
                profile.PrivateKey = GetParameterValue("user_private_key");
            } else {
                profile.Reset();
                SaveProfile(profile);
            }
        }

        public void SaveProfile(UserProfile profile)
        {
            SetParameterBool("profile_initialized", true);

            SetParameterValue("user_node_id", profile.NodeId.ToString());

            SetParameterValue("user_name", profile.UserName);
            SetParameterValue("user_country", profile.Country);
            SetParameterValue("user_timezone", profile.TimeZone);
            SetParameterValue("user_languages", profile.Languages);
            SetParameterValue("user_email", profile.Email);

            SetParameterBool("user_country_visible", profile.IsCountryVisible);
            SetParameterBool("user_timezone_visible", profile.IsTimeZoneVisible);
            SetParameterBool("user_languages_visible", profile.IsLanguagesVisible);
            SetParameterBool("user_email_visible", profile.IsEmailVisible);

            SetParameterValue("user_password", profile.PasswordHash);
            SetParameterValue("user_public_key", profile.PublicKey);
            SetParameterValue("user_private_key", profile.PrivateKey);
        }

        #endregion

        #region Peer profiles

        public IEnumerable<DBPeer> LoadPeers()
        {
            string query = string.Format("select * from Peers");
            var records = fConnection.Query<DBPeer>(query);
            return records;
        }

        public void SavePeer(PeerProfile peerProfile, IPEndPoint endPoint)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            var record = new DBPeer() {
                node_id = peerProfile.NodeId.ToString(),
                last_endpoint = endPoint.ToString(),
                user_name = peerProfile.UserName,
                country = peerProfile.Country,
                timezone = peerProfile.TimeZone,
                langs = peerProfile.Languages,
                email = peerProfile.Email,
                public_key = peerProfile.PublicKey
            };
            fConnection.InsertOrReplace(record);
        }

        #endregion

        #region DHT nodes

        public IEnumerable<DHTNode> LoadNodes()
        {
            var result = new List<DHTNode>();

            string query = string.Format("select * from DHTNodes");
            var dbNodes = fConnection.Query<DBNode>(query);
            if (dbNodes != null) {
                foreach (var dbn in dbNodes) {
                    var nodeId = DHTId.Parse(dbn.node_id);
                    var endPoint = Utilities.ParseIPEndPoint(dbn.endpoint);
                    result.Add(new DHTNode(nodeId, endPoint));
                }
            }

            return result;
        }

        public void SaveNode(DHTNode node)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            var record = new DBNode() {
                node_id = node.Id.ToString(),
                endpoint = node.EndPoint.ToString()
            };
            fConnection.InsertOrReplace(record);
        }

        #endregion

        #region Messages

        public IEnumerable<Message> LoadMessages(string contactId)
        {
            var result = new List<Message>();

            string query = string.Format("select * from Messages where (sender = '{0}' or receiver = '{1}')", contactId, contactId);
            var records = fConnection.Query<DBMessage>(query);
            if (records != null) {
                foreach (var rec in records) {
                    var msg = Message.FromDBRecord(rec);
                    result.Add(msg);
                }
            }

            return result;
        }

        public void SaveMessage(Message message)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            var record = message.ToDBRecord();
            fConnection.Insert(record);
        }

        public void UpdateMessageDelivered(DHTId receiverId, long timestamp)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            fConnection.Execute(string.Format("update Messages set flags = 1 where (receiver = '{0}' and timestamp = {1})", receiverId, timestamp));
        }

        #endregion

        #region Tuples for aggregate queries

        private class QString
        {
            public string value { get; set; }
        }

        #endregion

        #region Blockchain Data Provider

        public void AddBlock(Block block)
        {
            var dtObj = new DBBlockRec(block);
            fConnection.Insert(dtObj);
        }

        public void ClearBlocks()
        {
            fConnection.DeleteAll<DBBlockRec>();
        }

        public IList<Block> GetBlocks()
        {
            var dtRecs = fConnection.Query<DBBlockRec>("select * from Blocks");

            var result = new List<Block>();
            foreach (var blk in dtRecs) {
                result.Add(blk.GetData());
            }
            return result;
        }


        public IList<Block> GetBlocks(long startBlockIndex)
        {
            var dtRecs = fConnection.Query<DBBlockRec>("select * from Blocks where \"Index\" > ? order by \"Index\" asc", startBlockIndex);

            var result = new List<Block>();
            foreach (var blk in dtRecs) {
                result.Add(blk.GetData());
            }
            return result;
        }

        public Block FindBlock(string hash)
        {
            var dtRecs = fConnection.Query<DBBlockRec>("select * from Blocks where Hash = ?", hash);

            if (dtRecs == null || dtRecs.Count < 1) {
                return null;
            } else {
                return dtRecs[0].GetData();
            }
        }

        public Block GetLastBlock()
        {
            var dtRecs = fConnection.Query<DBBlockRec>("select * from Blocks order by \"Index\" desc limit 1");

            if (dtRecs == null || dtRecs.Count < 1) {
                return null;
            } else {
                return dtRecs[0].GetData();
            }
        }

        public void AddPendingTransaction(Transaction transaction)
        {
            var dtObj = new DBTransactionRec(transaction);
            fConnection.Insert(dtObj);
        }

        public void ClearPendingTransactions()
        {
            fConnection.DeleteAll<DBTransactionRec>();
        }

        public IList<Transaction> GetPendingTransactions()
        {
            var dtRecs = fConnection.Query<DBTransactionRec>("select * from PendingTransactions");

            var result = new List<Transaction>();
            foreach (var trx in dtRecs) {
                result.Add(trx.GetData());
            }
            return result;
        }

        #endregion
    }
}
