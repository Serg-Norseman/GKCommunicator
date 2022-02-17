/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Collections.Generic;

namespace GKLocations.Common
{
    public interface IDatabase
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
        IList<ITransaction> QueryLocalTransactions();

        /*void AddLocation(ILocation location);
        void AddLocationName(ILocationName location);
        void AddLocationRelation(ILocationRelation location);

        void AddTransaction(DateTime timestamp, TransactionType type, string data);*/
    }
}
