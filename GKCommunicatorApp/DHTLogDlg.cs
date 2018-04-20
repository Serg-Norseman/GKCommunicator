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

using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using GKNet.Core;
using GKNet.DHT;
using GKNet.Protocol;

namespace GKSimpleChat
{
    public partial class DHTLogDlg : Form, ILogger
    {
        private DHTClient fDHTClient;

        public DHTLogDlg()
        {
            InitializeComponent();

            int port = DHTClient.PublicDHTPort;
            fDHTClient = new DHTClient(IPAddress.Any, port, this);
            fDHTClient.PeersFound += delegate (object sender, PeersFoundEventArgs e) {
                WriteLog(string.Format("Found peers: {0}", e.Peers.Count));
                WriteLog(string.Format("Peers[0]: {0}", e.Peers[0].ToString()));
            };
        }

        private void DHTLogDlg_Load(object sender, System.EventArgs e)
        {
            var snkInfoHash = ProtocolHelper.CreateSignInfoKey();
            WriteLog("Search for: " + snkInfoHash.ToHexString());

            fDHTClient.Run();
            fDHTClient.JoinNetwork();
            fDHTClient.SearchNodes(snkInfoHash);
        }

        private void DHTLogDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            fDHTClient.StopSearch();
        }

        public void WriteLog(string str, bool display = true)
        {
            Invoke((MethodInvoker)delegate {
                rtbLog.AppendText(str + "\r\n");
            });

            var fswriter = new StreamWriter(new FileStream("./dht.log", FileMode.Append));
            fswriter.WriteLine(str);
            fswriter.Flush();
            fswriter.Close();
        }
    }
}
