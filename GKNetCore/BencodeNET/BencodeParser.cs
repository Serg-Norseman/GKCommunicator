using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BencodeNET
{
    /// <summary>
    /// Main class used for parsing bencode.
    /// </summary>
    public class BencodeParser
    {
        private delegate IBObject ParseProc(BencodeStream stream);

        private readonly Dictionary<Type, ParseProc> fParsers;

        /// <summary>
        /// The encoding use for parsing.
        /// </summary>
        public Encoding Encoding { get; protected set; }

        /// <summary>
        /// Creates an instance using <see cref="System.Text.Encoding.UTF8"/> and the default parsers.
        /// </summary>
        public BencodeParser() : this(Encoding.UTF8)
        {
        }

        /// <summary>
        /// Creates an instance using the specified encoding and the default parsers.
        /// </summary>
        /// <param name="encoding">The encoding to use when parsing.</param>
        public BencodeParser(Encoding encoding)
        {
            Encoding = encoding;

            fParsers = new Dictionary<Type, ParseProc>() {
                { typeof(BString), ParseStr },
                { typeof(BNumber), ParseNumber },
                { typeof(BList), ParseList },
                { typeof(BDictionary), ParseDict }
            };
        }

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public IBObject Parse(string bencodedString)
        {
            using (var stream = bencodedString.AsStream(Encoding)) {
                return Parse(stream);
            }
        }

        /// <summary>
        /// Parses a bencoded array of bytes into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="bytes">The bencoded bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public IBObject Parse(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes)) {
                return Parse(stream);
            }
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public IBObject Parse(Stream stream)
        {
            return Parse(new BencodeStream(stream));
        }

        /// <summary>
        /// Parses a <see cref="BencodeStream"/> into an <see cref="IBObject"/>.
        /// </summary>
        /// <param name="stream">The stream to parse.</param>
        /// <returns>The parsed object.</returns>
        public IBObject Parse(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            switch (stream.PeekChar()) {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return Parse<BString>(stream);
                case 'i': return Parse<BNumber>(stream);
                case 'l': return Parse<BList>(stream);
                case 'd': return Parse<BDictionary>(stream);
            }

            throw InvalidBencodeException<IBObject>.InvalidBeginningChar(stream.PeekChar(), stream.Position);
        }

        /// <summary>
        /// Parses a bencoded string into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="bencodedString">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public T Parse<T>(string bencodedString) where T : class, IBObject
        {
            using (var stream = bencodedString.AsStream(Encoding)) {
                return Parse<T>(stream);
            }
        }

        /// <summary>
        /// Parses a bencoded array of bytes into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="bytes">The bencoded bytes to parse.</param>
        /// <returns>The parsed object.</returns>
        public T Parse<T>(byte[] bytes) where T : class, IBObject
        {
            using (var stream = new MemoryStream(bytes)) {
                return Parse<T>(stream);
            }
        }

        /// <summary>
        /// Parses a stream into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="stream">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public T Parse<T>(Stream stream) where T : class, IBObject
        {
            return Parse<T>(new BencodeStream(stream));
        }

        /// <summary>
        /// Parses a <see cref="BencodeStream"/> into an <see cref="IBObject"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBObject"/> to parse as.</typeparam>
        /// <param name="stream">The bencoded string to parse.</param>
        /// <returns>The parsed object.</returns>
        public T Parse<T>(BencodeStream stream) where T : class, IBObject
        {
            Type type = typeof(T);
            var parseProc = fParsers.GetValueOrDefault(type);

            if (parseProc == null)
                throw new BencodeException(string.Format("Missing parser for the type '{0}'. Stream position: {1}", typeof(T).FullName, stream.Position));

            return parseProc(stream) as T;
        }

        #region Parser for bencoded dictionaries

        /// <summary>
        /// The minimum stream length in bytes for a valid dictionary ('de').
        /// </summary>
        protected const int MinimumDictLength = 2;

        /// <summary>
        /// Parses the next <see cref="BDictionary"/> from the stream and its contained keys and values.
        /// </summary>
        /// <param name="stream">The stream to parse from.</param>
        /// <returns>The parsed <see cref="BDictionary"/>.</returns>
        /// <exception cref="InvalidBencodeException{BDictionary}">Invalid bencode</exception>
        protected BDictionary ParseDict(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var startPosition = stream.Position;

            if (stream.Length < MinimumDictLength)
                throw InvalidBencodeException<BDictionary>.BelowMinimumLength(MinimumDictLength, stream.Length, startPosition);

            // Dictionaries must start with 'd'
            char c = stream.ReadChar();
            if (c != 'd')
                throw InvalidBencodeException<BDictionary>.UnexpectedChar('d', c, startPosition);

            var dictionary = new BDictionary();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1) {
                BString key;
                try {
                    // Decode next string in stream as the key
                    key = ParseStr(stream);
                } catch (BencodeException<BString> ex) {
                    throw InvalidDictException("Could not parse dictionary key. Keys must be strings.", ex, startPosition);
                }

                IBObject value;
                try {
                    // Decode next object in stream as the value
                    value = Parse(stream);
                } catch (BencodeException ex) {
                    throw InvalidDictException(
                        string.Format("Could not parse dictionary value for the key '{0}'. There needs to be a value for each key.", key),
                        ex, startPosition);
                }

                if (dictionary.ContainsKey(key)) {
                    throw InvalidDictException(
                        string.Format("The dictionary already contains the key '{0}'. Duplicate keys are not supported.", key), startPosition);
                }

                dictionary.Add(key, value);
            }

            c = stream.ReadChar();
            if (c != 'e') {
                if (stream.EndOfStream) throw InvalidBencodeException<BDictionary>.MissingEndChar();
                throw InvalidBencodeException<BDictionary>.InvalidEndChar(c, stream.Position);
            }

            return dictionary;
        }

        private static InvalidBencodeException<BDictionary> InvalidDictException(string message, long startPosition)
        {
            return new InvalidBencodeException<BDictionary>(
                string.Format("{0} The dictionary starts at position {1}.", message, startPosition), startPosition);
        }

        private static InvalidBencodeException<BDictionary> InvalidDictException(string message, Exception inner, long startPosition)
        {
            return new InvalidBencodeException<BDictionary>(
                string.Format("{0} The dictionary starts at position {1}.", message, startPosition), inner, startPosition);
        }

        #endregion

        #region Parser for bencoded byte strings

        /// <summary>
        /// The minimum stream length in bytes for a valid string ('0:').
        /// </summary>
        protected const int MinimumStrLength = 2;

        /// <summary>
        /// Parses the next <see cref="BString"/> from the stream.
        /// </summary>
        /// <param name="stream">The stream to parse from.</param>
        /// <returns>The parsed <see cref="BString"/>.</returns>
        /// <exception cref="InvalidBencodeException{BString}">Invalid bencode</exception>
        /// <exception cref="UnsupportedBencodeException{BString}">The bencode is unsupported by this library</exception>
        protected BString ParseStr(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            // Minimum valid bencode string is '0:' meaning an empty string
            if (stream.Length < MinimumStrLength)
                throw InvalidBencodeException<BString>.BelowMinimumLength(MinimumStrLength, stream.Length, stream.Position);

            var startPosition = stream.Position;

            var lengthString = new StringBuilder();
            for (var c = stream.ReadChar(); c != ':' && c != default(char); c = stream.ReadChar()) {
                // Because of memory limitations (~1-2 GB) we know for certain we cannot handle more than 10 digits (10GB)
                if (lengthString.Length >= BString.LengthMaxDigits) {
                    throw UnsupportedStrException(
                        string.Format("Length of string is more than {0} digits (>10GB) and is not supported (max is ~1-2GB).", BString.LengthMaxDigits),
                        startPosition);
                }

                lengthString.Append(c);
            }

            long stringLength;
            if (!TryParseLongFast(lengthString.ToString(), out stringLength))
                throw InvalidStrException(string.Format("Invalid length '{0}' of string.", lengthString), startPosition);

            // Int32.MaxValue is ~2GB and is the absolute maximum that can be handled in memory
            if (stringLength > int.MaxValue) {
                throw UnsupportedStrException(
                    string.Format("Length of string is {0:N0} but maximum supported length is {1:N0}.", stringLength, int.MaxValue),
                    startPosition);
            }

            var bytes = stream.Read((int)stringLength);

            // If the two don't match we've reached the end of the stream before reading the expected number of chars
            if (bytes.Length != stringLength) {
                throw InvalidStrException(
                    string.Format("Expected string to be {0:N0} bytes long but could only read {1:N0} bytes.", stringLength, bytes.Length),
                    startPosition);
            }

            return new BString(bytes, Encoding);
        }

        private static InvalidBencodeException<BString> InvalidStrException(string message, long startPosition)
        {
            return new InvalidBencodeException<BString>(
                string.Format("{0} The string starts at position {1}.", message, startPosition), startPosition);
        }

        private static UnsupportedBencodeException<BString> UnsupportedStrException(string message, long startPosition)
        {
            return new UnsupportedBencodeException<BString>(
                string.Format("{0} The string starts at position {1}.", message, startPosition), startPosition);
        }

        #endregion

        #region Parser for bencoded numbers

        /// <summary>
        /// The minimum stream length in bytes for a valid number ('i0e').
        /// </summary>
        protected const int MinimumNumberLength = 3;

        /// <summary>
        /// Parses the next <see cref="BNumber"/> from the stream.
        /// </summary>
        /// <param name="stream">The stream to parse from.</param>
        /// <returns>The parsed <see cref="BNumber"/>.</returns>
        /// <exception cref="InvalidBencodeException{BNumber}">Invalid bencode</exception>
        /// <exception cref="UnsupportedBencodeException{BNumber}">The bencode is unsupported by this library</exception>
        protected BNumber ParseNumber(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            if (stream.Length < MinimumNumberLength)
                throw InvalidBencodeException<BNumber>.BelowMinimumLength(MinimumNumberLength, stream.Length, stream.Position);

            var startPosition = stream.Position;

            // Numbers must start with 'i'
            char c = stream.ReadChar();
            if (c != 'i')
                throw InvalidBencodeException<BNumber>.UnexpectedChar('i', c, stream.Position);

            var digits = new StringBuilder();
            for (c = stream.ReadChar(); c != 'e' && c != default(char); c = stream.ReadChar()) {
                digits.Append(c);
            }

            // Last read character should be 'e'
            if (c != 'e') {
                if (stream.EndOfStream) throw InvalidBencodeException<BNumber>.MissingEndChar();
                throw InvalidBencodeException<BNumber>.InvalidEndChar(c, stream.Position);
            }

            var isNegative = digits[0] == '-';
            var numberOfDigits = isNegative ? digits.Length - 1 : digits.Length;

            // We do not support numbers that cannot be stored as a long (Int64)
            if (numberOfDigits > BNumber.MaxDigits) {
                throw UnsupportedNumException(
                    string.Format("The number '{0}' has more than 19 digits and cannot be stored as a long (Int64) and therefore is not supported.", digits),
                    startPosition);
            }

            // We need at least one digit
            if (numberOfDigits < 1)
                throw InvalidNumException("It contains no digits.", startPosition);

            var firstDigit = isNegative ? digits[1] : digits[0];

            // Leading zeros are not valid
            if (firstDigit == '0' && numberOfDigits > 1)
                throw InvalidNumException(string.Format("Leading '0's are not valid. Found value '{0}'.", digits), startPosition);

            // '-0' is not valid either
            if (firstDigit == '0' && numberOfDigits == 1 && isNegative)
                throw InvalidNumException("'-0' is not a valid number.", startPosition);

            long number;
            if (!TryParseLongFast(digits.ToString(), out number)) {
                var nonSignChars = isNegative ? digits.ToString(1, digits.Length - 1) : digits.ToString();
                if (nonSignChars.Any(x => !x.IsDigit()))
                    throw InvalidNumException(string.Format("The value '{0}' is not a valid number.", digits), startPosition);

                throw UnsupportedNumException(
                    string.Format("The value '{0}' is not a valid long (Int64). Supported values range from '{1:N0}' to '{2:N0}'.", digits, long.MinValue, long.MaxValue),
                    startPosition);
            }

            return new BNumber(number);
        }

        private static InvalidBencodeException<BNumber> InvalidNumException(string message, long startPosition)
        {
            return new InvalidBencodeException<BNumber>(
                string.Format("{0} The number starts at position {1}.", message, startPosition), startPosition);
        }

        private static UnsupportedBencodeException<BNumber> UnsupportedNumException(string message, long startPosition)
        {
            return new UnsupportedBencodeException<BNumber>(
                string.Format("{0} The number starts at position {1}.", message, startPosition), startPosition);
        }

        #endregion

        #region Parser for bencoded lists

        /// <summary>
        /// The minimum stream length in bytes for a valid list ('le').
        /// </summary>
        protected const int MinimumListLength = 2;

        /// <summary>
        /// Parses the next <see cref="BList"/> from the stream.
        /// </summary>
        /// <param name="stream">The stream to parse from.</param>
        /// <returns>The parsed <see cref="BList"/>.</returns>
        /// <exception cref="InvalidBencodeException{BList}">Invalid bencode</exception>
        protected BList ParseList(BencodeStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            if (stream.Length < MinimumListLength)
                throw InvalidBencodeException<BList>.BelowMinimumLength(MinimumListLength, stream.Length, stream.Position);

            // Lists must start with 'l'
            char c = stream.ReadChar();
            if (c != 'l')
                throw InvalidBencodeException<BList>.UnexpectedChar('l', c, stream.Position);

            var list = new BList();
            // Loop until next character is the end character 'e' or end of stream
            while (stream.Peek() != 'e' && stream.Peek() != -1) {
                // Decode next object in stream
                var bObject = Parse(stream);
                list.Add(bObject);
            }

            c = stream.ReadChar();
            if (c != 'e') {
                if (stream.EndOfStream) throw InvalidBencodeException<BList>.MissingEndChar();
                throw InvalidBencodeException<BList>.InvalidEndChar(c, stream.Position);
            }

            return list;
        }

        #endregion

        #region Helper methods

        private const int Int64MaxDigits = 19;

        /// <summary>
        /// A faster implementation than <see cref="long.TryParse(string, out long)"/>
        /// because we skip some checks that are not needed.
        /// </summary>
        protected static bool TryParseLongFast(string value, out long result)
        {
            result = 0;

            if (value == null)
                return false;

            var length = value.Length;

            // Cannot parse empty string
            if (length == 0)
                return false;

            var isNegative = value[0] == '-';
            var startIndex = isNegative ? 1 : 0;

            // Cannot parse just '-'
            if (isNegative && length == 1)
                return false;

            // Cannot parse string longer than long.MaxValue
            if (length - startIndex > Int64MaxDigits)
                return false;

            long parsedLong = 0;
            for (var i = startIndex; i < length; i++)
            {
                var character = value[i];
                if (!character.IsDigit())
                    return false;

                var digit = character - '0';

                if (isNegative)
                    parsedLong = 10 * parsedLong - digit;
                else
                    parsedLong = 10 * parsedLong + digit;
            }

            // Negative - should be less than zero (Int64.MinValue overflow)
            if (isNegative && parsedLong >= 0)
                return false;

            // Positive - should be equal to or greater than zero (Int64.MaxValue overflow)
            if (!isNegative && parsedLong < 0)
                return false;

            result = parsedLong;
            return true;
        }

        #endregion
    }
}
