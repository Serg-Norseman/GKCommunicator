using BencodeNET.Objects;
using BencodeNET.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DHTConnector
{
    public class BittorrentDownloader
    {
        public IPEndPoint Ipaddress = null;
        public byte[] InfoHash = null;
        public byte[] NodeID = null;
        Socket sock = null;
        byte[] buffer = new byte[4096];
        List<byte> DataBuffer = new List<byte>();
        int state = 0;
        private List<Tuple<int, byte[]>> TorList = new List<Tuple<int, byte[]>>();

        public BittorrentDownloader(IPEndPoint ipinfo, byte[] infohash, byte[] nodeid)
        {
            this.Ipaddress = ipinfo;
            this.InfoHash = infohash;
            this.NodeID = nodeid;
        }

        public void Run()
        {
            this.sock.BeginConnect(this.Ipaddress, new AsyncCallback(EndConnect), null);
        }

        private void SendShakeHand()
        {
            var peer = Helpers.GetRandomID();
            var list = new List<byte>();
            list.Add(0x13);
            list.AddRange(Encoding.ASCII.GetBytes("BitTorrent protocol"));
            list.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00 });
            list.AddRange(this.InfoHash);
            list.AddRange(peer);
            this.sock.Send(list.ToArray(), SocketFlags.None);
        }

        private void EndConnect(IAsyncResult result)
        {
            try {
                this.sock.EndConnect(result);
                this.sock.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(EndRecvData), null);
                this.SendShakeHand();
            } catch {

            }
        }

        private void EndRecvData(IAsyncResult result)
        {
            var recvCount = this.sock.EndReceive(result);
            this.DataBuffer.AddRange(buffer.Take(recvCount));
            check();
            if (this.state == 1) {

            }
            this.sock.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, EndRecvData, null);
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
            this.sock.Send(datalist.ToArray());
        }

        private bool check()
        {
            if (this.state == 0) {
                if (this.DataBuffer[0] != 0x13)
                    throw new Exception();
                var str1 = Encoding.ASCII.GetString(this.DataBuffer.ToArray(), 1, 19);
                if (str1 != "BitTorrent protocol")
                    throw new Exception();
                var reserved = this.DataBuffer.Skip(20).Take(8).ToArray();
                var infoHash = this.DataBuffer.Skip(28).Take(20).ToArray();
                if (infoHash != this.InfoHash) {
                    throw new Exception();
                }
                var peer_id = this.DataBuffer.ToArray().Skip(48).Take(20);
                this.state = 1;
                SendExtShakeHand();
                this.DataBuffer.RemoveRange(0, 68);
                return true;
            } else if (this.state == 1) {
                var lengthArray = this.DataBuffer.Take(4);
                var len = BitConverter.ToUInt32(lengthArray.Reverse().ToArray(), 0);
                if (this.DataBuffer.Count >= (4 + len)) {
                    var dataBuf = this.DataBuffer.Skip(4).Take((int)len).ToArray();
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
                        this.sock.Send(data2.ToArray());
                    }
                    this.state = 2;
                    return true;
                } else if (this.state == 2) {

                }
            }
            return false;
        }
    }
}
