/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System.Collections.Generic;

namespace GKLocations.Common
{
    public interface IDatabase
    {
        void SetPath(string dbPath);
        void Connect();
        void Disconnect();

        IList<ILocation> QueryLocations();
        IList<ILocationName> QueryLocationNames();
        IList<string> QueryLanguages();
        IList<ILocationRelation> QueryLocationRelations();
        IList<ITransaction> QueryLocalTransactions();
    }
}
