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
using System.IO;
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
            return NetHelper.GetAppPath() + "gkcommunicator.db";
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

            var param = new Parameter() {
                parameter = paramName,
                value = paramValue
            };
            fConnection.InsertOrReplace(param);
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
                // TODO: self_id, ctry_visible, tz_visible, langs_visible
                connection.CreateTable<Parameter>();

                /*var command = @"create table [Messages] (
                    [id] integer primary key autoincrement not null,
                    [sender] char(20) not null,
                    [receiver] char(20) not null,
                    [timestamp] int not null,
                    [flags] int not null,
                    [msg_text] text not null);";
                connection.Execute(command);

                command = @"create table [DHTNodes] (
                    [node_id] char(20) not null,
                    [address] char(40) not null,
                    [port] int not null);";
                connection.Execute(command);

                command = @"create table [Peers] (
                    [node_id] char(20) not null,
                    [address] char(40) not null,
                    [port] int not null,
                    [user_name] nvarchar(40) not null,
                    [country] nvarchar(200),
                    [timezone] nvarchar(200),
                    [langs] nvarchar(200));";
                connection.Execute(command);*/
            }
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

        public void LoadProfile(UserProfile profile)
        {
            bool initialized = GetParameterBool("profile_initialized");
            if (initialized) {
                profile.UserName = GetParameterValue("user_name");
                profile.Country = GetParameterValue("user_country");
                profile.TimeZone = GetParameterValue("user_timezone");
                profile.Languages = GetParameterValue("user_languages");
            } else {
                profile.ResetSystem();
                SaveProfile(profile);
            }
        }

        public void SaveProfile(UserProfile profile)
        {
            SetParameterBool("profile_initialized", true);
            SetParameterValue("user_name", profile.UserName);
            SetParameterValue("user_country", profile.Country);
            SetParameterValue("user_timezone", profile.TimeZone);
            SetParameterValue("user_languages", profile.Languages);
        }

        #region Tuples for aggregate queries

        private class QString
        {
            public string value { get; set; }
        }

        #endregion
    }
}
