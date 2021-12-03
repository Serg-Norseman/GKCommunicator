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
using GKNet;

namespace GKNetUI
{
    public class PeersList : ListBox
    {
        public PeersList()
        {
            DrawMode = DrawMode.OwnerDrawVariable;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);

            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            Peer peer = (Peer)Items[e.Index];

            int defItemHeight = e.ItemHeight;

            if (peer.State == PeerState.Identified) {
            }

            e.ItemHeight = defItemHeight * 3;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            Peer peer = (Peer)Items[e.Index];
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Rectangle rt = e.Bounds;
            Font fnt = Font;

            Brush sysBrush = (isSelected) ? SystemBrushes.GradientActiveCaption : SystemBrushes.Control;
            e.Graphics.FillRectangle(sysBrush, rt);

            if (isSelected) {
                Rectangle rt1 = rt;
                rt1.Width -= 1;
                rt1.Height -= 1;
                e.Graphics.DrawRectangle(SystemPens.WindowFrame, rt1);
            }

            if (peer.State == PeerState.Identified) {
                var status = peer.Presence;
                var icon = UIHelper.GetPresenceStatusImage(status);
                if (icon != null) {
                    e.Graphics.DrawImage(icon, rt.Right - icon.Width - 2, rt.Top + 2);
                    //fnt = new Font(fnt, FontStyle.Bold);
                }
            }

            rt.Inflate(-5, -2);

            StringFormat fmt = new StringFormat();
            fmt.Alignment = StringAlignment.Near;
            fmt.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(peer.ToString(), fnt, Brushes.Black, rt, fmt);

            base.OnDrawItem(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
        }
    }
}
