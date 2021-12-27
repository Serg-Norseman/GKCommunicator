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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GKNet;

namespace GKNetUI
{
    public class UIHelper
    {
        private static Dictionary<PresenceStatus, Bitmap> fStatusIcons;
        private static Dictionary<MessageStatus, Bitmap> fMessageStatusIcons;

        public static void InitResources()
        {
            fStatusIcons = new Dictionary<PresenceStatus, Bitmap>();
            fStatusIcons[PresenceStatus.Unknown] = null;
            fStatusIcons[PresenceStatus.Offline] = UIHelper.LoadResourceImage("btn_offline");
            fStatusIcons[PresenceStatus.Online] = UIHelper.LoadResourceImage("btn_available");
            fStatusIcons[PresenceStatus.Away] = UIHelper.LoadResourceImage("btn_away");
            fStatusIcons[PresenceStatus.Busy] = UIHelper.LoadResourceImage("btn_busy");
            fStatusIcons[PresenceStatus.Invisible] = UIHelper.LoadResourceImage("btn_invisible");

            fMessageStatusIcons = new Dictionary<MessageStatus, Bitmap>();
            fMessageStatusIcons[MessageStatus.Undelivered] = UIHelper.LoadResourceImage("status_undelivered");
            fMessageStatusIcons[MessageStatus.Delivered] = UIHelper.LoadResourceImage("status_delivered");
        }

        public static void DoneResources()
        {
            foreach (var image in fStatusIcons.Values) {
                if (image != null) {
                    image.Dispose();
                }
            }
            fStatusIcons.Clear();

            foreach (var image in fMessageStatusIcons.Values) {
                if (image != null) {
                    image.Dispose();
                }
            }
            fMessageStatusIcons.Clear();
        }

        public static Bitmap GetPresenceStatusImage(PresenceStatus status)
        {
            return fStatusIcons[status];
        }

        public static Bitmap GetMessageStatusImage(MessageStatus status)
        {
            return fMessageStatusIcons[status];
        }

        public static Bitmap LoadResourceImage(string name)
        {
            string resName = "GKNet.Resources.Images." + name + ".png";
            Stream resStream = Utilities.LoadResourceStream(resName);
            return new Bitmap(resStream);
        }

        public static ToolStripMenuItem AddToolStripItem(ContextMenuStrip contextMenu, string text, object tag, EventHandler clickHandler)
        {
            var tsItem = new ToolStripMenuItem(text, null, clickHandler);
            tsItem.Tag = tag;
            contextMenu.Items.Add(tsItem);
            return tsItem;
        }

        public static T GetMenuItemTag<T>(ContextMenuStrip contextMenu, object sender)
        {
            foreach (ToolStripMenuItem tsItem in contextMenu.Items) {
                tsItem.Checked = false;
            }
            var senderItem = ((ToolStripMenuItem)sender);
            ((ToolStripMenuItem)sender).Checked = true;
            return (T)senderItem.Tag;
        }

        public static void SetMenuItemTag<T>(ContextMenuStrip contextMenu, T value)
        {
            foreach (ToolStripMenuItem tsItem in contextMenu.Items) {
                T itemTag = (T)tsItem.Tag;
                if (Equals(itemTag, value)) {
                    tsItem.PerformClick();
                    break;
                }
            }
        }
    }
}
