/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2022 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GKCommunicator".
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

using System.Windows.Forms;
using GKNet;
using GKNetLocationsPlugin.Model;

namespace GKNetLocationsPlugin.Editor
{
    public partial class LocationsControl : UserControl, IDataEditor
    {
        private ToolStrip toolStrip1;
        private ToolStripComboBox cmbLanguages;
        private TreeControl treeControl1;

        private ICommunicatorCore fHost;
        private GKLCore fCore;


        public LocationsControl()
        {
            cmbLanguages = new ToolStripComboBox();
            cmbLanguages.Name = "cmbLanguages";
            cmbLanguages.Size = new System.Drawing.Size(121, 28);

            toolStrip1 = new ToolStrip();
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { cmbLanguages });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Size = new System.Drawing.Size(1144, 31);
            toolStrip1.TabIndex = 0;

            treeControl1 = new TreeControl();
            treeControl1.Dock = DockStyle.Fill;
            treeControl1.Location = new System.Drawing.Point(0, 31);
            treeControl1.Name = "treeControl1";
            treeControl1.Size = new System.Drawing.Size(1144, 591);
            treeControl1.TabIndex = 1;

            SuspendLayout();
            Controls.Add(treeControl1);
            Controls.Add(toolStrip1);
            ResumeLayout(false);
            PerformLayout();
        }

        public LocationsControl(IDataPlugin dataPlugin) : this()
        {
            Init(dataPlugin);
        }

        public void Init(IDataPlugin dataPlugin)
        {
            var locationsDataPlugin = ((LocationsDataPlugin)dataPlugin);

            fHost = locationsDataPlugin.Host;
            fCore = locationsDataPlugin.Core;

            treeControl1.Core = fCore;

            FillLanguagesCombo();

            FillTests();

            treeControl1.UpdateContent(GetSelectedLanguage());
        }

        private void FillLanguagesCombo()
        {
            cmbLanguages.Items.Clear();

            var languages = fCore.GetUsedLanguages();
            foreach (var lang in languages) {
                cmbLanguages.Items.Add(lang);
            }

            var curLang = fCore.GetCurrentLanguage();
            cmbLanguages.Text = curLang;
        }

        public string GetSelectedLanguage()
        {
            string result = cmbLanguages.Text;
            return (!string.IsNullOrEmpty(result)) ? result : fCore.GetCurrentLanguage();
        }

        private void FillTests()
        {
            fCore.DeleteDatabase();

            //fCore.BlockchainNode.Chain.CreateGenesisBlock();

            var lang = fCore.GetCurrentLanguage();

            var locRI = fCore.AddLocation();
            fCore.AddLocationName(locRI.GUID, "Российская Империя", "государство", "", "BET 02 NOV 1721 AND 14 SEP 1917", lang);

            var locPG = fCore.AddLocation();
            fCore.AddLocationName(locPG.GUID, "Пермская губерния", "губерния", "", "BET 12 DEC 1796 AND 03 NOV 1923", lang);
            fCore.AddLocationRelation(locPG.GUID, locRI.GUID, "P", "BET 12 DEC 1796 AND 03 NOV 1923");

            var locKUU = fCore.AddLocation();
            fCore.AddLocationName(locKUU.GUID, "Красноуфимский уезд", "уезд", "", "BET 1781 AND 1923", lang);
            fCore.AddLocationRelation(locKUU.GUID, locPG.GUID, "P", "BET 1781 AND 1923");

            var locSHV = fCore.AddLocation();
            fCore.AddLocationName(locSHV.GUID, "Шайтанская волость", "волость", "", "", lang);
            fCore.AddLocationRelation(locSHV.GUID, locKUU.GUID, "P", "");

            var locSHZ = fCore.AddLocation();
            fCore.AddLocationName(locSHZ.GUID, "село Шайтанский завод", "село", "", "", lang);
            fCore.AddLocationRelation(locSHZ.GUID, locSHV.GUID, "P", "");


            var locEKU = fCore.AddLocation();
            fCore.AddLocationName(locEKU.GUID, "Екатеринбургский уезд", "уезд", "", "BET 1781 AND 1923", lang);
            fCore.AddLocationRelation(locEKU.GUID, locPG.GUID, "P", "BET 1781 AND 1923");

            var locNVV = fCore.AddLocation();
            fCore.AddLocationName(locNVV.GUID, "Невьянская волость", "волость", "", "", lang);
            fCore.AddLocationRelation(locNVV.GUID, locEKU.GUID, "P", "");

            var locNVZ = fCore.AddLocation();
            fCore.AddLocationName(locNVZ.GUID, "село Невьянский завод", "село", "", "", lang);
            fCore.AddLocationRelation(locNVZ.GUID, locNVV.GUID, "P", "");

            // Total 23 transaction, 4360 bytes

            //fCore.BlockchainNode.Chain.CreateNewBlock();
        }
    }
}
