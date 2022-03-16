/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using GKNet;
using GKNetLocationsPlugin.Editor;

namespace GKNetLocationsPlugin
{
    public class LocationsDataPlugin : DataPlugin
    {
        public override string DisplayName
        {
            get {
                return "Locations";
            }
        }

        public override Type EditorType
        {
            get {
                return typeof(LocationsControl);
            }
        }
    }
}
