/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using GKLocations.Core;

namespace GKLocations.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly ICore fCore;

        public MainForm()
        {
            InitializeComponent();

            fCore = new GKLCore();

            FillLanguagesCombo();

            FillTests();
        }

        private void FillTests()
        {
            fCore.DeleteDatabase();

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

            // Total 23 transaction
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
    }
}
