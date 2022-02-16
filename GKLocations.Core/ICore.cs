﻿/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;
using GKLocations.Core.Model;

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
        void DeleteDatabase();

        string GetCurrentLanguage();
        IList<string> GetUsedLanguages();

        Location AddLocation(double latitude = 0.0d, double longitude = 0.0d);
        LocationName AddLocationName(string locationGUID, string name, string type, string description, string actualDates, string language);
        LocationRelation AddLocationRelation(string locationGUID, string ownerGUID, string relationType, string actualDates);
    }
}
