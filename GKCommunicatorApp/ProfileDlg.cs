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
using System.Windows.Forms;
using GKNet;
using LumiSoft.Net.STUN.Client;

namespace GKCommunicatorApp
{
    public partial class ProfileDlg : Form
    {
        private ICommunicatorCore fCore;
        private PeerProfile fProfile;

        public ProfileDlg() : this(null, null)
        {
        }

        public ProfileDlg(ICommunicatorCore core, PeerProfile profile)
        {
            InitializeComponent();
            fCore = core;
            fProfile = profile;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            txtUserName.Text = fProfile.UserName;
            txtCountry.Text = fProfile.Country;
            txtTimeZone.Text = fProfile.TimeZone;
            txtLanguages.Text = fProfile.Languages;

            var userProfile = fProfile as UserProfile;
            if (userProfile != null) {
                chkCountryVisible.Checked = userProfile.IsCountryVisible;
                chkTimeZoneVisible.Checked = userProfile.IsTimeZoneVisible;
                chkLanguagesVisible.Checked = userProfile.IsLanguagesVisible;

                var stunInfo = fCore.STUNInfo;
                AddProperty("NET type", stunInfo.NetType.ToString());
                AddProperty("Local end point", fCore.DHTClient.Socket.LocalEndPoint.ToString());
                if (stunInfo.NetType != STUN_NetType.UdpBlocked) {
                    AddProperty("Public end point", stunInfo.PublicEndPoint.ToString());
                } else {
                    AddProperty("Public end point", "-");
                }
            } else {
                chkCountryVisible.Visible = false;
                chkTimeZoneVisible.Visible = false;
                chkLanguagesVisible.Visible = false;

                // invisible is not implemented
                tabControl.TabPages.Remove(tabSysInfo);
            }
        }

        private void AddProperty(string propName, string value)
        {
            var listItem = lvSysInfo.Items.Add(propName);
            listItem.SubItems.Add(value);
        }
    }
}
