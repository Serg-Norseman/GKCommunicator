/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;

namespace GKLocations.Common
{
    public enum TransactionType
    {
        Unknown,

        Profile,

        AddLocation,
        UpdateLocation,
        DeleteLocation,

        AddLocationName,
        UpdateLocationName,
        DeleteLocationName,

        AddLocationRelation,
        UpdateLocationRelation,
        DeleteLocationRelation,
    }

    public interface ITransaction
    {
        DateTime Timestamp { get; set; }

        TransactionType Type { get; set; }

        string Data { get; set; }
    }
}
