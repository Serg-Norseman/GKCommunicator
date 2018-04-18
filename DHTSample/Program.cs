using System;
using System.IO;
using System.Net;
using BencodeNET.Objects;
using GKNet.DHT;

namespace DHTSample
{
    class Program : IDHTLogger
    {
        public const string NETWORK_SIGN = "GEDKEEPER NETWORK";

        static int Main(string[] args)
        {
            var program = new Program();

            int port;
            if (args.Length == 0 || !int.TryParse(args[0], out port)) {
                port = DHTClient.PublicDHTPort;
            }

            BDictionary subnetKey = new BDictionary();
            subnetKey.Add("info", NETWORK_SIGN);
            var snkInfoHash = DHTHelper.CalculateInfoHashBytes(subnetKey);
            Console.ForegroundColor = ConsoleColor.White;
            program.WriteLog("Search for: " + snkInfoHash.ToHexString());

            var dhtClient = new DHTClient(IPAddress.Any, port, program);
            dhtClient.PeersFound += delegate (object sender, PeersFoundEventArgs e) {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Found peers: {0}", e.Peers.Count);
                Console.WriteLine("Peers[0]: {0}", e.Peers[0].ToString());
                Console.ResetColor();
            };

            dhtClient.Run();
            dhtClient.JoinNetwork();
            dhtClient.SearchNodes(snkInfoHash);

            Console.ReadLine();
            return 0;
        }

        public void WriteLog(string str, bool display = true)
        {
            if (display) {
                Console.WriteLine(str);
                Console.ResetColor();
            }

            var fswriter = new StreamWriter(new FileStream("./logFile", FileMode.Append));
            fswriter.WriteLine(str);
            fswriter.Flush();
            fswriter.Close();
        }
    }
}
