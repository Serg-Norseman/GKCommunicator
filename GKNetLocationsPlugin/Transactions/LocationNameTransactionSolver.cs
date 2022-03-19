/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2022 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GKCommunicator".
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

using GKNet.Blockchain;
using GKNetLocationsPlugin.Database;
using GKNetLocationsPlugin.Model;

namespace GKNetLocationsPlugin.Transactions
{
    public sealed class LocationNameTransactionSolver : BaseTransactionSolver
    {
        public override string Sign
        {
            get {
                return "location_name";
            }
        }


        public LocationNameTransactionSolver(GKLCore core) : base(core)
        {
        }

        public override void Solve(IBlockchainNode node, Transaction transaction)
        {
            string typeOperator = transaction.GetTypeOperator();

            var locName = transaction.DeserializeContent<LocationName>();

            switch (typeOperator) {
                case TransactionType.Oper_Create:
                    fCore.Database.AddRecord(new DBLocationNameRec(locName));
                    break;

                case TransactionType.Oper_Update:
                    fCore.Database.UpdateRecord(new DBLocationNameRec(locName));
                    break;

                case TransactionType.Oper_Delete:
                    fCore.Database.DeleteRecord<DBLocationNameRec>(locName);
                    break;
            }
        }

        public override bool Verify(Transaction transaction)
        {
            try {
                string typeOperator = transaction.GetTypeOperator();

                var locName = transaction.DeserializeContent<LocationName>();

                // TODO: check record contents 
                bool result;
                switch (typeOperator) {
                    case TransactionType.Oper_Create:
                        result = !string.IsNullOrEmpty(locName.GUID);
                        break;

                    case TransactionType.Oper_Update:
                    case TransactionType.Oper_Delete:
                        result = !string.IsNullOrEmpty(locName.GUID) && fCore.ExistsRecord<LocationName>(locName.GUID);
                        break;

                    default:
                        result = false;
                        break;
                }
                return result;
            } catch {
                return false;
            }
        }
    }
}
