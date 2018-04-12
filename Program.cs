using System;
using System.Net;

namespace DHTConnector
{
    class Program
    {
        public const string NETWORK_SIGN = "GEDKEEPER NETWORK";

        static int Main(string[] args)
        {
            string ip = ""; int iport = 0;

            int port;
            if (args.Length == 0 || (args.Length == 1 && !int.TryParse(args[0], out port))) {
                Console.WriteLine("Please enter a port number.");
                Console.WriteLine("Usage: DHTConnector <port>");
                return 1;
            } else {
                if (args.Length == 1) {
                    int.TryParse(args[0], out port);
                } else if (args.Length == 4 && args[1] == "add") {
                    int.TryParse(args[0], out port);

                    ip = args[2];
                    int.TryParse(args[3], out iport);
                    Console.WriteLine("Add " + ip + ":" + iport);
                } else {
                    return 1;
                }
            }

            var udpServer = new UDPServer(port, IPAddress.Any); // 6882
            udpServer.SubnetKey = NETWORK_SIGN;
            udpServer.Run();
            udpServer.ReJoin();

            if (ip != "" && iport != 0) {
                udpServer.AddNode(ip, iport);
            }

            udpServer.SendFindNodes();

            Console.ReadLine();
            return 0;
        }
    }
}
