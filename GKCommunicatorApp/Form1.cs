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
using System.Text;
using System.Windows.Forms;
using GKNet.Protocol;
using GKNet.TCP;

namespace GKSimpleChat
{
    public partial class Form1 : Form
    {
        private TCPDuplexClient fClient;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            fClient = new TCPDuplexClient();
            fClient.DataReceive += RaiseDataReceive;
            fClient.Start(int.Parse(txtLocalPort.Text));

            txtLocalPort.Enabled = false;
            btnConnect.Enabled = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //var endPoint = new IPEndPoint(IPAddress.Parse(txtRemoteAddress.Text), int.Parse(txtRemotePort.Text));
            //fClient.Send(endPoint, txtMsg.Text);
            SendHandshakeQuery();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (fClient != null) {
                fClient.Disconnect();
            }
        }

        public void RaiseDataReceive(object sender, DataReceiveEventArgs e)
        {
            Invoke((MethodInvoker)delegate {
                txtLog.Text += Encoding.UTF8.GetString(e.Data) + "\r\n";
            });
        }

        #region Protocol features

        private void SendHandshakeQuery()
        {
            var data = ProtocolHelper.CreateHandshakeQuery();
            var endPoint = new IPEndPoint(IPAddress.Parse(txtRemoteAddress.Text), int.Parse(txtRemotePort.Text));
            var conn = fClient.GetConnection(endPoint);
            conn.Send(data);
        }

        #endregion
    }
}
