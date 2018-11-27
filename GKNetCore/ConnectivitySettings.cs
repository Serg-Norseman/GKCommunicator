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
using System.Net;

namespace GKNet
{
    public enum ProxyType
    {
        None,
        Socks4,
        Socks5
    }

    public class ConnectivitySettings
    {
        private string fProxyHost = String.Empty;
        private int fProxyPort;
        private string fProxyUsername = String.Empty;
        private string fProxyPassword = String.Empty;
        private ProxyType fProxyType = ProxyType.None;
        private WebProxy fWebProxy;

        public string ProxyHost
        {
            get { return fProxyHost; }
            set { fProxyHost = value; }
        }

        public int ProxyPort
        {
            get { return fProxyPort; }
            set { fProxyPort = value; }
        }

        public string ProxyUsername
        {
            get { return fProxyUsername; }
            set { fProxyUsername = value; }
        }

        public string ProxyPassword
        {
            get { return fProxyPassword; }
            set { fProxyPassword = value; }
        }

        public ProxyType ProxyType
        {
            get { return fProxyType; }
            set { fProxyType = value; }
        }

        public WebProxy WebProxy
        {
            get { return fWebProxy; }
            set { fWebProxy = value; }
        }


        public ConnectivitySettings()
        {
        }

        public ConnectivitySettings(ConnectivitySettings x)
        {
            fProxyType = x.fProxyType;
            fProxyHost = x.fProxyHost;
            fProxyPort = x.fProxyPort;
            fProxyUsername = x.fProxyUsername;
            fProxyPassword = x.fProxyPassword;
            fWebProxy = x.fWebProxy;
        }

        public ConnectivitySettings(string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, ProxyType proxyType)
        {
            fProxyHost = proxyHost;
            fProxyPort = proxyPort;
            fProxyUsername = proxyUsername;
            fProxyPassword = proxyPassword;
            fProxyType = proxyType;
        }

        public ConnectivitySettings(string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, ProxyType proxyType, WebProxy webProxy)
        {
            fProxyHost = proxyHost;
            fProxyPort = proxyPort;
            fProxyUsername = proxyUsername;
            fProxyPassword = proxyPassword;
            fProxyType = proxyType;
            fWebProxy = webProxy;
        }

        public override string ToString()
        {
            return "{ProxyHost=" + ProxyHost + ", ProxyPort=" + ProxyPort + "}";
        }
    }
}
