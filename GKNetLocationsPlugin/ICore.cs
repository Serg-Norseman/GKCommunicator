/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Collections.Generic;
using GKNet.Blockchain;
using GKNetLocationsPlugin.Model;

namespace GKNetLocationsPlugin
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICore
    {
        IBlockchainNode BlockchainNode { get; }
        IDatabase Database { get; }

        string GetAppPath();
        string GetBinPath();
        string GetDataPath();
        void DeleteDatabase();

        string GetCurrentLanguage();
        IList<string> GetUsedLanguages();

        Location AddLocation(double latitude = 0.0d, double longitude = 0.0d);
        Location UpdateLocation(string locationGUID, double latitude = 0.0d, double longitude = 0.0d);
        void DeleteLocation(string locationGUID);

        LocationName AddLocationName(string locationGUID, string name, string type, string description, string actualDates, string language);
        LocationName UpdateLocationName(string locationNameGUID, string locationGUID, string name, string type, string description, string actualDates, string language);
        void DeleteLocationName(string locationNameGUID);

        LocationRelation AddLocationRelation(string locationGUID, string ownerGUID, string relationType, string actualDates);
        LocationRelation UpdateLocationRelation(string locationRelationGUID, string locationGUID, string ownerGUID, string relationType, string actualDates);
        void DeleteLocationRelation(string locationRelationGUID);
    }
}
