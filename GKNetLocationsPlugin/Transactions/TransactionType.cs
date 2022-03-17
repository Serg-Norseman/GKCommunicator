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

namespace GKNetLocationsPlugin.Transactions
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

        public const string LocationRelation_Create = "location_relation:create";
        public const string LocationRelation_Update = "location_relation:update";
        public const string LocationRelation_Delete = "location_relation:delete";
    }
}
