/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2024 by Sergey V. Zhdanovskih.
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
using System.ComponentModel;
using System.Windows.Forms;
using GKNetLocationsPlugin.Dates;

namespace GKNetLocationsPlugin.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GKDateControl : UserControl
    {
        private readonly IContainer fComponents;
        private readonly ToolTip fToolTip;


        public GDMCustomDate Date
        {
            get { return GetDate(); }
            set { SetDate(value); }
        }


        public GKDateControl()
        {
            fComponents = new Container();
            fToolTip = new ToolTip(this.fComponents);

            InitializeComponent();
            SetLocale();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (fComponents != null) fComponents.Dispose();
            }
            base.Dispose(disposing);
        }

        public void SetLocale()
        {
            fToolTip.SetToolTip(txtDate1, txtDate1.RegionalDatePattern);
            fToolTip.SetToolTip(txtDate2, txtDate2.RegionalDatePattern);
        }

        private GDMCustomDate GetDate()
        {
            GDMCustomDate result = null;

            GDMDate gcd1 = GDMDate.CreateByFormattedStr(txtDate1.NormalizeDate, true);
            if (gcd1 == null) throw new ArgumentNullException("gcd1");
            gcd1.YearBC = chkBC1.Checked;

            GDMDate gcd2 = GDMDate.CreateByFormattedStr(txtDate2.NormalizeDate, true);
            if (gcd2 == null) throw new ArgumentNullException("gcd2");
            gcd2.YearBC = chkBC2.Checked;

            result = GDMCustomDate.CreatePeriod(gcd1, gcd2);

            return result;
        }

        private void SetDate(GDMCustomDate date)
        {
            if (date is GDMDatePeriod) {
                GDMDatePeriod dtPeriod = date as GDMDatePeriod;

                FillControls(1, dtPeriod.DateFrom);
                FillControls(2, dtPeriod.DateTo);
            } else {
                txtDate1.NormalizeDate = "";
                chkBC1.Checked = false;
            }
        }

        private void FillControls(int dateIndex, GDMDate date)
        {
            switch (dateIndex) {
                case 1:
                    txtDate1.NormalizeDate = date.GetDisplayString(DateFormat.dfDD_MM_YYYY);
                    chkBC1.Checked = date.YearBC;
                    break;

                case 2:
                    txtDate2.NormalizeDate = date.GetDisplayString(DateFormat.dfDD_MM_YYYY);
                    chkBC2.Checked = date.YearBC;
                    break;
            }
        }

        #region Design

        private GKDateBox txtDate1;
        private GKDateBox txtDate2;
        private CheckBox chkBC1;
        private CheckBox chkBC2;

        private void InitializeComponent()
        {
            this.txtDate1 = new GKDateBox();
            this.txtDate2 = new GKDateBox();
            this.chkBC2 = new System.Windows.Forms.CheckBox();
            this.chkBC1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();

            this.txtDate1.BackColor = System.Drawing.SystemColors.Window;
            this.txtDate1.Location = new System.Drawing.Point(145, 2);
            this.txtDate1.Name = "txtDate1";
            this.txtDate1.Size = new System.Drawing.Size(158, 24);
            this.txtDate1.TabIndex = 2;

            this.chkBC1.AutoSize = true;
            this.chkBC1.Location = new System.Drawing.Point(259, 33);
            this.chkBC1.Name = "chkBC1";
            this.chkBC1.Size = new System.Drawing.Size(47, 21);
            this.chkBC1.TabIndex = 4;
            this.chkBC1.Text = "BC";
            this.chkBC1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkBC1.UseVisualStyleBackColor = true;

            this.txtDate2.Location = new System.Drawing.Point(313, 2);
            this.txtDate2.Name = "txtDate2";
            this.txtDate2.Size = new System.Drawing.Size(158, 24);
            this.txtDate2.TabIndex = 5;

            this.chkBC2.AutoSize = true;
            this.chkBC2.Location = new System.Drawing.Point(427, 33);
            this.chkBC2.Name = "chkBC2";
            this.chkBC2.Size = new System.Drawing.Size(47, 21);
            this.chkBC2.TabIndex = 7;
            this.chkBC2.Text = "BC";
            this.chkBC2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkBC2.UseVisualStyleBackColor = true;

            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.chkBC2);
            this.Controls.Add(this.chkBC1);
            this.Controls.Add(this.txtDate1);
            this.Controls.Add(this.txtDate2);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "GKDateControl";
            this.Size = new System.Drawing.Size(473, 62);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
