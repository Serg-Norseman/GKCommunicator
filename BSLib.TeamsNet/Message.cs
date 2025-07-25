/*
 *  "BSLib.TeamsNet", the serverless peer-to-peer network library.
 *  Copyright (C) 2018-2025 by Sergey V. Zhdanovskih.
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

namespace BSLib.TeamsNet
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
        }
    }
}
