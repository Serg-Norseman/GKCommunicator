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
