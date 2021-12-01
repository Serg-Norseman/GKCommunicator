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
    public sealed class DHTInfoHash : IEquatable<DHTInfoHash>, IComparable<DHTInfoHash>, IComparable
    {
        private static readonly Random random = new Random();

        private byte[] fData;

        public byte[] Data
        {
            get { return fData; }
        }

        public DHTInfoHash(byte[] data)
        {
            if (data == null || data.Length != 20)
                throw new ArgumentException("InfoHash must be exactly 20 bytes long");

            byte[] newData = new byte[20];
            Buffer.BlockCopy(data, 0, newData, 0, data.Length);
            fData = newData;
        }

        public DHTInfoHash(BString value)
            : this(value.Value)
        {
        }

        public static DHTInfoHash CreateRandom()
        {
            byte[] b = new byte[20];
            lock (random)
                random.NextBytes(b);
            return new DHTInfoHash(b);
        }

        public override int GetHashCode()
        {
            return fData.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DHTInfoHash);
        }

        public bool Equals(byte[] other)
        {
            return (other == null || other.Length != 20) ? false : Algorithms.ArraysEqual(fData, other);
        }

        public bool Equals(DHTInfoHash other)
        {
            return this == other;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as DHTInfoHash);
        }

        public int CompareTo(DHTInfoHash other)
        {
            if (other is null)
                return 1;

            for (int i = 0; i < fData.Length; i++) {
                if (fData[i] != other.fData[i])
                    return fData[i] - other.fData[i];
            }

            return 0;
        }

        public static bool operator ==(DHTInfoHash left, DHTInfoHash right)
        {
            if ((object)left == null)
                return (object)right == null;
            if ((object)right == null)
                return false;
            return Algorithms.ArraysEqual(left.Data, right.Data);
        }

        public static bool operator !=(DHTInfoHash left, DHTInfoHash right)
        {
            return !(left == right);
        }

        public byte[] ToArray()
        {
            return (byte[])fData.Clone();
        }

        public override string ToString()
        {
            return BitConverter.ToString(fData);
        }

        public BString ToBencodedString()
        {
            return new BString((byte[])fData.Clone());
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

        public static DHTInfoHash FromHex(string infoHash)
        {
            if (infoHash == null || infoHash.Length != 40)
                throw new ArgumentException("InfoHash must be 40 characters long");

            byte[] hash = new byte[20];
            for (int i = 0; i < hash.Length; i++)
                hash[i] = byte.Parse(infoHash.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);

            return new DHTInfoHash(hash);
        }
    }
}
