using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace DHTConnector
{
    public static class Helpers
    {
        private static Random r = new Random();
        private static SHA1 sha1 = new SHA1CryptoServiceProvider();

        public static byte[] GetTil()
        {
            var result = new byte[2];
            r.NextBytes(result);
            return result;
        }

        public static byte[] GetRandomID()
        {
            var result = new byte[20];
            r.NextBytes(result);
            lock (sha1) {
                result = sha1.ComputeHash(result);
                return result;
            }
        }

        public static List<Tuple<byte[], IPEndPoint>> ParseNodesList(byte[] data)
        {
            var result = new List<Tuple<byte[], IPEndPoint>>();
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
                var tt = new Tuple<byte[], IPEndPoint>(id, new IPEndPoint(ip, port));
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

        public static string ToHexString2(this byte[] data)
        {
            return string.Join("", data.SelectMany((x) => new int[] { x / 16, x % 16 }).ToList().Select(x => (char)(x + (x <= 9 ? '0' : 'W'))));
        }

        public static byte[] ParseToHex(this string data)
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
        }

        public static IEnumerable<(T, T)> Tuken<T>(this IEnumerable<T> array)
        {
            if (array.Count() % 2 != 0)
                throw new Exception();
            for (int i = 0, len = array.Count(); i < len; i += 2) {
                var cc = array.Skip(i).Take(2).ToList();
                yield return (cc[0], cc[1]);
            }
        }

        public static byte[] ToDicBytes(this Dictionary<string, object> temp)
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
        }
    }
}
