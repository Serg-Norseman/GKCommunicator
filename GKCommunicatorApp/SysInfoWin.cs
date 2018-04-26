﻿/*
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

namespace GKCommunicatorApp
{
    public partial class SysInfoWin : Form
    {
        public SysInfoWin()
        {
            InitializeComponent();
        }

        private void SysInfoWin_Load(object sender, EventArgs e)
        {
            textBox1.Text += "UserName: " + SysHelper.GetUserName() + "\r\n";
            textBox1.Text += "UserCountry: " + SysHelper.GetUserCountry() + "\r\n";
            textBox1.Text += "TimeZone: " + SysHelper.GetTimeZone() + "\r\n";
            textBox1.Text += "Languages: " + SysHelper.GetLanguages() + "\r\n";
        }
    }
}
