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
using GKLocations.Database;

namespace GKLocations.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class GKLCore : ICore
    {
        private readonly GKLDatabase fDatabase;

        public GKLCore()
        {
            fDatabase = new GKLDatabase();
            fDatabase.SetPath(GetDataPath());
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
    }
}
