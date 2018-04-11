using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BencodeNET.Objects;

namespace DHTConnector
{
    public static class Helpers
    {
        private static Random r = new Random();
        private static SHA1 sha1 = new SHA1CryptoServiceProvider();

        /*public static byte[] GetTransactionID()
        {
            var result = new byte[2];
            r.NextBytes(result);
            return result;
        }*/

        public static byte[] GetRandomID()
        {
            var r = new Random();
            byte[] result = new byte[20];
            r.NextBytes(result);
            return result;
        }

        public static byte[] GetRandomHashID()
        {
            var result = new byte[20];
            r.NextBytes(result);
            lock (sha1) {
                result = sha1.ComputeHash(result);
                return result;
            }
        }

        public static List<PeerNode> ParseNodesList(byte[] data)
        {
            var result = new List<PeerNode>();
            for (int i = 0; i < data.Length; i += 26) {
                var dd = data.Skip(i).Take(26).ToArray();
                //var bc = dd.ToHexString();
                //Console.WriteLine(bc);
                var b = dd[24];
                dd[24] = dd[25];
                dd[25] = b;
                var id = dd.Take(20).ToArray();
                var ip = new IPAddress(dd.Skip(20).Take(4).ToArray());
                var port = BitConverter.ToUInt16(dd, 24);
                var tt = new PeerNode(id, new IPEndPoint(ip, port));
                result.Add(tt);
            }
            return result;
        }

        public static string ToHexString(this byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in data) {
                var t = b / 16;
                sb.Append((char)(t + (t <= 9 ? '0' : 'W')));
                var f = b % 16;
                sb.Append((char)(f + (f <= 9 ? '0' : 'W')));
            }

            return sb.ToString();
        }

        public static byte[] GetNeighbor(byte[] target, byte[] nid)
        {
            var result = new byte[20];
            for (int i = 0; i < 10; i++)
                result[i] = target[i];
            for (int i = 10; i < 20; i++)
                result[i] = nid[i];
            return result;
        }

        public static byte[] CreateQuery()
        {
            BDictionary sendData = new BDictionary();

            return sendData.EncodeAsBytes();
        }

        public static byte[] CreateGetPeersResponse(BString transactionID, byte[] nid, byte[] infoHash)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "r");

            var r = new BDictionary();
            r.Add("id", new BString(nid));
            r.Add("token", new BString(infoHash.Take(2)));
            r.Add("nodes", "");
            sendData.Add("r", r);

            return sendData.EncodeAsBytes();
        }

        public static byte[] CreatePingResponse(BString transactionID, byte[] nid)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("y", "r");
            sendData.Add("t", transactionID);

            var r = new BDictionary();
            r.Add("id", new BString(nid));
            sendData.Add("r", r);

            return sendData.EncodeAsBytes();
        }

        public static byte[] CreateFindNodeQuery(BString transactionID, byte[] nid)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "find_node");

            var args = new BDictionary();
            args.Add("id", new BString(nid));
            args.Add("target", new BString(Helpers.GetRandomHashID()));
            sendData.Add("a", args);

            return sendData.EncodeAsBytes();
        }

        public static byte[] CreateAnnouncePeerQuery(BString transactionID, byte[] nid, byte[] infoHash,
            byte implied_port, int port, BString token)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "announce_peer");

            var args = new BDictionary();
            args.Add("id", new BString(nid));
            args.Add("implied_port", new BNumber(implied_port));
            args.Add("info_hash", new BString(infoHash));
            args.Add("port", new BNumber(port));
            args.Add("token", token);
            sendData.Add("a", args);

            return sendData.EncodeAsBytes();
        }

        public static byte[] CreateGetPeersQuery(BString transactionID, byte[] nid, byte[] infoHash)
        {
            BDictionary sendData = new BDictionary();

            sendData.Add("t", transactionID);
            sendData.Add("y", "q");
            sendData.Add("q", "get_peers");

            var args = new BDictionary();
            args.Add("id", new BString(nid));
            args.Add("info_hash", new BString(infoHash));
            sendData.Add("a", args);

            return sendData.EncodeAsBytes();
        }

        /*public static string ToHexString2(this byte[] data)
        {
            return string.Join("", data.SelectMany((x) => new int[] { x / 16, x % 16 }).ToList().Select(x => (char)(x + (x <= 9 ? '0' : 'W'))));
        }*/

        /*public static byte[] ParseToHex(this string data)
        {
            if (data.Length % 2 != 0)
                throw new Exception();
            data = data.ToLower();
            List<int> temp = new List<int>();
            foreach (var c in data) {
                var t = c - '0';
                if (t < 0)
                    throw new Exception();
                else if (t > 9) {
                    t = t - ('a' - '0');
                    if (t < 0 || t > 5)
                        throw new Exception();
                    temp.Add(t + 10);
                } else
                    temp.Add(t);
            }
            return temp.Tuken().Select(x => (byte)(x.Item1 * 16 + x.Item2)).ToArray();
        }*/

        /*public static IEnumerable<(T, T)> Tuken<T>(this IEnumerable<T> array)
        {
            if (array.Count() % 2 != 0)
                throw new Exception();
            for (int i = 0, len = array.Count(); i < len; i += 2) {
                var cc = array.Skip(i).Take(2).ToList();
                yield return (cc[0], cc[1]);
            }
        }*/

        /*public static byte[] ToDicBytes(this Dictionary<string, object> temp)
        {
            var result = new List<byte>();
            result.Add((byte)'d');
            foreach (var tempItem in temp) {
                var key = tempItem.Key;
                if (tempItem.Value.GetType() == typeof(string)) {
                    result.AddRange(Encoding.ASCII.GetBytes($"{key.Length}:{key}"));
                    var value = tempItem.Value as string;
                    result.AddRange(Encoding.ASCII.GetBytes($"{value.Length}:{value}"));
                }
                if (tempItem.Value.GetType() == typeof(byte[])) {
                    result.AddRange(Encoding.ASCII.GetBytes($"{key.Length}:{key}"));
                    var value = tempItem.Value as byte[];
                    result.AddRange(Encoding.ASCII.GetBytes($"{value.Length}:"));
                    result.AddRange(value);
                }
                if (tempItem.Value.GetType() == typeof(Dictionary<string, object>)) {
                    result.AddRange(Encoding.ASCII.GetBytes($"{key.Length}:{key}"));
                    var val = ToDicBytes((Dictionary<string, Object>)tempItem.Value);
                    result.AddRange(val);
                }
            }
            result.Add((byte)'e');
            return result.ToArray();
        }*/
    }
}
