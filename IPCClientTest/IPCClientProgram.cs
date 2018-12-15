using System;
using System.Threading;
using GKCore.IPC;

namespace IPCClientTest
{
    public interface IGKHost
    {
        void Select(string fileName);
    }

    public static class IPCClientProgram
    {
        private static IPCClientChannel fChannel;

        private static void OnDataReceive(object sender, DataReceiveEventArgs e)
        {
            string data = (e.Message == null) ? string.Empty : e.Message.ToString();
            Console.WriteLine("Received: " + data);
        }

        public static void Main(string[] args)
        {
            fChannel = new IPCClientChannel("gk", 5000);
            IGKHost host = fChannel.GetServiceProxy<IGKHost>();
            fChannel.DataReceive += OnDataReceive;
            fChannel.Connect();

            Console.WriteLine("CONNECTED");

            while (true) {
                try {
                    var rnd = new Random();
                    var x = rnd.Next();
                    Console.WriteLine("Sended: " + x);
                    var result = fChannel.SendMessage("service", x.ToString(), new object[0] {});
                    Console.WriteLine("Return: " + result);

                    host.Select("sample filename");
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                Thread.Sleep(1000);
            }

            Console.ReadLine();
        }
    }
}
