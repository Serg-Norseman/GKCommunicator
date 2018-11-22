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
using System.Net.Sockets;
using System.Windows.Forms;
using GKNet;
using LumiSoft.Net.STUN.Client;

namespace GKCommunicatorApp
{
    public partial class SysInfoWin : Form
    {
        private ChatForm fChatForm;

        public SysInfoWin() : this(null)
        {
        }

        public SysInfoWin(ChatForm chatForm)
        {
            InitializeComponent();
            fChatForm = chatForm;
        }

        private void SysInfoWin_Load(object sender, EventArgs e)
        {
            var peerInfo = new PeerProfile();
            peerInfo.ResetSystem();

            textBox1.Text += "UserName: " + peerInfo.UserName + "\r\n";
            textBox1.Text += "UserCountry: " + peerInfo.Country + "\r\n";
            textBox1.Text += "TimeZone: " + peerInfo.TimeZone + "\r\n";
            textBox1.Text += "Languages: " + peerInfo.Languages + "\r\n\r\n\r\n";

            string server = "stun.ekiga.net";

            this.Cursor = Cursors.WaitCursor;
            try {
                if (string.IsNullOrEmpty(server)) {
                    MessageBox.Show(this, "Please specify STUN server!", "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                /*Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));*/
                Socket socket = fChatForm.Core.DHTClient.Socket;

                STUN_Result result = STUN_Client.Query(server, 3478, socket);
                textBox1.Text += "NET type: " + result.NetType.ToString() + "\r\n";
                textBox1.Text += "Local end point: " + socket.LocalEndPoint.ToString() + "\r\n";
                if (result.NetType != STUN_NetType.UdpBlocked) {
                    textBox1.Text += "Public end point: " + result.PublicEndPoint.ToString() + "\r\n";
                } else {
                    textBox1.Text += "Public end point: -\r\n";
                }
            } catch (Exception x) {
                MessageBox.Show(this, "Error: " + x.ToString(), "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
