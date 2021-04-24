namespace BencodeNET
{
    /// <summary>
    /// Represents parse errors for when encountering bencode that is potentially valid but not supported by this library.
    /// Usually numbers larger than <see cref="long.MaxValue"/> or strings longer than that.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnsupportedBencodeException<T> : BencodeException<T>
    {
        public long StreamPosition { get; set; }

        public UnsupportedBencodeException(string message, long streamPosition) : base(message)
        {
            StreamPosition = streamPosition;
        }
    }
}
