/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2023 by Sergey V. Zhdanovskih.
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
using System.IO;
using System.Threading;
using Eto.Forms;
using Eto.Serialization.Xaml;
using GKNet;
using GKNet.Logging;
using LumiSoft.Net.STUN.Client;

namespace GKNetUI
{
    public partial class ChatForm : Form, IChatForm
    {
        #region Design components
#pragma warning disable CS0169, CS0649, IDE0044, IDE0051

        private MenuBar menuStrip1;
        private ToolBar toolStrip1;
        private ButtonMenuItem miConnection;
        private ButtonMenuItem miService;
        private ButtonMenuItem miHelp;
        private ButtonMenuItem miView;
        private ButtonMenuItem miDHTLog;
        private Splitter splitContainer1;
        private PeersList lstMembers;
        private Splitter splitContainer2;
        private TextArea txtChatMsg;
        private StackLayout flowLayoutPanel1;
        private Button btnSendToAll;
        private Button btnSend;
        private MessagesList lstChatMsgs;
        private ContextMenu contextMenuStrip1;
        private ButtonMenuItem miConnect;
        private ButtonMenuItem miDisconnect;
        private ButtonMenuItem miProfile;
        private ButtonMenuItem miExit;
        private ButtonToolItem tbConnect;
        private ButtonToolItem tbDisconnect;
        private ButtonToolItem tbProfile;
        private ButtonMenuItem miPeerProfile;
        private ButtonMenuItem miAddPeer;
        private ButtonMenuItem miPeersList;
        private RadioMenuItem miAllPeers;
        private RadioMenuItem miOnlyFriends;
        private CheckMenuItem miConnectionInfo;
        private ButtonMenuItem miContents;
        private ButtonMenuItem miAbout;
        private DropDownToolItem tbPresenceStatus;
        //private ContextMenu menuPresenceStatuses;
        private ButtonToolItem tbSendInvitation;
        private ButtonToolItem tbAcceptInvitation;
        private TabControl tabControl1;
        private TabPage tabChat;
        private TableLayout statusStrip1;
        private Label lblConnectionStatus;
        private Label lblTicks;

#pragma warning restore CS0169, CS0649, IDE0044, IDE0051
        #endregion


        private const string TICK_SYMS = @"|/-\";

        private readonly CommunicatorCore fCore;
        private readonly string fLocalId;
        private readonly ILogger fLogger;

        private bool fInitialized;
        private int fTick;


        public ICommunicatorCore Core
        {
            get { return fCore; }
        }


        public ChatForm()
        {
            XamlReader.Load(this);

            UIHelper.InitResources();

            Closing += ChatForm_Closing;

            RadioMenuItem controller = null;
            for (var ps = PresenceStatus.Offline; ps <= PresenceStatus.Invisible; ps++) {
                var menuItem = UIHelper.AddToolStripItem(tbPresenceStatus, ref controller, ps.ToString(), ps, miPresenceStatus_Click);
                //menuItem.Image = UIHelper.GetPresenceStatusImage(ps);
            }

            if (File.Exists(ProtocolHelper.LOG_FILE)) {
                File.Delete(ProtocolHelper.LOG_FILE);
            }
            fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "ChatForm");

            fTick = -1;
            fInitialized = false;
            fCore = new CommunicatorCore(this);
            fLocalId = fCore.LocalPeer.ID.ToString();

            lstChatMsgs.Core = fCore;
            lstChatMsgs.LocalId = fLocalId;

            miConnectionInfo.Checked = fCore.ShowConnectionInfo;
            UpdateShowConnectionInfo();

            lblConnectionStatus.Text = "Network initialization...";
            UIHelper.SetMenuItemTag(tbPresenceStatus, fCore.LocalPeer.Presence);
            UpdateStatus();

            ProcessPlugins();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                UIHelper.DoneResources();
            }
            base.Dispose(disposing);
        }

        private void ProcessPlugins()
        {
            foreach (var dataPlugin in fCore.DataPlugins) {
                TabPage tabPage = new TabPage(dataPlugin.DisplayName);
                tabControl1.Pages.Add(tabPage);

                var editor = Activator.CreateInstance(dataPlugin.EditorType) as Control;
                tabPage.Content = editor;
                ((IDataEditor)editor).Init(dataPlugin);
            }
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

        private void PrintMessage(GKNet.Message msg, bool scrollToBottom)
        {
            lstChatMsgs.AddMessage(msg, scrollToBottom);

            /*if (scrollToBottom) {
                lstChatMsgs.ScrollToBottom();
            }*/
        }

        private void ShowProfile(PeerProfile profile)
        {
            using (var dlg = new ProfileDlg(fCore, profile)) {
                dlg.ShowModal(this);
            }
        }

        private Peer GetSelectedPeer()
        {
            if (lstMembers.SelectedValue == null)
                return null;

            return (lstMembers.SelectedValue as Peer);
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

        private void InvokeX(Action action)
        {
            try {
                Application.Instance.Invoke(action);
            } catch {
                // dummy
            }
        }

        private void Authenticate()
        {
            if (fCore.Profile.IsIdentified) {
                string password = string.Empty;
                if (InputDlg.QueryPassword(this, CommunicatorCore.APP_NAME, "Password", ref password)) {
                    if (!fCore.Authenticate(password)) {
                        MessageBox.Show("Authentication failed", CommunicatorCore.APP_NAME, MessageBoxType.Error);
                        Close();
                    }
                } else {
                    Close();
                }
            } else {
                ShowProfile(fCore.Profile);
            }
        }

        private void SendMessage(Peer selectedPeer)
        {
            try {
                var msgText = txtChatMsg.Text;
                if (string.IsNullOrEmpty(msgText))
                    return;

                txtChatMsg.Text = ""; //Clear();
                txtChatMsg.Focus();

                if (selectedPeer != null && !selectedPeer.IsLocal) {
                    var msg = fCore.SendMessage(selectedPeer, msgText);
                    PrintMessage(msg, true);
                } else {
                    foreach (var peer in fCore.Peers) {
                        if (peer.IsLocal)
                            continue;

                        var msg = fCore.SendMessage(peer, msgText);
                        if (peer == selectedPeer) {
                            PrintMessage(msg, true);
                        }
                    }
                }
            } catch (Exception ex) {
                fLogger.WriteError("ChatForm.SendMessage()", ex);
            }
        }

        private void UpdateShowConnectionInfo()
        {
            lstMembers.ShowConnectionInfo = fCore.ShowConnectionInfo;
            lstMembers.Invalidate();
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
            SendMessage(null);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //PrintMessage(new Message(DateTime.Now, "sample", "noname", "noname"), false);

            SendMessage(GetSelectedPeer());
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

        private void miMyProfile_Click(object sender, EventArgs e)
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

        private void miAddPeer_Click(object sender, EventArgs e)
        {
            string endpoint = string.Empty;
            if (InputDlg.QueryText(this, CommunicatorCore.APP_NAME, "Peer endpoint", ref endpoint)) {
                if (Utilities.IsValidIpAddress(endpoint)) {
                    var peerEndPoint = Utilities.ParseIPEndPoint(endpoint);
                    fCore.UpdatePeer(peerEndPoint);
                    ((IChatForm)this).OnPeersListChanged();
                }
            }
        }

        private void miPresenceStatus_Click(object sender, EventArgs e)
        {
            var ps = UIHelper.GetMenuItemTag<PresenceStatus>(tbPresenceStatus, sender);
            tbPresenceStatus.Image = UIHelper.GetPresenceStatusImage(ps);
            tbPresenceStatus.Text = ps.ToString();
            fCore.LocalPeer.Presence = ps;
            fCore.Database.SavePresence(ps);
        }

        private void lstMembers_SelectedValueChanged(object sender, EventArgs e)
        {
            lstChatMsgs.Clear();

            var selectedPeer = GetSelectedPeer();
            if (selectedPeer == null || selectedPeer.IsLocal || selectedPeer.ID == null) {
                return;
            }

            var messages = fCore.LoadMessages(selectedPeer);

            foreach (var msg in messages) {
                PrintMessage(msg, false);
            }

            lstChatMsgs.ScrollToBottom();
        }

        private void miConnectionInfo_CheckedChanged(object sender, EventArgs e)
        {
            fCore.ShowConnectionInfo = miConnectionInfo.Checked;
            UpdateShowConnectionInfo();
        }

        private void tbSendInvitation_Click(object sender, EventArgs e)
        {
            fCore.SendInvitation();
        }

        private void tbAcceptInvitation_Click(object sender, EventArgs e)
        {
            string nodeId = string.Empty;
            if (InputDlg.QueryText(this, CommunicatorCore.APP_NAME, "Friend's node id received by mail", ref nodeId)) {
                fCore.AcceptInvitation(nodeId);
            }
        }

        #endregion

        #region IChatForm members

        void IChatForm.OnInitialized()
        {
            InvokeX(delegate {
                ((IChatForm)this).OnPeersListChanged();

                if (fCore.STUNInfo.NetType == STUN_NetType.UdpBlocked) {
                    MessageBox.Show("STUN status: UDP blocked", CommunicatorCore.APP_NAME, MessageBoxType.Error);
                } else {
                    fInitialized = true;
                    lblConnectionStatus.Text = "Network initialized. You can start the connection.";
                    UpdateStatus();
                }

                Authenticate();
            });
        }

        void IChatForm.OnPeersListChanged()
        {
            InvokeX(delegate {
                int membersNum = 0;

                var selItem = lstMembers.SelectedValue;
                //lstMembers.BeginUpdate();
                lstMembers.Items.Clear();
                foreach (var peer in fCore.Peers) {
                    if (!peer.IsLocal /*&& peer.State >= PeerState.Unchecked*/) {
                        membersNum += 1;
                        lstMembers.Items.Add(peer);
                    }
                }
                //lstMembers.EndUpdate();
                if (fCore.Peers.Contains(selItem)) {
                    lstMembers.SelectedValue = selItem;
                }

                lblConnectionStatus.Text = string.Format("Members online: {0} ({1} total)", membersNum, fCore.Peers.Count);

                UpdateStatus();
            });
        }

        void IChatForm.OnMessageReceived(Peer sender, GKNet.Message message)
        {
            InvokeX(delegate {
                PrintMessage(message, true);

                UpdateStatus();
            });
        }

        #endregion
    }
}
