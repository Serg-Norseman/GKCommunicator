using System;
using System.IO;
using System.Net;
using GKNet.Core;
using GKNet.DHT;
using GKNet.Protocol;

namespace DHTSample
{
    class Program : ILogger
    {
        static int Main(string[] args)
        {
            var program = new Program();

            int port;
            if (args.Length == 0 || !int.TryParse(args[0], out port)) {
                port = DHTClient.PublicDHTPort;
            }

            var snkInfoHash = ProtocolHelper.CreateSignInfoKey();
            program.WriteLog("Search for: " + snkInfoHash.ToHexString());

            var dhtClient = new DHTClient(IPAddress.Any, port, program);
            dhtClient.PeersFound += delegate (object sender, PeersFoundEventArgs e) {
                program.WriteLog(string.Format("Found peers: {0}", e.Peers.Count));
                program.WriteLog(string.Format("Peers[0]: {0}", e.Peers[0].ToString()));
            };

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                dhtClient.StopSearch();
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
            }

            var fswriter = new StreamWriter(new FileStream("./logFile", FileMode.Append));
            fswriter.WriteLine(str);
            fswriter.Flush();
            fswriter.Close();
        }
    }
}
