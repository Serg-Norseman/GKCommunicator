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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GKNet;
using GKNet.Core;

namespace GKCommunicatorApp
{
    public partial class ChatForm : Form, IChatForm
    {
        private delegate void NoArgDelegate();

        private string fMemberName;
        private IChatCore fCore;

        public ChatForm()
        {
            InitializeComponent();

            Closing += new CancelEventHandler(WindowMain_Closing);
            txtMemberName.Focus();

            fCore = new ChatDHTCP(this);
        }

        void WindowMain_Closing(object sender, CancelEventArgs e)
        {
            fCore.Disconnect();
        }

        private void AddChatText(string text)
        {
            lstChatMsgs.AppendText(text + "\r\n");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            lblConnectionStatus.Visible = true;

            fCore.MemberName = fMemberName;

            // join the P2P mesh from a worker thread
            NoArgDelegate executor = new NoArgDelegate(fCore.Connect);
            executor.BeginInvoke(null, null);
        }

        private void btnSendToAll_Click(object sender, EventArgs e)
        {
            var msgText = txtChatMsg.Text;

            if (!String.IsNullOrEmpty(msgText)) {
                fCore.SendToAll(msgText);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var peerItem = (lstMembers.SelectedItem as Peer);
            var msgText = txtChatMsg.Text;

            if ((!String.IsNullOrEmpty(msgText)) && (peerItem != null)) {
                fCore.Send(peerItem, msgText);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        private void miDHTLog_Click(object sender, EventArgs e)
        {
            LoadExtFile("./dht.log");
        }

        private void miSysInfo_Click(object sender, EventArgs e)
        {
            using (var dlgSysInfo = new SysInfoWin()) {
                dlgSysInfo.ShowDialog();
            }
        }

        private void miExternalIP_Click(object sender, EventArgs e)
        {
            LoadExtFile("https://2ip.ru/");
        }

        #region IChatForm members

        void IChatForm.OnPeersListChanged()
        {
            Invoke((MethodInvoker)delegate {
                lstMembers.BeginUpdate();
                lstMembers.Items.Clear();
                foreach (var peer in fCore.Peers) {
                    lstMembers.Items.Add(peer);
                }
                lstMembers.EndUpdate();
            });
        }

        void IChatForm.OnMessageReceived(Peer sender, string message)
        {
            Invoke((MethodInvoker)delegate {
                AddChatText(message);
            });
        }

        void IChatForm.OnJoin(Peer member)
        {
            Invoke((MethodInvoker)delegate {
                //AddChatText(member + " joined the chatroom.");
            });
        }

        void IChatForm.OnLeave(Peer member)
        {
            Invoke((MethodInvoker)delegate {
                //AddChatText(member + " left the chatroom.");
            });
        }

        #endregion

        #region External functions

        public static void LoadExtFile(string fileName)
        {
#if !CI_MODE
            if (File.Exists(fileName)) {
                Process.Start(new ProcessStartInfo("file://" + fileName) { UseShellExecute = true });
            } else {
                Process.Start(fileName);
            }
#endif
        }

        #endregion
    }
}
