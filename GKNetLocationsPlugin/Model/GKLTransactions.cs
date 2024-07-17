/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
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

namespace GKNetLocationsPlugin.Model
{
    public static class TransactionType
    {
        public const string Oper_Create = "create";
        public const string Oper_Update = "update";
        public const string Oper_Delete = "delete";

        public const string Location_Create = "location:create";
        public const string Location_Update = "location:update";
        public const string Location_Delete = "location:delete";

        public const string LocationName_Create = "location_name:create";
        public const string LocationName_Update = "location_name:update";
        public const string LocationName_Delete = "location_name:delete";

        public const string LocationNameTranslation_Create = "location_name_translation:create";
        public const string LocationNameTranslation_Update = "location_name_translation:update";
        public const string LocationNameTranslation_Delete = "location_name_translation:delete";

        public const string LocationRelation_Create = "location_relation:create";
        public const string LocationRelation_Update = "location_relation:update";
        public const string LocationRelation_Delete = "location_relation:delete";
    }


    public abstract class BaseTransactionSolver : ITransactionSolver
    {
        protected readonly GKLCore fCore;


        public abstract string Sign { get; }


        public BaseTransactionSolver(GKLCore core)
        {
            fCore = core;
        }

        public abstract void Solve(IBlockchainNode node, Transaction transaction);

        public abstract bool Verify(Transaction transaction);
    }


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


    public sealed class LocationNameTranslationTransactionSolver : BaseTransactionSolver
    {
        public override string Sign
        {
            get {
                return "location_name_translation";
            }
        }


        public LocationNameTranslationTransactionSolver(GKLCore core) : base(core)
        {
        }

        public override void Solve(IBlockchainNode node, Transaction transaction)
        {
            string typeOperator = transaction.GetTypeOperator();

            var locNameTranslation = transaction.DeserializeContent<LocationNameTranslation>();

            switch (typeOperator) {
                case TransactionType.Oper_Create:
                    fCore.Database.AddRecord(new DBLocationNameTranslationRec(locNameTranslation));
                    break;

                case TransactionType.Oper_Update:
                    fCore.Database.UpdateRecord(new DBLocationNameTranslationRec(locNameTranslation));
                    break;

                case TransactionType.Oper_Delete:
                    fCore.Database.DeleteRecord<DBLocationNameTranslationRec>(locNameTranslation);
                    break;
            }
        }

        public override bool Verify(Transaction transaction)
        {
            try {
                string typeOperator = transaction.GetTypeOperator();

                var locNameTranslation = transaction.DeserializeContent<LocationNameTranslation>();

                // TODO: check record contents 
                bool result;
                switch (typeOperator) {
                    case TransactionType.Oper_Create:
                        result = !string.IsNullOrEmpty(locNameTranslation.GUID);
                        break;

                    case TransactionType.Oper_Update:
                    case TransactionType.Oper_Delete:
                        result = !string.IsNullOrEmpty(locNameTranslation.GUID) && fCore.ExistsRecord<LocationNameTranslation>(locNameTranslation.GUID);
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


    public sealed class LocationRelationTransactionSolver : BaseTransactionSolver
    {
        public override string Sign
        {
            get {
                return "location_relation";
            }
        }


        public LocationRelationTransactionSolver(GKLCore core) : base(core)
        {
        }

        public override void Solve(IBlockchainNode node, Transaction transaction)
        {
            string typeOperator = transaction.GetTypeOperator();

            var locRel = transaction.DeserializeContent<LocationRelation>();

            switch (typeOperator) {
                case TransactionType.Oper_Create:
                    fCore.Database.AddRecord(new DBLocationRelationRec(locRel));
                    break;

                case TransactionType.Oper_Update:
                    fCore.Database.UpdateRecord(new DBLocationRelationRec(locRel));
                    break;

                case TransactionType.Oper_Delete:
                    fCore.Database.DeleteRecord<DBLocationRelationRec>(locRel);
                    break;
            }
        }

        public override bool Verify(Transaction transaction)
        {
            try {
                string typeOperator = transaction.GetTypeOperator();

                var locRel = transaction.DeserializeContent<LocationRelation>();

                // TODO: check record contents 
                bool result;
                switch (typeOperator) {
                    case TransactionType.Oper_Create:
                        result = !string.IsNullOrEmpty(locRel.GUID);
                        break;

                    case TransactionType.Oper_Update:
                    case TransactionType.Oper_Delete:
                        result = !string.IsNullOrEmpty(locRel.GUID) && fCore.ExistsRecord<LocationRelation>(locRel.GUID);
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
