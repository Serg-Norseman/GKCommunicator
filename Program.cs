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
                port = 6882;
            }

            var dhtClient = new DHTClient(port, IPAddress.Any);
            dhtClient.SubnetKey = NETWORK_SIGN;
            dhtClient.Run();
            dhtClient.ReJoin();
            dhtClient.SendFindNodes();

            Console.ReadLine();
            return 0;
        }
    }
}
