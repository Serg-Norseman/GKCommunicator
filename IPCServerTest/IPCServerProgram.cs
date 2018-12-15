using System;
using GKCore.IPC;

namespace IPCServerTest
{
    public static class IPCServerProgram
    {
        private static IPCServerChannel fChannel;

        private static void OnDataReceive(object sender, DataReceiveEventArgs e)
        {
            string msg = e.Message == null ? string.Empty : e.Message.ToString();
            Console.WriteLine("Received: [" + e.EndPoint + "], "  + msg);
            if (e.Stream != null && e.Message != null) {
                e.Message.Return = "answer";
                e.Stream.WriteMessage(e.Message);
            }
        }

        public static void Main(string[] args)
        {
            fChannel = new IPCServerChannel("gk", 5000);
            fChannel.DataReceive += OnDataReceive;
            fChannel.Connect();

            Console.ReadLine();
        }
    }
}
