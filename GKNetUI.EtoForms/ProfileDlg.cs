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
using Eto.Forms;
using Eto.Serialization.Xaml;
using GKNet;

namespace GKNetUI
{
    public partial class ProfileDlg : Dialog
    {
        #region Design components
#pragma warning disable CS0169, CS0649, IDE0044, IDE0051

        private TabControl tabControl;
        private TabPage tabProfile;
        private Label lblLanguages;
        private Label lblTimeZone;
        private Label lblCountry;
        private Label lblUserName;
        private TextBox txtLanguages;
        private TextBox txtTimeZone;
        private TextBox txtCountry;
        private TextBox txtUserName;
        private CheckBox chkLanguagesVisible;
        private CheckBox chkTimeZoneVisible;
        private CheckBox chkCountryVisible;
        private Button btnSave;
        private TextBox txtEndPoint;
        private Label lblEndPoint;
        private CheckBox chkEmailVisible;
        private TextBox txtEmail;
        private Label lblEmail;

#pragma warning restore CS0169, CS0649, IDE0044, IDE0051
        #endregion


        private readonly ICommunicatorCore fCore;
        private readonly PeerProfile fProfile;

        public ProfileDlg() : this(null, null)
        {
        }

        public ProfileDlg(ICommunicatorCore core, PeerProfile profile)
        {
            XamlReader.Load(this);
            fCore = core;
            fProfile = profile;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            txtUserName.Text = fProfile.UserName;
            txtCountry.Text = fProfile.Country;
            txtTimeZone.Text = fProfile.TimeZone;
            txtLanguages.Text = fProfile.Languages;
            txtEmail.Text = fProfile.Email;

            var userProfile = fProfile as UserProfile;
            bool isUser = (userProfile != null);
            if (isUser) {
                txtEndPoint.Text = fCore.PublicEndPoint.ToString();

                chkCountryVisible.Checked = userProfile.IsCountryVisible;
                chkTimeZoneVisible.Checked = userProfile.IsTimeZoneVisible;
                chkLanguagesVisible.Checked = userProfile.IsLanguagesVisible;
                chkEmailVisible.Checked = userProfile.IsEmailVisible;
            } else {
                txtEndPoint.Text = string.Empty;

                chkCountryVisible.Visible = false;
                chkTimeZoneVisible.Visible = false;
                chkLanguagesVisible.Visible = false;
                chkEmailVisible.Visible = false;
            }

            txtUserName.ReadOnly = !isUser;
            txtCountry.ReadOnly = !isUser;
            txtTimeZone.ReadOnly = !isUser;
            txtLanguages.ReadOnly = !isUser;
            txtEmail.ReadOnly = !isUser;

            btnSave.Visible = isUser;
            btnSave.Enabled = isUser;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var userProfile = fProfile as UserProfile;

            string password = string.Empty;
            if (!InputDlg.QueryPassword(this, CommunicatorCore.APP_NAME, "Password", ref password)) {
                return;
            }
            fCore.Identify(userProfile, password);

            userProfile.UserName = txtUserName.Text;
            userProfile.Country = txtCountry.Text;
            userProfile.TimeZone = txtTimeZone.Text;
            userProfile.Languages = txtLanguages.Text;
            userProfile.Email = txtEmail.Text;

            userProfile.IsCountryVisible = chkCountryVisible.Checked.Value;
            userProfile.IsTimeZoneVisible = chkTimeZoneVisible.Checked.Value;
            userProfile.IsLanguagesVisible = chkLanguagesVisible.Checked.Value;
            userProfile.IsEmailVisible = chkEmailVisible.Checked.Value;

            fCore.Database.SaveProfile(userProfile);

            Close();
        }
    }
}
