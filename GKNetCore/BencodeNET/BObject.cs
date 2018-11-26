using System.IO;
using System.Text;

namespace BencodeNET
{
    /// <summary>
    /// Abstract base class with default implementation of most methods of <see cref="IBObject"/>.
    /// </summary>
    public abstract class BObject : IBObject
    {
        protected BObject()
        {
        }

        /// <summary>
        /// Encodes the object and returns the result as a string using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <returns>
        /// The object bencoded and converted to a string using <see cref="Encoding.UTF8"/>.
        /// </returns>
        public virtual string EncodeAsString()
        {
            return EncodeAsString(Encoding.UTF8);
        }

        /// <summary>
        /// Encodes the object and returns the result as a string using the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding used to convert the encoded bytes to a string.</param>
        /// <returns>
        /// The object bencoded and converted to a string using the specified encoding.
        /// </returns>
        public virtual string EncodeAsString(Encoding encoding)
        {
            using (var stream = EncodeTo(new MemoryStream()))
            {
                return encoding.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Encodes the object and returns the raw bytes.
        /// </summary>
        /// <returns>The raw bytes of the bencoded object.</returns>
        public virtual byte[] EncodeAsBytes()
        {
            using (var stream = new MemoryStream())
            {
                EncodeTo(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <typeparam name="TStream">The type of stream.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        public TStream EncodeTo<TStream>(TStream stream) where TStream : Stream
        {
            EncodeObject(new BencodeStream(stream));
            return stream;
        }

        /// <summary>
        /// Writes the object as bencode to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <returns>The used stream.</returns>
        public BencodeStream EncodeTo(BencodeStream stream)
        {
            EncodeObject(stream);
            return stream;
        }

        /// <summary>
        /// Implementations of this method should encode their
        /// underlying value to bencode and write it to the stream.
        /// </summary>
        /// <param name="stream">The stream to encode to.</param>
        protected abstract void EncodeObject(BencodeStream stream);
    }

    /// <summary>
    /// Base class of bencode objects with a specific underlying value type.
    /// </summary>
    /// <typeparam name="T">Type of the underlying value.</typeparam>
    public abstract class BObject<T> : BObject
    {
        protected BObject()
        {
        }

        /// <summary>
        /// The underlying value of the <see cref="BObject{T}"/>.
        /// </summary>
        public abstract T Value { get; }
    }
}
