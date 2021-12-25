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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BSLib;
using GKNet;

namespace GKNetUI
{
    public class MessagesList : ListBox
    {
        public string LocalId
        {
            get; set;
        }

        public ICommunicatorCore Core
        {
            get; set;
        }


        public MessagesList()
        {
            DrawMode = DrawMode.OwnerDrawVariable;
            DoubleBuffered = true;
            BackColor = Color.LightGray;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        public void ScrollToBottom()
        {
            TopIndex = Items.Count - 1;
        }

        private const int ItemMargin = 10;

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);

            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            GKNet.Message msg = (GKNet.Message)Items[e.Index];
            string txt = GetMessageItem(msg);
            SizeF txt_size = e.Graphics.MeasureString(txt, this.Font);

            e.ItemHeight = (int)txt_size.Height + 2 * ItemMargin;
            e.ItemWidth = (int)txt_size.Width;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            GKNet.Message msg = (GKNet.Message)Items[e.Index];
            bool senderIsLocal = (msg.Sender == LocalId);
            string txt = GetMessageItem(msg);

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Rectangle rt = e.Bounds;
            Font fnt = Font;

            Brush itemBrush;
            if (isSelected) {
                itemBrush = (senderIsLocal) ? Brushes.Goldenrod : Brushes.LightSteelBlue;
            } else {
                itemBrush = (senderIsLocal) ? Brushes.PaleGoldenrod : Brushes.GhostWhite;
            }

            Rectangle rt1 = rt;
            rt1.Width -= 1;
            rt1.Height -= 1;
            using (GraphicsPath path =
                CreateRoundedRectangle(rt1.Left, rt1.Top + 3, rt1.Width, rt1.Height - 3 * 2, 5)) {
                e.Graphics.FillPath(itemBrush, path);
                e.Graphics.DrawPath(Pens.Black, path);
            }

            rt.Inflate(-5, -2);

            StringFormat fmt = new StringFormat();
            fmt.Alignment = StringAlignment.Near;
            fmt.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(txt, fnt, Brushes.Black, rt, fmt);

            base.OnDrawItem(e);
        }

        private string GetMessageItem(GKNet.Message msg)
        {
            string deliver = (msg.Status == MessageStatus.Delivered) ? " <vv>" : " <-->";
            bool senderIsLocal = (msg.Sender == LocalId);
            Peer sender = Core.FindPeer(msg.Sender);

            string text = msg.Text;
            string senderName = (senderIsLocal) ? string.Empty : string.Format("[{0}]", sender.Profile.UserName);

            string result = string.Format("{0:yyyy/MM/dd HH:mm} {1} {2}\r\n{3}", msg.Timestamp, senderName, deliver, text);
            return result;
        }

        private static GraphicsPath CreateRoundedRectangle(float x, float y, float width, float height, float radius)
        {
            float xw = x + width;
            float yh = y + height;
            float xwr = xw - radius;
            float yhr = yh - radius;
            float xr = x + radius;
            float yr = y + radius;
            float r2 = radius * 2;
            float xwr2 = xw - r2;
            float yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            p.AddArc(x, y, r2, r2, 180, 90); // Top Left Corner
            p.AddLine(xr, y, xwr, y); // Top Edge
            p.AddArc(xwr2, y, r2, r2, 270, 90); // Top Right Corner
            p.AddLine(xw, yr, xw, yhr); // Right Edge
            p.AddArc(xwr2, yhr2, r2, r2, 0, 90); // Bottom Right Corner
            p.AddLine(xwr, yh, xr, yh); // Bottom Edge
            p.AddArc(x, yhr2, r2, r2, 90, 90); // Bottom Left Corner
            p.AddLine(x, yhr, x, yr); // Left Edge

            p.CloseFigure();
            return p;
        }
    }
}
