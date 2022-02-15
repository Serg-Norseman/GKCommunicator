/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;

namespace GKLocations.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICore
    {
        string GetAppPath();
        string GetBinPath();
        string GetDataPath();

        string GetCurrentLanguage();
        IList<string> GetUsedLanguages();
    }
}
