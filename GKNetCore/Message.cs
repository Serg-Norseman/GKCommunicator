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
using GKNet.Database;

namespace GKNet
{
    public enum MessageStatus : byte
    {
        Undelivered = 0,
        Delivered = 1
    }

    public class Message
    {
        public DateTime Timestamp { get; private set; }
        public string Text { get; private set; }
        public MessageStatus Status { get; private set; }


        public Message(DateTime timestamp, string text)
        {
            Timestamp = timestamp;
            Text = text;
            Status = MessageStatus.Undelivered;
        }

        public void SetDelivered()
        {
            Status = MessageStatus.Delivered;
            Timestamp = DateTime.UtcNow;
        }

        internal DBMessage ToDBRecord()
        {
            var result = new DBMessage();

            result.timestamp = Timestamp;
            result.msg_text = Text;
            result.flags = (int)Status;

            return result;
        }

        internal void FromDBRecord(DBMessage dbMsg)
        {
            Timestamp = dbMsg.timestamp;
            Text = dbMsg.msg_text;
            Status = (MessageStatus)dbMsg.flags;
        }
    }
}
