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
    public sealed class LocationTransactionSolver : BaseTransactionSolver
    {
        public override string Sign
        {
            get {
                return "location";
            }
        }


        public LocationTransactionSolver(GKLCore core) : base(core)
        {
        }

        public override void Solve(IBlockchainNode node, Transaction transaction)
        {
            string typeOperator = transaction.GetTypeOperator();

            var loc = transaction.DeserializeContent<Location>();

            switch (typeOperator) {
                case TransactionType.Oper_Create:
                    fCore.Database.AddRecord(new DBLocationRec(loc));
                    break;

                case TransactionType.Oper_Update:
                    fCore.Database.UpdateRecord(new DBLocationRec(loc));
                    break;

                case TransactionType.Oper_Delete:
                    fCore.Database.DeleteRecord<DBLocationRec>(loc);
                    break;
            }
        }

        public override bool Verify(Transaction transaction)
        {
            try {
                string typeOperator = transaction.GetTypeOperator();

                var loc = transaction.DeserializeContent<Location>();

                // TODO: check record contents 
                bool result;
                switch (typeOperator) {
                    case TransactionType.Oper_Create:
                        result = !string.IsNullOrEmpty(loc.GUID);
                        break;

                    case TransactionType.Oper_Update:
                    case TransactionType.Oper_Delete:
                        result = !string.IsNullOrEmpty(loc.GUID) && fCore.ExistsRecord<Location>(loc.GUID);
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
