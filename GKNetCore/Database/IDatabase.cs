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

using System;
using System.IO;
using GKNet.Logging;

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
    public abstract class IDatabase
    {
        protected readonly ILogger fLogger;

        public abstract bool IsConnected { get; }

        public bool IsExists
        {
            get {
                string baseName = GetBaseName();
                return File.Exists(baseName);
            }
        }

        protected IDatabase()
        {
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "IDatabase");
        }

        protected abstract string GetBaseName();

        public abstract void Connect();
        public abstract void Disconnect();

        public abstract void CreateDatabase();

        public void DeleteDatabase()
        {
            if (IsConnected) Disconnect();

            string baseName = GetBaseName();
            try {
                if (File.Exists(baseName)) {
                    File.Delete(baseName);
                }
            } catch (Exception ex) {
                fLogger.WriteError("IDatabase.DeleteDatabase()", ex);
            }
        }

        public abstract string GetParameterValue(string paramName);
        public abstract void SetParameterValue(string paramName, string paramValue);

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

        public static IDatabase CreateDefault()
        {
            return new LtDatabase();
        }
    }
}
