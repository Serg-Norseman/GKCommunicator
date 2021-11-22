/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
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
using System.Drawing;
using System.Windows.Forms;

namespace GKCommunicatorApp
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InputDlg : Form
    {
        public string Value
        {
            get { return txtValue.Text; }
            set { txtValue.Text = value; }
        }

        private InputDlg(string caption, string prompt, string value, bool pwMode = false)
        {
            InitializeComponent();

            Text = caption;
            label1.Text = prompt;
            Value = value;

            if (pwMode) {
                txtValue.PasswordChar = '*';
            }

            AcceptButton = btnAccept;
            CancelButton = btnCancel;

            btnAccept.Text = "Accept";
            btnCancel.Text = "Cancel";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                // dummy
            }
            base.Dispose(disposing);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            try {
                DialogResult = DialogResult.OK;
            } catch {
                DialogResult = DialogResult.None;
            }
        }


        public static bool QueryText(string caption, string prompt, ref string value)
        {
            bool result = false;

            using (var inputBox = new InputDlg(caption, prompt, value)) {
                if (inputBox.ShowDialog() == DialogResult.OK) {
                    value = inputBox.Value.Trim();
                    result = true;
                }
            }

            return result;
        }

        public static bool QueryPassword(string caption, string prompt, ref string value)
        {
            bool result = false;

            using (var inputBox = new InputDlg(caption, prompt, value, true)) {
                if (inputBox.ShowDialog() == DialogResult.OK) {
                    value = inputBox.Value.Trim();
                    result = true;
                }
            }

            return result;
        }

        #region Design

        private TextBox txtValue;
        private Label label1;
        private Button btnCancel;
        private Button btnAccept;

        private void InitializeComponent()
        {
            SuspendLayout();

            txtValue = new TextBox();
            txtValue.Location = new Point(12, 25);
            txtValue.Name = "txtValue";
            txtValue.Size = new Size(354, 20);
            txtValue.TabIndex = 0;

            label1 = new Label();
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(35, 13);
            label1.TabIndex = 3;
            label1.Text = "label1";

            btnAccept = new Button();
            btnAccept.ImageAlign = ContentAlignment.MiddleLeft;
            btnAccept.Location = new Point(197, 61);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(81, 25);
            btnAccept.TabIndex = 4;
            btnAccept.Text = "btnAccept";
            btnAccept.TextAlign = ContentAlignment.MiddleRight;
            btnAccept.Click += btnAccept_Click;

            btnCancel = new Button();
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.ImageAlign = ContentAlignment.MiddleLeft;
            btnCancel.Location = new Point(285, 61);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(81, 25);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "btnCancel";
            btnCancel.TextAlign = ContentAlignment.MiddleRight;
            btnCancel.Click += btnCancel_Click;

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(378, 98);
            Controls.Add(btnAccept);
            Controls.Add(btnCancel);
            Controls.Add(label1);
            Controls.Add(txtValue);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputBox";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "InputBox";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
