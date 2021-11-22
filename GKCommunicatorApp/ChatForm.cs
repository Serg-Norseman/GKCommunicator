/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
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
using System.Threading;
using System.Windows.Forms;
using GKNet;
using GKNet.Logging;
using LumiSoft.Net.STUN.Client;

namespace GKCommunicatorApp
{
    public partial class ChatForm : Form, IChatForm
    {
        private const string TICK_SYMS = @"|/-\";

        private readonly CommunicatorCore fCore;
        private readonly ILogger fLogger;

        private bool fInitialized;
        private int fTick;


        public ICommunicatorCore Core
        {
            get { return fCore; }
        }


        public ChatForm()
        {
            InitializeComponent();

            lstMembers.Columns.Add("Peer", 400);

            Closing += ChatForm_Closing;

            if (File.Exists(ProtocolHelper.LOG_FILE)) {
                File.Delete(ProtocolHelper.LOG_FILE);
            }

            fTick = -1;
            fInitialized = false;
            lblConnectionStatus.Text = "Network initialization...";

            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "ChatForm");
            fCore = new CommunicatorCore(this);

            UpdateStatus();
        }

        #region Private methods

        private void Connect()
        {
            lblConnectionStatus.Text = "Searching for members. Please wait";

            // join the P2P network from a worker thread
            //var executor = new MethodInvoker(fCore.Connect);
            //executor.BeginInvoke(null, null);
            fCore.Connect();

            StartConnectionTicks();
        }

        private void Disconnect()
        {
            fCore.Disconnect();

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            bool disconnected = (fCore.ConnectionState == ConnectionState.Disconnected);

            miConnect.Enabled = fInitialized && disconnected;
            tbConnect.Enabled = fInitialized && disconnected;

            miDisconnect.Enabled = fInitialized && !disconnected;
            tbDisconnect.Enabled = fInitialized && !disconnected;

            bool connected = (fCore.ConnectionState == ConnectionState.Connected);

            btnSend.Enabled = fInitialized && connected;
            btnSendToAll.Enabled = fInitialized && connected;
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

        private void AddChatText(Peer sender, string text, Color headerColor, Color textColor)
        {
            string senderText = (sender == null) ? " [local]" : string.Format(" [{0}]", sender.EndPoint);
            lstChatMsgs.AppendText("\r\n");
            AddTextChunk(DateTime.Now.ToString() + senderText, headerColor);
            lstChatMsgs.AppendText("\r\n");
            AddTextChunk(text, textColor);
            lstChatMsgs.AppendText("\r\n");
            lstChatMsgs.ScrollToCaret();
        }

        private void ShowProfile(PeerProfile profile)
        {
            using (var dlg = new ProfileDlg(fCore, profile)) {
                dlg.ShowDialog();
            }
        }

        private Peer GetSelectedPeer()
        {
            if (lstMembers.SelectedItems.Count == 0)
                return null;

            return (lstMembers.SelectedItems[0].Tag as Peer);
        }

        private void StartConnectionTicks()
        {
            new Thread(() => {
                while (fCore.ConnectionState != ConnectionState.Disconnected) {
                    InvokeX(() => {
                        if (fCore.ConnectionState == ConnectionState.Connection) {
                            fTick = (fTick >= 3) ? 0 : fTick + 1;
                            lblTicks.Text = "" + TICK_SYMS[fTick];
                        } else {
                            lblTicks.Text = "*";
                        }

                        UpdateStatus();
                    });

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        private void InvokeX(MethodInvoker invoker)
        {
            if (InvokeRequired) {
                Invoke(invoker);
            } else {
                invoker();
            }
        }

        #endregion

        #region Event handlers

        private void ChatForm_Load(object sender, EventArgs e)
        {
            ((IChatForm)this).OnInitialized();
        }

        private void ChatForm_Closing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        private void btnSendToAll_Click(object sender, EventArgs e)
        {
            var msgText = txtChatMsg.Text;

            if (!string.IsNullOrEmpty(msgText)) {
                AddChatText(null, msgText, Color.Navy, Color.Black);
                txtChatMsg.Clear();
                txtChatMsg.Focus();

                fCore.SendToAll(msgText);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var peerItem = GetSelectedPeer();
            var msgText = txtChatMsg.Text;

            if ((!string.IsNullOrEmpty(msgText)) && (peerItem != null && !peerItem.IsLocal)) {
                AddChatText(null, msgText, Color.Green, Color.Black);
                txtChatMsg.Clear();
                txtChatMsg.Focus();

                fCore.Send(peerItem, msgText);
            }
        }

        private void miDHTLog_Click(object sender, EventArgs e)
        {
            Utilities.LoadExtFile(Path.Combine(Utilities.GetAppPath(), ProtocolHelper.LOG_FILE));
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
            MethodInvoker invoker = delegate () {
                ((IChatForm)this).OnPeersListChanged();

                if (fCore.STUNInfo.NetType == STUN_NetType.UdpBlocked) {
                    MessageBox.Show("STUN status: UDP blocked", "GKCommunicator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else {
                    fInitialized = true;
                    lblConnectionStatus.Text = "Network initialized. You can start the connection.";
                    UpdateStatus();
                }

                string password = string.Empty;
                if (InputDlg.QueryPassword("GKCommunicator", "Password", ref password)) {
                    if (!fCore.Profile.Authentication(password)) {
                        MessageBox.Show("Authentication failed", "GKCommunicator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            if (InvokeRequired) {
                Invoke(invoker);
            } else {
                invoker();
            }
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
                AddChatText(sender, message, Color.Red, Color.Black);

                UpdateStatus();
            });
        }

        void IChatForm.OnJoin(Peer member)
        {
            Invoke((MethodInvoker)delegate {
                AddChatText(member, member + " joined the chat.", Color.Maroon, Color.DarkMagenta);

                UpdateStatus();
            });
        }

        void IChatForm.OnLeave(Peer member)
        {
            Invoke((MethodInvoker)delegate {
                AddChatText(member, member + " left the chat.", Color.Maroon, Color.DarkMagenta);

                UpdateStatus();
            });
        }

        #endregion
    }
}
