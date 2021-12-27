/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2021 by Sergey V. Zhdanovskih.
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
using System.Reflection;
using BSLib.Design.Graphics;
using GKCore;
using GKCore.Interfaces;
using GKCore.Plugins;
using GKNetUI;

[assembly: AssemblyTitle("GKCommunicatorPlugin")]
[assembly: AssemblyDescription("GEDKeeper Communicator plugin")]
[assembly: AssemblyProduct("GEDKeeper")]
[assembly: AssemblyCopyright("Copyright © 2018-2021 by Sergey V. Zhdanovskih")]
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyCulture("")]

namespace GKCommunicatorPlugin
{
    public enum CLS
    {
        /* 00 */ LSID_Title,
    }

    public class Plugin : OrdinaryPlugin
    {
        private string fDisplayName = "GKCommunicatorPlugin";
        private ILangMan fLangMan;

        public override string DisplayName { get { return fDisplayName; } }
        public override ILangMan LangMan { get { return fLangMan; } }
        public override IImage Icon { get { return null; } }
        public override PluginCategory Category { get { return PluginCategory.Common; } }

        private ChatForm fForm;

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                CloseForm();
            }
            base.Dispose(disposing);
        }

        internal void CloseForm()
        {
            if (fForm != null) {
                fForm.Close();
                fForm = null;
            }
        }

        public override void Execute()
        {
            if (fForm == null) {
                fForm = new ChatForm();
                fForm.Show();
            } else {
                CloseForm();
            }
        }

        public override void OnLanguageChange()
        {
            try {
                fLangMan = Host.CreateLangMan(this);
                fDisplayName = fLangMan.LS(CLS.LSID_Title);

                //if (fForm != null) fForm.SetLocale();
            } catch (Exception ex) {
                Logger.WriteError("GKCommunicatorPlugin.OnLanguageChange()", ex);
            }
        }

        public override bool Shutdown()
        {
            bool result = true;
            try {
                CloseForm();
            } catch (Exception ex) {
                Logger.WriteError("GKCommunicatorPlugin.Shutdown()", ex);
                result = false;
            }
            return result;
        }
    }
}
