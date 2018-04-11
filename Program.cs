using System;
using System.Net;

namespace DHTConnector
{
    class Program
    {
        public const string NETWORK_SIGN = "GEDKEEPER NETWORK";

        static int Main(string[] args)
        {
            int port;
            if (args.Length == 0 || !int.TryParse(args[0], out port)) {
                Console.WriteLine("Please enter a port number.");
                Console.WriteLine("Usage: DHTConnector <port>");
                return 1;
            }

            var udpServer = new UDPServer(port, IPAddress.Any); // 6882
            udpServer.SubnetKey = NETWORK_SIGN;
            udpServer.Run();
            udpServer.ReJoin();
            udpServer.SendFindNodes();

            Console.ReadLine();
            return 0;
        }
    }
}
