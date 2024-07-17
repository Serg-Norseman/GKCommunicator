/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BSLib;
using GKNet;
using GKNetLocationsPlugin.Controls;
using GKNetLocationsPlugin.Dates;
using GKNetLocationsPlugin.Model;

namespace GKNetLocationsPlugin.Editor
{
    public partial class LocationsControl : UserControl, IDataEditor
    {
        private ToolStrip toolStrip1;
        private ToolStripComboBox cmbLanguages;
        private GKListView lstLocations;
        private HyperView hvSummary;

        private ICommunicatorCore fHost;
        private GKLCore fCore;


        public LocationsControl()
        {
            cmbLanguages = new ToolStripComboBox();
            cmbLanguages.Name = "cmbLanguages";
            cmbLanguages.Size = new System.Drawing.Size(121, 28);

            toolStrip1 = new ToolStrip();
            toolStrip1.Dock = DockStyle.Top;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { cmbLanguages });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Size = new System.Drawing.Size(1144, 31);
            toolStrip1.TabIndex = 0;

            lstLocations = new GKListView();
            lstLocations.Dock = DockStyle.Fill;
            lstLocations.Location = new System.Drawing.Point(0, 31);
            lstLocations.Name = "treeControl1";
            lstLocations.Size = new System.Drawing.Size(1144, 100);
            lstLocations.TabIndex = 1;
            lstLocations.AddColumn("Location", 300);
            lstLocations.SelectedIndexChanged += lstLocations_SelectedIndexChanged;

            hvSummary = new HyperView();
            hvSummary.BorderWidth = 4;
            hvSummary.Dock = DockStyle.Bottom;
            hvSummary.Size = new Size(1144, 290);
            hvSummary.OnLink += hvSummary_Link;

            var spl = new Splitter();
            spl.Dock = DockStyle.Bottom;
            spl.Size = new Size(1144, 4);
            spl.MinExtra = 100;
            spl.MinSize = 100;

            SuspendLayout();

            Controls.Add(toolStrip1);
            Controls.Add(lstLocations);
            Controls.Add(hvSummary);
            Controls.Add(spl);

            Controls.SetChildIndex(toolStrip1, 0);
            Controls.SetChildIndex(lstLocations, 1);
            Controls.SetChildIndex(spl, 2);
            Controls.SetChildIndex(hvSummary, 3);

            ResumeLayout(false);
            PerformLayout();
        }

        private void lstLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            DBLocationRec locRec = (DBLocationRec)lstLocations.GetSelectedData();

            ShowLocationInfo(locRec, hvSummary.Lines);
        }

        private void hvSummary_Link(object sender, string linkName)
        {
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

            FillLanguagesCombo();

            FillTests();

            UpdateContent(GetSelectedLanguage());
        }

        public void ShowLocationInfo(DBLocationRec locRec, StringList summary)
        {
            if (summary == null)
                return;

            try {
                summary.BeginUpdate();
                try {
                    summary.Clear();
                    if (locRec != null) {
                        summary.Add("");

                        for (int i = 0; i < locRec.Names.Count; i++) {
                            var locName = locRec.Names[i];
                            summary.Add("[u][b][size=+1]" + locName.Name + "[/size][/b][/u]");

                            string st = locName.ActualDatesEx.GetDisplayStringExt(DateFormat.dfDD_MM_YYYY, true, true);
                            if (!string.IsNullOrEmpty(st)) {
                                summary.Add("    " + st);
                            }

                            summary.Add("");
                        }

                        summary.Add("Координаты: " + locRec.Coordinates);

                        var fullNames = GetFullNames(locRec);
                        if (fullNames.Count > 0) {
                            summary.Add("");
                            summary.Add("История:");

                            int num = fullNames.Count;
                            for (int i = 0; i < num; i++) {
                                var xName = fullNames[i];
                                summary.Add("    " + string.Format("{0}: {1}", xName.ActualDatesEx.GetDisplayStringExt(DateFormat.dfDD_MM_YYYY, true, true), xName.Name));
                            }
                        }
                    }
                } finally {
                    summary.EndUpdate();
                }
            } catch (Exception ex) {
                //Logger.WriteError("GKUtils.ShowLocationInfo()", ex);
            }
        }

        public List<DBLocationNameRec> GetFullNames(DBLocationRec locRec)
        {
            var result = new List<DBLocationNameRec>();

            if (locRec.Relations.Count > 0) {
                var buffer = new List<DBLocationNameRec>();

                for (int j = 0; j < locRec.Relations.Count; j++) {
                    var topLevel = locRec.Relations[j];
                    var topLoc = fCore.GetLocation(topLevel.OwnerGUID);
                    if (topLoc == null)
                        continue;

                    var topNames = GetFullNames(topLoc);
                    for (int i = 0; i < topNames.Count; i++) {
                        var topName = topNames[i];

                        var interDate = GDMCustomDate.GetIntersection(topLevel.ActualDatesEx, topName.ActualDatesEx);
                        if (!interDate.IsEmpty()) {
                            var newLocName = new DBLocationNameRec();
                            newLocName.Name = topName.Name;
                            newLocName.ActualDatesEx = interDate;
                            buffer.Add(newLocName);
                        }
                    }
                }

                for (int j = 0; j < buffer.Count; j++) {
                    var topLocName = buffer[j];
                    var topName = topLocName.Name;
                    var topDate = topLocName.ActualDatesEx;

                    for (int i = 0; i < locRec.Names.Count; i++) {
                        var locName = locRec.Names[i];

                        var interDate = GDMCustomDate.GetIntersection(topDate, locName.ActualDatesEx);
                        if (!interDate.IsEmpty()) {
                            string newName = locName.Name + ", " + topName;

                            var newLocName = new DBLocationNameRec();
                            newLocName.Name = newName;
                            newLocName.ActualDatesEx = interDate;
                            result.Add(newLocName);
                        }
                    }
                }
            } else {
                for (int i = 0; i < locRec.Names.Count; i++) {
                    var locName = locRec.Names[i];
                    if (locName.ActualDatesEx.IsEmpty())
                        continue;

                    result.Add(locName);
                }
            }

            return result;
        }

        public string GetNameByDate(DBLocationRec locRec, GDMCustomDate date, bool full = false)
        {
            if (date != null && !date.IsEmpty()) {
                var namesList = !full ? locRec.Names : GetFullNames(locRec);

                for (int i = 0; i < namesList.Count; i++) {
                    var locName = namesList[i];

                    var interDate = GDMCustomDate.GetIntersection(date, locName.ActualDatesEx);
                    if (!interDate.IsEmpty()) {
                        return locName.Name;
                    }
                }
            }

            return (locRec.Names.Count == 0) ? string.Empty : locRec.Names[locRec.Names.Count - 1].Name;
        }

        public void UpdateContent(string lang)
        {
            var locations = fCore.Database.QueryLocations();
            fCore.Database.PreloadLocations(locations);

            lstLocations.BeginUpdate();
            try {
                foreach (var loc in locations) {
                    lstLocations.AddItem(loc, GetNameByDate(loc, null, false));
                }
            } finally {
                lstLocations.EndUpdate();
            }
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
            // FIXME: debug only!
            fCore.DeleteDatabase();

            //fCore.BlockchainNode.Chain.CreateGenesisBlock();

            var lang = fCore.GetCurrentLanguage();

            var locRI = fCore.AddLocation();
            fCore.AddLocationName(locRI.GUID, "Российская Империя", "государство", "FROM 02 NOV 1721 TO 14 SEP 1917", lang);

            var locPG = fCore.AddLocation();
            fCore.AddLocationName(locPG.GUID, "Пермская губерния", "губерния", "FROM 12 DEC 1796 TO 03 NOV 1923", lang);
            fCore.AddLocationRelation(locPG.GUID, locRI.GUID, "P", "FROM 12 DEC 1796 TO 03 NOV 1923");

            var locKUU = fCore.AddLocation();
            fCore.AddLocationName(locKUU.GUID, "Красноуфимский уезд", "уезд", "FROM 1781 TO 1923", lang);
            fCore.AddLocationRelation(locKUU.GUID, locPG.GUID, "P", "FROM 1781 TO 1923");

            var locSHV = fCore.AddLocation();
            fCore.AddLocationName(locSHV.GUID, "Шайтанская волость", "волость", "FROM 1727 TO 1919", lang);
            fCore.AddLocationRelation(locSHV.GUID, locKUU.GUID, "P", "FROM 1727 TO 1919");

            var locSHZ = fCore.AddLocation();
            fCore.AddLocationName(locSHZ.GUID, "село Шайтанский завод", "село", "FROM 1727 TO 1907", lang);
            fCore.AddLocationRelation(locSHZ.GUID, locSHV.GUID, "P", "FROM 1727 TO 1907");


            var locEKU = fCore.AddLocation();
            fCore.AddLocationName(locEKU.GUID, "Екатеринбургский уезд", "уезд", "FROM 1781 TO 1923", lang);
            fCore.AddLocationRelation(locEKU.GUID, locPG.GUID, "P", "FROM 1781 TO 1923");

            var locNVV = fCore.AddLocation();
            fCore.AddLocationName(locNVV.GUID, "Невьянская волость", "волость", "FROM 1701 TO 1919", lang);
            fCore.AddLocationRelation(locNVV.GUID, locEKU.GUID, "P", "FROM 1701 TO 1919");

            var locNVZ = fCore.AddLocation();
            fCore.AddLocationName(locNVZ.GUID, "село Невьянский завод", "село", "FROM 1701 TO 1919", lang);
            fCore.AddLocationRelation(locNVZ.GUID, locNVV.GUID, "P", "FROM 1701 TO 1919");

            // Total 23 transaction, 4360 bytes

            //fCore.BlockchainNode.Chain.CreateNewBlock();
            //var t = fCore.Database.QueryHigherLocations(locNVZ.GUID);
            //Console.WriteLine(t.Count == 5);
        }
    }
}
