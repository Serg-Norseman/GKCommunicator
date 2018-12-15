using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BSLib;

namespace GKCore.IPC
{
    public sealed class IPCStream : BaseObject
    {
        private static readonly IFormatter BinFormatter = new BinaryFormatter();

        private readonly Stream fBaseStream;

        public IPCStream(Stream baseStream)
        {
            fBaseStream = baseStream;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                fBaseStream.Close();
            }
            base.Dispose(disposing);
        }

        public IPCMessage ReadMessage()
        {
            byte[] buffer = null;
            try {
                int length = fBaseStream.ReadByte() * 256;
                length += fBaseStream.ReadByte();

                buffer = new byte[length];
                fBaseStream.Read(buffer, 0, length);

                return ReadMessage(buffer);
            } catch (Exception ex) {
                return null;
            }
        }

        public IPCMessage ReadMessage(byte[] buffer)
        {
            if (buffer == null) {
                return null;
            } else {
                IPCMessage result;
                using (var memStream = new MemoryStream(buffer)) {
                    result = (IPCMessage)BinFormatter.Deserialize(memStream);
                }
                //result.ReadObject(buffer);
                return result;
            }
        }

        public void WriteMessage(IPCMessage message)
        {
            using (var memStream = new MemoryStream()) {
                BinFormatter.Serialize(memStream, message);
                byte[] msgBuffer = memStream.GetBuffer();

                //byte[] msgBuffer = message.WriteObject();

                int length = msgBuffer.Length;
                if (length > UInt16.MaxValue)
                    throw new InvalidOperationException("Message is too long");

                byte[] buffer = new byte[length + 2];
                buffer[0] = (byte)(length / 256);
                buffer[1] = (byte)(length % 256);
                Buffer.BlockCopy(msgBuffer, 0, buffer, 2, length);

                fBaseStream.Write(buffer, 0, length + 2);
                fBaseStream.Flush();
            }
        }
    }
}
