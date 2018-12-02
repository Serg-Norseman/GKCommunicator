using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BencodeNET
{
    /// <summary>
    /// Represents a bencoded string, i.e. a byte-string.
    /// It isn't necessarily human-readable.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="byte"/> array.
    /// </remarks>
    public sealed class BString : BObject<byte[]>, IComparable<BString>, IEquatable<BString>
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// The maximum number of digits that can be handled as the length part of a bencoded string.
        /// </summary>
        internal const int LengthMaxDigits = 10;

        private Encoding fEncoding;
        private readonly byte[] fValue;

        /// <summary>
        /// The underlying bytes of the string.
        /// </summary>
        public override byte[] Value
        {
            get { return fValue; }
        }

        /// <summary>
        /// Gets the length of the string in bytes.
        /// </summary>
        public int Length
        {
            get { return fValue.Length; }
        }

        /// <summary>
        /// Gets or sets the encoding used as the default with <c>ToString()</c>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Encoding Encoding
        {
            get { return fEncoding; }
            set { fEncoding = value ?? DefaultEncoding; }
        }

        /// <summary>
        /// Creates a <see cref="BString"/> from bytes with the specified encoding.
        /// </summary>
        /// <param name="bytes">The bytes representing the data.</param>
        /// <param name="encoding">The encoding of the bytes. Defaults to <see cref="System.Text.Encoding.UTF8"/>.</param>
        public BString(IEnumerable<byte> bytes, Encoding encoding = null)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");

            fEncoding = encoding ?? DefaultEncoding;
            fValue = bytes as byte[] ?? bytes.ToArray();
        }

        /// <summary>
        /// Creates a <see cref="BString"/> using the specified encoding to convert the string to bytes.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="encoding">The encoding used to convert the string to bytes.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BString(string str, Encoding encoding = null)
        {
            if (str == null) throw new ArgumentNullException("str");

            fEncoding = encoding ?? DefaultEncoding;
            fValue = fEncoding.GetBytes(str);
        }

        /// <summary>
        /// Encodes this byte-string as bencode and returns the encoded string.
        /// Uses the current value of the <see cref="Encoding"/> property.
        /// </summary>
        /// <returns>
        /// This byte-string as a bencoded string.
        /// </returns>
        public override string EncodeAsString()
        {
            return EncodeAsString(fEncoding);
        }

        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write(fValue.Length);
            stream.Write(':');
            stream.Write(fValue);
        }

        public static implicit operator BString(string value)
        {
            return new BString(value);
        }

        public static bool operator ==(BString first, BString second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(BString first, BString second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            BString bstr = obj as BString;
            if (bstr != null)
                return fValue.SequenceEqual(bstr.fValue);

            return false;
        }

        public bool Equals(BString other)
        {
            return (other != null) && fValue.SequenceEqual(other.fValue);
        }

        public override int GetHashCode()
        {
            var bytesToHash = Math.Min(fValue.Length, 32);

            long hashValue = 0;
            for (var i = 0; i < bytesToHash; i++) {
                hashValue = (37 * hashValue + fValue[i]) % int.MaxValue;
            }

            return (int)hashValue;
        }

        public int CompareTo(BString other)
        {
            if (other == null)
                return 1;

            var maxLength = Math.Max(this.Length, other.Length);

            for (var i = 0; i < maxLength; i++) {
                // This is shorter and thereby this is "less than" the other
                if (i >= this.Length)
                    return -1;

                // The other is shorter and thereby this is "greater than" the other
                if (i >= other.Length)
                    return 1;

                if (this.fValue[i] > other.fValue[i])
                    return 1;

                if (this.fValue[i] < other.fValue[i])
                    return -1;
            }

            return 0;
        }

        /// <summary>
        /// Converts the underlying bytes to a string representation using the current value of the <see cref="Encoding"/> property.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return fEncoding.GetString(fValue);
        }

        /// <summary>
        /// Converts the underlying bytes to a string representation using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use to convert the underlying byte array to a <see cref="System.String" />.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(Encoding encoding)
        {
            encoding = encoding ?? fEncoding;
            return encoding.GetString(fValue);
        }
    }
}
