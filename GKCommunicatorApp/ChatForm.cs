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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GKNet;
using GKNet.Logging;

namespace GKCommunicatorApp
{
    public partial class ChatForm : Form, IChatForm
    {
        private delegate void NoArgDelegate();

        private readonly ICommunicatorCore fCore;
        private readonly ILogger fLogger;

        private bool fInitialized;


        public ICommunicatorCore Core
        {
            get { return fCore; }
        }


        public ChatForm()
        {
            InitializeComponent();

            Closing += ChatForm_Closing;

            if (File.Exists(ProtocolHelper.LOG_FILE)) {
                File.Delete(ProtocolHelper.LOG_FILE);
            }

            fInitialized = false;
            lblConnectionStatus.Text = "Network initialization...";

            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "ChatForm");
            fCore = new CommunicatorCore(this);

            UpdateStatus();
        }

        #region Private methods

        private void Connect()
        {
            lblConnectionStatus.Text = "Attemping to connect. Please standby.";

            // join the P2P network from a worker thread
            NoArgDelegate executor = new NoArgDelegate(fCore.Connect);
            executor.BeginInvoke(null, null);
        }

        private void Disconnect()
        {
            fCore.Disconnect();

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            miConnect.Enabled = fInitialized && !fCore.IsConnected;
            miDisconnect.Enabled = fInitialized && fCore.IsConnected;

            tbConnect.Enabled = fInitialized && !fCore.IsConnected;
            tbDisconnect.Enabled = fInitialized && fCore.IsConnected;

            btnSend.Enabled = fInitialized && fCore.IsConnected;
            btnSendToAll.Enabled = fInitialized && fCore.IsConnected;
        }

        private void AddTextChunk(string text, Color color)
        {
            int selStart = lstChatMsgs.Text.Length - 1;
            lstChatMsgs.AppendText(text);
            int selEnd = lstChatMsgs.Text.Length - 1;

            lstChatMsgs.SelectionStart = selStart;
            lstChatMsgs.SelectionLength = selEnd - selStart + 1;
            lstChatMsgs.SelectionColor = color;
        }

        private void AddChatText(string text, Color color)
        {
            lstChatMsgs.AppendText("\r\n");
            AddTextChunk(DateTime.Now.ToString(), Color.Red);
            lstChatMsgs.AppendText("\r\n");
            AddTextChunk(text, Color.Navy);
            lstChatMsgs.ScrollToCaret();
        }

        private void ShowProfile(PeerProfile profile)
        {
            using (var dlg = new ProfileDlg(fCore, profile)) {
                dlg.ShowDialog();
            }
        }

        #endregion

        #region Event handlers

        private void ChatForm_Closing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        private void btnSendToAll_Click(object sender, EventArgs e)
        {
            var msgText = txtChatMsg.Text;

            if (!string.IsNullOrEmpty(msgText)) {
                fCore.SendToAll(msgText);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (lstMembers.SelectedItems.Count == 0) return;

            var peerItem = (lstMembers.SelectedItems[0].Tag as Peer);
            var msgText = txtChatMsg.Text;

            if ((!string.IsNullOrEmpty(msgText)) && (peerItem != null)) {
                fCore.Send(peerItem, msgText);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        private Peer GetSelectedPeer()
        {
            if (lstMembers.SelectedItems.Count == 0) return null;

            return (lstMembers.SelectedItems[0].Tag as Peer);
        }

        private void miDHTLog_Click(object sender, EventArgs e)
        {
            NetHelper.LoadExtFile(Path.Combine(NetHelper.GetAppPath(), ProtocolHelper.LOG_FILE));
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void miConnect_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void miDisconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void miProfile_Click(object sender, EventArgs e)
        {
            ShowProfile(fCore.Profile);
        }

        private void miPeerProfile_Click(object sender, EventArgs e)
        {
            var peer = GetSelectedPeer();
            if (peer != null) {
                ShowProfile(peer.Profile);
            }
        }

        #endregion

        #region IChatForm members

        void IChatForm.OnInitialized()
        {
            Invoke((MethodInvoker)delegate {
                fInitialized = true;
                lblConnectionStatus.Text = "Network initialized. You can start the connection.";
                UpdateStatus();
            });
        }

        void IChatForm.OnPeersListChanged()
        {
            Invoke((MethodInvoker)delegate {
                int membersNum = 0;

                lstMembers.BeginUpdate();
                lstMembers.Items.Clear();
                foreach (var peer in fCore.Peers) {
                    if (!peer.IsLocal) {
                        membersNum += 1;
                    }
                    var listItem = lstMembers.Items.Add(peer.ToString());
                    listItem.Tag = peer;
                }
                lstMembers.EndUpdate();

                lblConnectionStatus.Text = string.Format("Members online: {0} ({1})", membersNum, fCore.Peers.Count);

                UpdateStatus();
            });
        }

        void IChatForm.OnMessageReceived(Peer sender, string message)
        {
            Invoke((MethodInvoker)delegate {
                AddChatText(message, Color.Black);

                UpdateStatus();
            });
        }

        void IChatForm.OnJoin(Peer member)
        {
            Invoke((MethodInvoker)delegate {
                lblConnectionStatus.Text = "Network connection established.";
                //AddChatText(member + " joined the chatroom.");

                UpdateStatus();
            });
        }

        void IChatForm.OnLeave(Peer member)
        {
            Invoke((MethodInvoker)delegate {
                //AddChatText(member + " left the chatroom.");

                UpdateStatus();
            });
        }

        #endregion
    }
}
