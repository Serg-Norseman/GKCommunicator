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
using System.Globalization;
using System.Text;
using BencodeNET;
using BSLib;

namespace GKNet.DHT
{
    public sealed class DHTId : IEquatable<DHTId>, IComparable<DHTId>, IComparable
    {
        private static readonly Random random = new Random();

        private byte[] fData;

        public byte[] Data
        {
            get { return fData; }
        }

        public DHTId(byte[] data)
        {
            if (data == null || data.Length != 20)
                throw new ArgumentException("Id must be exactly 20 bytes long");

            fData = data;
        }

        public DHTId(BString value)
            : this(value.Value)
        {
        }

        public static DHTId CreateRandom()
        {
            byte[] b = new byte[20];
            lock (random)
                random.NextBytes(b);
            return new DHTId(b);
        }

        public override int GetHashCode()
        {
            return fData.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DHTId);
        }

        public bool Equals(byte[] other)
        {
            return (other == null || other.Length != 20) ? false : Algorithms.ArraysEqual(fData, other);
        }

        public bool Equals(DHTId other)
        {
            if ((object)other == null)
                return false;

            return Algorithms.ArraysEqual(fData, other.fData);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as DHTId);
        }

        public int CompareTo(DHTId other)
        {
            if ((object)other == null)
                return 1;

            byte[] x = fData;
            byte[] y = other.fData;

            if (x.Length != y.Length) {
                return x.Length > y.Length ? +1 : -1;
            }

            var length = Math.Min(x.Length, y.Length);

            for (var i = 0; i < length; i++) {
                if (x[i] != y[i]) {
                    return (x[i] > y[i]) ? +1 : -1;
                }
            }

            return 0;
        }

        public static bool operator ==(DHTId left, DHTId right)
        {
            if ((object)left == null)
                return (object)right == null;
            if ((object)right == null)
                return false;
            return Algorithms.ArraysEqual(left.Data, right.Data);
        }

        public static bool operator !=(DHTId left, DHTId right)
        {
            return !(left == right);
        }

        public byte[] ToArray()
        {
            return (byte[])fData.Clone();
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

        public static DHTId FromHex(string hex)
        {
            if (hex == null || hex.Length != 40)
                throw new ArgumentException("Id must be 40 characters long");

            byte[] data = new byte[20];
            for (int i = 0; i < data.Length; i++)
                data[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);

            return new DHTId(data);
        }
    }
}
