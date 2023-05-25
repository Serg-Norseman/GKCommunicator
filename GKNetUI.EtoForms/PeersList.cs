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

using System;
using System.Collections.ObjectModel;
using Eto.Drawing;
using Eto.Forms;
using GKNet;

namespace GKNetUI
{
    public class PeersList : Panel
    {
        private Font fFont;
        private GridView fGrid;
        private ObservableCollection<Peer> fItems;

        public Collection<Peer> Items
        {
            get { return fItems; }
        }

        public bool ShowConnectionInfo
        {
            get; set;
        }

        public Peer SelectedValue
        {
            get { return fGrid.SelectedItem as Peer; }
            set { fGrid.SelectRow(fItems.IndexOf(value)); }
        }

        public event EventHandler<EventArgs> SelectionChanged
        {
            add {
                fGrid.SelectionChanged += value;
            }
            remove {
                fGrid.SelectionChanged -= value;
            }
        }

        public PeersList()
        {
            fItems = new ObservableCollection<Peer>();

            fFont = new Font(FontFamilies.SansFamilyName, 9, FontStyle.None);

            fGrid = new GridView();
            fGrid.AllowMultipleSelection = false;
            fGrid.AllowEmptySelection = false;
            fGrid.ShowHeader = false;
            fGrid.DataStore = fItems;
            fGrid.RowHeight = (int)(fFont.LineHeight * 3);

            var drawableCell = new DrawableCell();
            drawableCell.Paint += (sender, e) => {
                var peer = e.Item as Peer;
                if (peer != null) {
                    Color textColor;
                    if (e.CellState.HasFlag(CellStates.Selected)) {
                        e.Graphics.FillRectangle(Brushes.Cached(Colors.Blue), e.ClipRectangle);
                        textColor = Colors.White;
                    } else {
                        e.Graphics.FillRectangle(Brushes.Cached(Colors.White), e.ClipRectangle);
                        textColor = Colors.Black;
                    }

                    var rect = e.ClipRectangle;

                    e.Graphics.DrawRectangle(textColor, rect);

                    rect.Inflate(-5, -5);

                    if (peer.State == PeerState.Identified) {
                        var status = peer.Presence;
                        var icon = UIHelper.GetPresenceStatusImage(status);
                        if (icon != null) {
                            float iconY = (!ShowConnectionInfo) ? (rect.Height - icon.Width) / 2 : rect.Top + 2;
                            e.Graphics.DrawImage(icon, rect.Right - icon.Width - 2, iconY);
                        }
                    }

                    rect.Inflate(-5, -2);

                    e.Graphics.DrawText(fFont, Brushes.Cached(textColor), rect, GetPeerItem(peer));
                }
            };
            fGrid.Columns.Add(new GridColumn {
                HeaderText = "Peer",
                DataCell = drawableCell,
                Expand = true
            });

            Content = fGrid;
        }

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
