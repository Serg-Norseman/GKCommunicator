using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.Objects;

namespace BencodeNET.Parsing
{
    /// <summary>
    /// A parser for bencoded byte strings.
    /// </summary>
    public class BStringParser : BObjectParser<BString>
    {
        /// <summary>
        /// The minimum stream length in bytes for a valid string ('0:').
        /// </summary>
        protected const int MinimumLength = 2;
        private readonly Encoding fEncoding;

        /// <summary>
        /// Creates an instance using <see cref="System.Text.Encoding.UTF8"/> for parsing.
        /// </summary>
        public BStringParser()
            : this(Encoding.UTF8)
        { }

        /// <summary>
        /// Creates an instance using the specified encoding for parsing.
        /// </summary>
        /// <param name="encoding"></param>
        public BStringParser(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException("encoding");

            fEncoding = encoding;
        }

        /// <summary>
        /// The encoding used when creating the <see cref="BString"/> when parsing.
        /// </summary>
        protected override Encoding Encoding
        {
            get {
                return fEncoding;
            }
        }

        /// <summary>
        /// Parses the next <see cref="BString"/> from the stream.
        /// </summary>
        /// <param name="stream">The stream to parse from.</param>
        /// <returns>The parsed <see cref="BString"/>.</returns>
        /// <exception cref="InvalidBencodeException{BString}">Invalid bencode</exception>
        /// <exception cref="UnsupportedBencodeException{BString}">The bencode is unsupported by this library</exception>
        public override BString Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < MinimumLength)
                throw InvalidBencodeException<BString>.BelowMinimumLength(MinimumLength, stream.Length, stream.Position);

            var startPosition = stream.Position;

            var lengthString = new StringBuilder();
            for (var c = stream.ReadChar(); c != ':' && c != default(char); c = stream.ReadChar()) {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthString.Length >= BString.LengthMaxDigits) {
                    throw UnsupportedException(
                        string.Format("Length of string is more than {0} digits (>10GB) and is not supported (max is ~1-2GB).", BString.LengthMaxDigits),
                        startPosition);
                }

                lengthString.Append(c);
            }

            long stringLength;
            if (!ParseUtil.TryParseLongFast(lengthString.ToString(), out stringLength))
                throw InvalidException(string.Format("Invalid length '{0}' of string.", lengthString), startPosition);

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue) {
                throw UnsupportedException(
                    string.Format("Length of string is {0:N0} but maximum supported length is {1:N0}.", stringLength, int.MaxValue),
                    startPosition);
            }

            var bytes = stream.Read((int)stringLength);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Length != stringLength) {
                throw InvalidException(
                    string.Format("Expected string to be {0:N0} bytes long but could only read {1:N0} bytes.", stringLength, bytes.Length),
                    startPosition);
            }

            return new BString(bytes, Encoding);
        }

        private static InvalidBencodeException<BString> InvalidException(string message, long startPosition)
        {
            return new InvalidBencodeException<BString>(
                string.Format("{0} The string starts at position {1}.", message, startPosition),
                startPosition);
        }

        private static UnsupportedBencodeException<BString> UnsupportedException(string message, long startPosition)
        {
            return new UnsupportedBencodeException<BString>(
                string.Format("{0} The string starts at position {1}.", message, startPosition),
                startPosition);
        }
    }
}
