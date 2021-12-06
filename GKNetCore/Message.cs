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

        public string Sender { get; set; }
        public string Receiver { get; set; }


        private Message()
        {
        }

        public Message(DateTime timestamp, string text, string sender, string receiver)
        {
            Timestamp = timestamp;
            Text = text;
            Status = MessageStatus.Undelivered;
            Sender = sender;
            Receiver = receiver;
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

            result.sender = Sender;
            result.receiver = Receiver;

            return result;
        }

        internal static Message FromDBRecord(DBMessage dbMsg)
        {
            var result = new Message();

            result.Timestamp = dbMsg.timestamp;
            result.Text = dbMsg.msg_text;
            result.Status = (MessageStatus)dbMsg.flags;

            result.Sender = dbMsg.sender;
            result.Receiver = dbMsg.receiver;

            return result;
        }
    }
}
