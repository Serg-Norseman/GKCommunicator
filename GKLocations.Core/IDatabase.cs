/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Collections.Generic;
using GKLocations.Blockchain;

namespace GKLocations.Core
{
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
    }
}
