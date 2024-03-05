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

using System;
using GKNet;
using GKNetLocationsPlugin.Editor;
using GKNetLocationsPlugin.Model;

namespace GKNetLocationsPlugin
{
    public class LocationsDataPlugin : DataPlugin
    {
        private ICommunicatorCore fHost;
        private GKLCore fCore;


        public GKLCore Core
        {
            get {
                return fCore;
            }
        }

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

        public ICommunicatorCore Host
        {
            get {
                return fHost;
            }
        }


        public LocationsDataPlugin()
        {
        }

        public override bool Startup(ICommunicatorCore host)
        {
            fHost = host;
            fCore = new GKLCore(fHost);
            fHost.BlockchainNode.RegisterSolver(new LocationTransactionSolver(fCore));
            fHost.BlockchainNode.RegisterSolver(new LocationNameTransactionSolver(fCore));
            fHost.BlockchainNode.RegisterSolver(new LocationNameTranslationTransactionSolver(fCore));
            fHost.BlockchainNode.RegisterSolver(new LocationRelationTransactionSolver(fCore));
            return true;
        }

        public override bool Shutdown()
        {
            return true;
        }
    }
}
