/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Collections.Generic;
using GKNet.Blockchain;

namespace GKNetLocationsPlugin
{
    public class QLocation
    {
        public string LocationGUID { get; set; }
        public string OwnerGUID { get; set; }
        public string RelationType { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
    }

    public interface IDatabase : IDataProvider
    {
        void SetPath(string dbPath);
        void Connect();
        void Disconnect();
        void DeleteDatabase();

        int AddRecord(object obj);
        void UpdateRecord(object obj);
        void DeleteRecord(object obj);
        int DeleteRecord<T>(object primaryKey);

        IList<ILocation> QueryLocations();
        IList<ILocationName> QueryLocationNames();
        IList<string> QueryLanguages();
        IList<ILocationRelation> QueryLocationRelations();

        IList<QLocation> QueryLocationsEx(string lang);
    }
}
