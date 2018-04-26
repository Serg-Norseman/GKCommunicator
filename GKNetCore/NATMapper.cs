/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
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
using GKNet.DHT;
using Mono.Nat;

namespace GKNet
{
    public static class NATMapper
    {
        private static ILogger fLogger;

        public static void CreateNATMapping(ILogger logger)
        {
            fLogger = logger;

            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;
            NatUtility.StartDiscovery();

            fLogger.WriteLog("NAT Discovery started");

            /*while (true) {
                Thread.Sleep(500000);
                NatUtility.StopDiscovery();
                NatUtility.StartDiscovery();
            }*/
        }

        private static void DeviceFound(object sender, DeviceEventArgs args)
        {
            try {
                INatDevice device = args.Device;

                fLogger.WriteLog("Device found");
                fLogger.WriteLog("Type: {0}", device.GetType().Name);
                fLogger.WriteLog("External IP: {0}", device.GetExternalIP());

                Mapping mapping = new Mapping(Mono.Nat.Protocol.Tcp, ProtocolHelper.PublicTCPPort, ProtocolHelper.PublicTCPPort);
                device.CreatePortMap(mapping);
                fLogger.WriteLog("Create Mapping: protocol={0}, public={1}, private={2}", mapping.Protocol, mapping.PublicPort, mapping.PrivatePort);

                mapping = new Mapping(Mono.Nat.Protocol.Udp, DHTClient.PublicDHTPort, DHTClient.PublicDHTPort);
                device.CreatePortMap(mapping);
                fLogger.WriteLog("Create Mapping: protocol={0}, public={1}, private={2}", mapping.Protocol, mapping.PublicPort, mapping.PrivatePort);

                try {
                    Mapping m = device.GetSpecificMapping(Mono.Nat.Protocol.Tcp, 6001);
                    fLogger.WriteLog("Specific Mapping: protocol={0}, public={1}, private={2}", m.Protocol, m.PublicPort, m.PrivatePort);
                } catch {
                    fLogger.WriteLog("Couldnt get specific mapping");
                }
                foreach (Mapping mp in device.GetAllMappings()) {
                    fLogger.WriteLog("Existing Mapping: protocol={0}, public={1}, private={2}", mp.Protocol, mp.PublicPort, mp.PrivatePort);
                    device.DeletePortMap(mp);
                }

                fLogger.WriteLog("Done...");
            } catch (Exception ex) {
                fLogger.WriteLog(ex.Message);
                fLogger.WriteLog(ex.StackTrace);
            }
        }

        private static void DeviceLost(object sender, DeviceEventArgs args)
        {
            fLogger.WriteLog("Device Lost");
            fLogger.WriteLog("Type: {0}", args.Device.GetType().Name);
        }
    }
}
