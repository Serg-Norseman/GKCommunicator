using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif

#pragma warning disable 1591
namespace BencodeNET.Exceptions
{
    /// <summary>
    /// Represents parse errors when encountering invalid bencode of some sort.
    /// </summary>
    /// <typeparam name="T">The type being parsed.</typeparam>
#if !NETSTANDARD
    [Serializable]
#endif
    public class InvalidBencodeException<T> : BencodeException<T>
    {
        /// <summary>
        /// The position in the stream where the error happened or
        /// the starting position of the parsed object that caused the error.
        /// </summary>
        public long StreamPosition { get; set; }

        public InvalidBencodeException()
        { }

        public InvalidBencodeException(string message)
            : base(message)
        { }

        public InvalidBencodeException(string message, Exception inner)
            : base(message, inner)
        { }

        public InvalidBencodeException(string message, Exception inner, long streamPosition)
            : base(string.Format("Failed to parse {0}. {1}", typeof(T).Name, message), inner)
        {
            StreamPosition = streamPosition;
        }

        public InvalidBencodeException(string message, long streamPosition)
            : base(string.Format("Failed to parse {0}. {1}", typeof(T).Name, message))
        {
            StreamPosition = streamPosition;
        }

#if !NETSTANDARD
        protected InvalidBencodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null) return;
            StreamPosition = info.GetInt64("StreamPosition");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("StreamPosition", StreamPosition);
        }
#endif

        internal static InvalidBencodeException<T> InvalidBeginningChar(char invalidChar, long streamPosition)
        {
            var message =
                string.Format("Invalid beginning character of object. Found '{0}' at position {1}. Valid characters are: 0-9, 'i', 'l' and 'd'", invalidChar, streamPosition);
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        internal static InvalidBencodeException<T> InvalidEndChar(char invalidChar, long streamPosition)
        {
            var message =
                string.Format("Invalid end character of object. Expected 'e' but found '{0}' at position {1}.", invalidChar, streamPosition);
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        internal static InvalidBencodeException<T> MissingEndChar()
        {
            var message = "Missing end character of object. Expected 'e' but reached the end of the stream.";
            return new InvalidBencodeException<T>(message);
        }

        internal static InvalidBencodeException<T> BelowMinimumLength(int minimumLength, long actualLength, long streamPosition)
        {
            var message =
                string.Format("Invalid length. Minimum valid stream length for parsing '{0}' is {1} but the actual length was only {2}.", typeof (T).FullName, minimumLength, actualLength);
            return new InvalidBencodeException<T>(message, streamPosition);
        }

        internal static InvalidBencodeException<T> UnexpectedChar(char expected, char unexpected, long streamPosition)
        {
            var message = string.Format("Unexpected character. Expected '{0}' but found '{1}' at position {2}.", expected, unexpected, streamPosition);
            return new InvalidBencodeException<T>(message, streamPosition);
        }
    }
}
