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
using System.Windows.Forms;
using GKNet.Core;
using GKNetPNRP;

namespace GKSimpleChat
{
    public partial class ChatForm : Form, IChatForm
    {
        private delegate void NoArgDelegate();

        private string fMemberName;
        private IChatCore fCore;

        public ChatForm()
        {
            InitializeComponent();
            fCore = new ChatPNRP(this);
            fCore.Online += ostat_Online;
            fCore.Offline += ostat_Offline;
            Closing += new CancelEventHandler(WindowMain_Closing);
            txtMemberName.Focus();
        }

        void WindowMain_Closing(object sender, CancelEventArgs e)
        {
            fCore.Disconnect();
        }

        private void AddChatText(string text)
        {
            lstChatMsgs.AppendText(text + "\r\n");
        }

        #region IChatForm members

        public void OnJoin(string member)
        {
            Invoke(
                (MethodInvoker)delegate {
                    AddChatText(member + " joined the chatroom.");

                    // this will retrieve any new members that have joined before the current user
                    fCore.SendSynchronizeMemberList(fMemberName);
                });
        }

        public void OnChat(string member, string message)
        {
            Invoke(
                (MethodInvoker)delegate {
                    AddChatText(member + " says: " + message);
                });
        }

        public void OnWhisper(string member, string memberTo, string message)
        {
            Invoke(
                (MethodInvoker)delegate {
                    //this is a rudimentary form of whisper and is flawed so should NOT be used in production.
                    //this method simply checks the sender and to address and only displays the message
                    //if it belongs to this member, however! - if there are N members with the same name
                    //they will all be whispered to from the sender since the message is broadcast to everybody.
                    //the correct way to implement this would
                    //be to instead retrieve the peer name from the mesh for the member you want to whisper to
                    //and send the message directly to that peer node via the mesh.  i may update the code to do 
                    //that in the future but for now i'm too busy with other things to mess with it hence it's
                    //left as an exercise for the reader.
                    if (fMemberName.Equals(member) || fMemberName.Equals(memberTo)) {
                        AddChatText(member + " whispers: " + message);
                    }
                });
        }

        public void OnLeave(string member)
        {
            Invoke(
                (MethodInvoker)delegate {
                    AddChatText(member + " left the chatroom.");
                });
        }

        public void OnSynchronizeMemberList(string member)
        {
            Invoke(
                (MethodInvoker)delegate {
                    //as member names come in we simply disregard duplicates and 
                    //add them to the member list, this way we can retrieve a list
                    //of members already in the chatroom when we enter at any time.

                    //again, since this is just an example this is the simplified
                    //way to do things.  the correct way would be to retrieve a list
                    //of peernames and retrieve the metadata from each one which would
                    //tell us what the member name is and add it.  we would want to check
                    //this list when we join the mesh to make sure our member name doesn't 
                    //conflict with someone else
                    if (!lstMembers.Items.Contains(member)) {
                        lstMembers.Items.Add(member);
                    }
                });
        }

        void ostat_Offline(object sender, EventArgs e)
        {
            // implement later
        }

        void ostat_Online(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate {
                fMemberName = txtMemberName.Text;
                lblConnectionStatus.Text = "Welcome to the chat room!";
                fCore.SendJoin(fMemberName);
            });
        }

        #endregion

        private void btnConnect_Click(object sender, EventArgs e)
        {
            lblConnectionStatus.Visible = true;
            // join the P2P mesh from a worker thread
            fCore.MemberName = fMemberName;
            NoArgDelegate executor = new NoArgDelegate(fCore.Connect);
            executor.BeginInvoke(null, null);
        }

        private void btnSendToAll_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtChatMsg.Text)) {
                fCore.SendChat(fMemberName, txtChatMsg.Text);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if ((!String.IsNullOrEmpty(txtChatMsg.Text)) && (lstMembers.SelectedIndex >= 0)) {
                fCore.SendWhisper(fMemberName, lstMembers.SelectedItem.ToString(), txtChatMsg.Text);
                txtChatMsg.Clear();
                txtChatMsg.Focus();
            }
        }

        private void miDHTLog_Click(object sender, EventArgs e)
        {
            using (var dlg = new DHTLogDlg()) {
                dlg.ShowDialog();
            }
        }

        private void miSysInfo_Click(object sender, EventArgs e)
        {
            using (var dlgSysInfo = new SysInfoWin()) {
                dlgSysInfo.ShowDialog();
            }
        }
    }
}
