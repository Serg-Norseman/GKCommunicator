// https://gist.github.com/gubatron/cd9cfa66839e18e49846

using System;
using System.Net;

namespace DHTConnector
{
    class Program
    {
        public const string NETWORK_SIGN = "GEDKEEPER NETWORK";

        static void Main(string[] args)
        {
            var udpServer = new UDPServer(6882, IPAddress.Any);
            udpServer.Run();
            udpServer.ReJoin();
            udpServer.SendFindNodes();

            Console.ReadLine();
        }
    }
}
