using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BencodeNET.Objects;
using BencodeNET.Parsing;

namespace DHTConnector
{
    public class BittorrentDownloader
    {
        public IPEndPoint Ipaddress = null;
        public byte[] InfoHash = null;
        public byte[] NodeID = null;

        private Socket fSocket = null;
        private byte[] fBuffer = new byte[4096];
        private List<byte> fDataBuffer = new List<byte>();
        private int fState = 0;

        public BittorrentDownloader(IPEndPoint ipinfo, byte[] infohash, byte[] nodeid)
        {
            Ipaddress = ipinfo;
            InfoHash = infohash;
            NodeID = nodeid;
        }

        public void Run()
        {
            fSocket.BeginConnect(Ipaddress, new AsyncCallback(EndConnect), null);
        }

        private void SendShakeHand()
        {
            var peer = DHTHelper.GetRandomHashID();
            var list = new List<byte>();
            list.Add(0x13);
            list.AddRange(Encoding.ASCII.GetBytes("BitTorrent protocol"));
            list.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00 });
            list.AddRange(InfoHash);
            list.AddRange(peer);
            fSocket.Send(list.ToArray(), SocketFlags.None);
        }

        private void EndConnect(IAsyncResult result)
        {
            try {
                fSocket.EndConnect(result);
                fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None, new AsyncCallback(EndRecvData), null);
                SendShakeHand();
            } catch {
            }
        }

        private void EndRecvData(IAsyncResult result)
        {
            var recvCount = fSocket.EndReceive(result);
            fDataBuffer.AddRange(fBuffer.Take(recvCount));
            check();
            if (fState == 1) {

            }
            fSocket.BeginReceive(fBuffer, 0, fBuffer.Length, SocketFlags.None, EndRecvData, null);
        }

        private void SendExtShakeHand()
        {
            BDictionary supose = new BDictionary();
            supose.Add("ut_metadata", new BNumber(1));
            BDictionary dic = new BDictionary();
            dic.Add("m", supose);
            var datalist = new List<byte>();
            datalist.Add(0x14);
            datalist.Add(0x00);
            datalist.AddRange(dic.EncodeAsBytes());
            datalist.InsertRange(0, BitConverter.GetBytes((UInt32)datalist.Count).Reverse());
            fSocket.Send(datalist.ToArray());
        }

        private bool check()
        {
            if (fState == 0) {
                if (fDataBuffer[0] != 0x13)
                    throw new Exception();
                var str1 = Encoding.ASCII.GetString(fDataBuffer.ToArray(), 1, 19);
                if (str1 != "BitTorrent protocol")
                    throw new Exception();
                var reserved = fDataBuffer.Skip(20).Take(8).ToArray();
                var infoHash = fDataBuffer.Skip(28).Take(20).ToArray();
                if (infoHash != InfoHash) {
                    throw new Exception();
                }
                var peer_id = fDataBuffer.ToArray().Skip(48).Take(20);
                fState = 1;
                SendExtShakeHand();
                fDataBuffer.RemoveRange(0, 68);
                return true;
            } else if (fState == 1) {
                var lengthArray = fDataBuffer.Take(4);
                var len = BitConverter.ToUInt32(lengthArray.Reverse().ToArray(), 0);
                if (fDataBuffer.Count >= (4 + len)) {
                    var dataBuf = fDataBuffer.Skip(4).Take((int)len).ToArray();
                    var msgid = dataBuf[0];
                    if (msgid != 0x08)
                        throw new Exception();
                    var extendMsgID = dataBuf[1];
                    if (extendMsgID != 0x00)
                        throw new Exception();
                    BencodeParser parse = new BencodeParser();
                    var supose = parse.Parse<BDictionary>(dataBuf.Skip(2).ToArray());
                    if (!supose.ContainsKey("m"))
                        throw new Exception();
                    var suplist = supose.Get<BDictionary>("m");
                    if (!suplist.ContainsKey("ut_metadata"))
                        throw new Exception();
                    var numid = suplist.Get<BNumber>("ut_metadata");
                    var size = supose.Get<BNumber>("metadata_size");
                    var count = size / (16 * 1024) + (size % (16 * 1024) > 0 ? 1 : 0);
                    for (int i = 0; i < count; i++) {
                        BDictionary data = new BDictionary();
                        data.Add("msg_type", 0);
                        data.Add("prece", i);
                        var data2 = new List<byte>();
                        data2.Add(0x14);
                        data2.Add((byte)numid.Value);
                        data2.AddRange(data.EncodeAsBytes());
                        data2.AddRange(BitConverter.GetBytes((UInt32)data2.Count).Reverse());
                        fSocket.Send(data2.ToArray());
                    }
                    fState = 2;
                    return true;
                } else if (fState == 2) {

                }
            }
            return false;
        }
    }
}
