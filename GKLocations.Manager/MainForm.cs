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
            var locRIName = fCore.AddLocationName(locRI.GUID, "Российская Империя", "государство", "", "BET 02 NOV 1721 AND 14 SEP 1917", lang);

            var locPG = fCore.AddLocation();
            var locPGName = fCore.AddLocationName(locPG.GUID, "Пермская губерния", "губерния", "", "BET 12 DEC 1796 AND 03 NOV 1923", lang);

            var locPGRel = fCore.AddLocationRelation(locPG.GUID, locRI.GUID, "P", "BET 12 DEC 1796 AND 03 NOV 1923");
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
