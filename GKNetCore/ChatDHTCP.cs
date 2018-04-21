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
using GKNet.Core;

namespace GKNet
{
    public class ChatDHTCP : IChatCore
    {
        private IChatForm fForm;

        private string fMemberName;

        public event EventHandler Offline;
        public event EventHandler Online;
        public event OnSynchronizeMemberList OnSynchronizeMemberList;

        public string MemberName
        {
            get { return fMemberName; }
            set { fMemberName = value; }
        }

        public ChatDHTCP(IChatForm form)
        {
            fForm = form;
        }

        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        public void SendChat(string member, string message)
        {
        }

        public void SendWhisper(string Member, string MemberTo, string Message)
        {
        }

        public void SendJoin(string Member)
        {
        }

        public void SendLeave(string Member)
        {
        }

        public void SendSynchronizeMemberList(string Member)
        {
        }

        private void ostat_Offline(object sender, EventArgs e)
        {
            if (Offline != null) {
                Offline(this, e);
            }
        }

        private void ostat_Online(object sender, EventArgs e)
        {
            if (Online != null) {
                Online(this, e);
            }
        }
    }
}
