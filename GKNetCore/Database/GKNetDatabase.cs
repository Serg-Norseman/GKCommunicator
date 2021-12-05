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
using System.Collections.Generic;
using System.IO;
using System.Net;
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
    public sealed class GKNetDatabase
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
            return Utilities.GetAppPath() + "gkcommunicator.db";
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

            using (var connection = new SQLiteConnection(baseName)) {
                connection.CreateTable<DBParameter>();
                connection.CreateTable<DBNode>();
                connection.CreateTable<DBPeer>();
                connection.CreateTable<DBMessage>();
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

        public void LoadProfile(UserProfile profile)
        {
            bool initialized = GetParameterBool("profile_initialized");
            if (initialized) {
                profile.NodeId = DHTId.FromHex(GetParameterValue("user_node_id"));

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

            SetParameterValue("user_node_id", profile.NodeId.ToHex());

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
                node_id = peerProfile.NodeId.ToHex(),
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
                    var nodeId = DHTId.FromHex(dbn.node_id);
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
                node_id = node.Id.ToHex(),
                endpoint = node.EndPoint.ToString()
            };
            fConnection.InsertOrReplace(record);
        }

        #endregion

        #region Tuples for aggregate queries

        private class QString
        {
            public string value { get; set; }
        }

        #endregion
    }
}
