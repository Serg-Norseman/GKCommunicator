/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Text;
using BencodeNET;
using BSLib;

namespace GKNet.DHT
{
    public class DHTNodeId : IEquatable<DHTNodeId>, IComparable<DHTNodeId>, IComparable
    {
        private static readonly Random random = new Random();

        private byte[] fData;

        public byte[] Data
        {
            get { return fData; }
        }

        public DHTNodeId(byte[] data)
        {
            if (data == null || data.Length != 20)
                throw new ArgumentException("NodeId must be exactly 20 bytes long");

            byte[] newData = new byte[20];
            Buffer.BlockCopy(data, 0, newData, 0, data.Length);
            fData = newData;
        }

        public DHTNodeId(DHTInfoHash infoHash)
            : this(infoHash.ToArray())
        {
        }

        public DHTNodeId(BString value)
            : this(value.Value)
        {
        }

        public static DHTNodeId Create()
        {
            byte[] b = new byte[20];
            lock (random)
                random.NextBytes(b);
            return new DHTNodeId(b);
        }

        public override int GetHashCode()
        {
            return fData.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DHTNodeId);
        }

        public bool Equals(DHTNodeId other)
        {
            if ((object)other == null)
                return false;

            return Algorithms.ArraysEqual(fData, other.fData);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as DHTNodeId);
        }

        public int CompareTo(DHTNodeId other)
        {
            if ((object)other == null)
                return 1;

            byte[] x = fData;
            byte[] y = other.fData;

            if (x.Length != y.Length) {
                return x.Length > y.Length ? -1 : 1;
            }
            var length = Math.Min(x.Length, y.Length);
            for (var i = 0; i < length; i++) {
                if (x[i] == y[i])
                    continue;
                return x[i] > y[i] ? -1 : 1;
            }
            return 1;
        }

        public static bool operator ==(DHTNodeId first, DHTNodeId second)
        {
            if ((object)first == null)
                return (object)second == null;
            if ((object)second == null)
                return false;
            return Algorithms.ArraysEqual(first.fData, second.fData);
        }

        public static bool operator !=(DHTNodeId first, DHTNodeId second)
        {
            return !(first == second);
        }

        public override string ToString()
        {
            return ToHex();
        }

        public BString ToBencodedString()
        {
            return new BString(fData);
        }

        public string ToHex()
        {
            StringBuilder sb = new StringBuilder(40);
            for (int i = 0; i < fData.Length; i++) {
                string hex = fData[i].ToString("X");
                if (hex.Length != 2)
                    sb.Append("0");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}
