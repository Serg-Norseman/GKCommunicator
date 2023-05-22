/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2023 by Sergey V. Zhdanovskih.
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

using Eto.Drawing;
using Eto.Forms;
using GKNet;

namespace GKNetUI
{
    public class PeersList : ListBox
    {
        public bool ShowConnectionInfo
        {
            get; set;
        }

        public PeersList()
        {
        }

        /*protected override void OnMeasureItem(MeasureItemEventArgs e)
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
                    int iconY = (!ShowConnectionInfo) ? (rt.Height - icon.Width) / 2 : rt.Top + 2;
                    e.Graphics.DrawImage(icon, rt.Right - icon.Width - 2, iconY);
                }
            }

            rt.Inflate(-5, -2);

            //StringFormat fmt = new StringFormat();
            //fmt.Alignment = StringAlignment.Near;
            //fmt.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(GetPeerItem(peer), fnt, Brushes.Black, rt);

            //base.OnDrawItem(e);
        }*/

        private string GetPeerItem(Peer peer)
        {
            string location = (peer.IsLocal) ? "local" : "external";
            string connInfo = string.Format("{0} ({1}, {2})", peer.EndPoint, peer.State, location);
            string peerName = (peer.IsLocal || peer.State == PeerState.Identified) ? peer.Profile.UserName : "???";
            string result = (!ShowConnectionInfo) ? peerName : string.Format("{0}\r\n{1}", peerName, connInfo);
            return result;
        }
    }
}
