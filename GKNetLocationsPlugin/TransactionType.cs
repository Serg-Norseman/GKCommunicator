/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

namespace GKNetLocationsPlugin
{
    public static class TransactionType
    {
        public const string Unknown = "unknown";

        public const string Profile = "profile";

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
