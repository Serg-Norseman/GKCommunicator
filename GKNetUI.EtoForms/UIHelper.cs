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
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
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

        public static RadioMenuItem AddToolStripItem(ContextMenu contextMenu, RadioMenuItem controller, string text, object tag, EventHandler<EventArgs> clickHandler)
        {
            var tsItem = new RadioMenuItem(controller);
            tsItem.Text = text;
            tsItem.Tag = tag;
            tsItem.Click += clickHandler;
            contextMenu.Items.Add(tsItem);
            return tsItem;
        }

        public static T GetMenuItemTag<T>(ContextMenu contextMenu, object sender)
        {
            foreach (RadioMenuItem tsItem in contextMenu.Items) {
                tsItem.Checked = false;
            }
            var senderItem = ((RadioMenuItem)sender);
            ((RadioMenuItem)sender).Checked = true;
            return (T)senderItem.Tag;
        }

        public static void SetMenuItemTag<T>(ContextMenu contextMenu, T value)
        {
            foreach (RadioMenuItem tsItem in contextMenu.Items) {
                T itemTag = (T)tsItem.Tag;
                if (Equals(itemTag, value)) {
                    tsItem.PerformClick();
                    break;
                }
            }
        }



        public static RadioMenuItem AddToolStripItem(DropDownToolItem owner, ref RadioMenuItem controller, string text, object tag, EventHandler<EventArgs> clickHandler)
        {
            var tsItem = new RadioMenuItem(controller);
            tsItem.Text = text;
            tsItem.Tag = tag;
            tsItem.Click += clickHandler;
            owner.Items.Add(tsItem);

            if (controller == null)
                controller = tsItem;

            return tsItem;
        }

        public static T GetMenuItemTag<T>(DropDownToolItem owner, object sender)
        {
            foreach (RadioMenuItem tsItem in owner.Items) {
                tsItem.Checked = false;
            }
            var senderItem = ((RadioMenuItem)sender);
            ((RadioMenuItem)sender).Checked = true;
            return (T)senderItem.Tag;
        }

        public static void SetMenuItemTag<T>(DropDownToolItem owner, T value)
        {
            foreach (RadioMenuItem tsItem in owner.Items) {
                T itemTag = (T)tsItem.Tag;
                if (Equals(itemTag, value)) {
                    tsItem.PerformClick();
                    break;
                }
            }
        }
    }
}
