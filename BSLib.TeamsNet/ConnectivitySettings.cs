/*
 *  "BSLib.TeamsNet", the serverless peer-to-peer network library.
 *  Copyright (C) 2018-2025 by Sergey V. Zhdanovskih.
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

namespace BSLib.TeamsNet
{
    public enum ProxyType
    {
        None,
        Socks4,
        Socks5
    }

    public class ConnectivitySettings
    {
        private string fProxyHost = string.Empty;
        private int fProxyPort;
        private string fProxyUsername = string.Empty;
        private string fProxyPassword = string.Empty;
        private ProxyType fProxyType = ProxyType.None;

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


        public ConnectivitySettings()
        {
        }

        public ConnectivitySettings(string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, ProxyType proxyType)
        {
            fProxyHost = proxyHost;
            fProxyPort = proxyPort;
            fProxyUsername = proxyUsername;
            fProxyPassword = proxyPassword;
            fProxyType = proxyType;
        }

        public override string ToString()
        {
            return "{ProxyHost=" + fProxyHost + ", ProxyPort=" + fProxyPort + "}";
        }
    }
}
