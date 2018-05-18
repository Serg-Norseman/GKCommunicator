using System;

namespace GKNet
{
    public class Message
    {
        public DateTime Timestamp { get; private set; }
        public string Text { get; private set; }

        public Message(DateTime timestamp, string text)
        {
            Timestamp = timestamp;
            Text = text;
        }
    }
}
