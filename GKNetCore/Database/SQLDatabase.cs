/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
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

using System.Data;
using System.Data.SQLite;

namespace GKNet.Database
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SQLDatabase : IDatabase
    {
        private SQLiteConnection fConnection;

        public override bool IsConnected
        {
            get { return (fConnection != null); }
        }

        public SQLDatabase()
        {
        }

        protected override string GetBaseName()
        {
            return NetHelper.GetAppPath() + "gkcommunicator.sl3";
        }

        public override void Connect()
        {
            if (IsConnected)
                throw new DatabaseException("Database already connected");

            if (!IsExists) {
                CreateDatabase();
            }

            string baseName = GetBaseName();
            fConnection = (SQLiteConnection)SQLiteFactory.Instance.CreateConnection();
            fConnection.ConnectionString = "Data Source = " + baseName;
            fConnection.Open();
        }

        public override void Disconnect()
        {
            if (!IsConnected)
                throw new DatabaseException("Database already disconnected");

            fConnection.Close();
            fConnection.Dispose();
            fConnection = null;
        }

        public override string GetParameterValue(string paramName)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            using (SQLiteCommand cmd = fConnection.CreateCommand()) {
                cmd.CommandText = "select [value] from Settings where [parameter] = \"" + paramName + "\"";
                SQLiteDataReader r = cmd.ExecuteReader();
                if (r.Read()) {
                    return r.GetString(0);
                } else {
                    return string.Empty;
                }
            }
        }

        public override void SetParameterValue(string paramName, string paramValue)
        {
            if (!IsConnected)
                throw new DatabaseException("Database disconnected");

            using (SQLiteCommand cmd = fConnection.CreateCommand()) {
                cmd.CommandText = string.Format("replace into Settings (parameter, value) values (\"{0}\", \"{1}\")", paramName, paramValue);
                cmd.ExecuteNonQuery();
            }
        }

        public override void CreateDatabase()
        {
            string baseName = GetBaseName();

            SQLiteConnection.CreateFile(baseName);

            using (SQLiteConnection connection = (SQLiteConnection)SQLiteFactory.Instance.CreateConnection()) {
                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection)) {
                    command.CommandText = @"create table [Messages] (
                    [id] integer primary key autoincrement not null,
                    [sender] char(20) not null,
                    [receiver] char(20) not null,
                    [timestamp] int not null,
                    [flags] int not null,
                    [msg_text] text not null);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command = new SQLiteCommand(connection)) {
                    command.CommandText = @"create table [DHTNodes] (
                    [node_id] char(20) not null,
                    [address] char(40) not null,
                    [port] int not null);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command = new SQLiteCommand(connection)) {
                    command.CommandText = @"create table [Peers] (
                    [node_id] char(20) not null,
                    [address] char(40) not null,
                    [port] int not null,
                    [user_name] varchar(40) not null,
                    [country] varchar(200),
                    [timezone] varchar(200),
                    [langs] varchar(200));";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                // self_id, user_name, ctry_visible, tz_visible, langs_visible
                using (SQLiteCommand command = new SQLiteCommand(connection)) {
                    command.CommandText = @"create table [Settings] (
                    [parameter] varchar(200) primary key,
                    [value] varchar(200) not null);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
