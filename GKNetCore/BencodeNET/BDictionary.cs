using System;
using System.Collections;
using System.Collections.Generic;

namespace BencodeNET
{
    /// <summary>
    /// Represents a bencoded dictionary of <see cref="BString"/> keys and <see cref="IBObject"/> values.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="IDictionary{BString,IBObject}"/>.
    /// </remarks>
    public sealed class BDictionary : BObject<IDictionary<BString, IBObject>>, IDictionary<BString, IBObject>
    {
        private readonly IDictionary<BString, IBObject> fValue;

        /// <summary>
        /// The underlying dictionary.
        /// </summary>
        public override IDictionary<BString, IBObject> Value
        {
            get { return fValue; }
        }

        /// <summary>
        /// Creates an empty dictionary.
        /// </summary>
        public BDictionary()
        {
            fValue = new SortedDictionary<BString, IBObject>();
        }

        /// <summary>
        /// Creates a dictionary with an initial value of the supplied dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public BDictionary(IDictionary<BString, IBObject> dictionary)
        {
            fValue = dictionary;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary as <see cref="BString"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value)
        {
            Add(new BString(key), new BString(value));
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary as <see cref="BNumber"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, long value)
        {
            Add(new BString(key), new BNumber(value));
        }

        /// <summary>
        /// Gets the value associated with the specified key and casts it as <typeparamref name="T"/>.
        /// If the key does not exist or the value is not of the specified type null is returned.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to.</typeparam>
        /// <param name="key">The key to get the associated value of.</param>
        /// <returns>The associated value of the specified key or null if the key does not exist.
        /// If the value is not of the specified type null is returned as well.</returns>
        public T Get<T>(BString key) where T : class, IBObject
        {
            return this[key] as T;
        }

        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write('d');
            foreach (var kvPair in this) {
                kvPair.Key.EncodeTo(stream);
                kvPair.Value.EncodeTo(stream);
            }
            stream.Write('e');
        }

        #region IDictionary<BString, IBObject> Members

        public ICollection<BString> Keys
        {
            get { return fValue.Keys; }
        }

        public ICollection<IBObject> Values
        {
            get { return fValue.Values; }
        }

        public int Count
        {
            get { return fValue.Count; }
        }

        public bool IsReadOnly
        {
            get { return fValue.IsReadOnly; }
        }

        /// <summary>
        /// Returns the value associated with the key or null if the key doesn't exist.
        /// </summary>
        public IBObject this[BString key]
        {
            get { return ContainsKey(key) ? fValue[key] : null; }
            set {
                if (value == null) throw new ArgumentNullException("value", "A null value cannot be added to a BDictionary");
                fValue[key] = value;
            }
        }

        public void Add(KeyValuePair<BString, IBObject> item)
        {
            if (item.Value == null) throw new ArgumentException("Must not contain a null value", "item");
            fValue.Add(item);
        }

        public void Add(BString key, IBObject value)
        {
            if (value == null) throw new ArgumentNullException("value");
            fValue.Add(key, value);
        }

        public void Clear()
        {
            fValue.Clear();
        }

        public bool Contains(KeyValuePair<BString, IBObject> item)
        {
            return fValue.Contains(item);
        }

        public bool ContainsKey(BString key)
        {
            return fValue.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<BString, IBObject>[] array, int arrayIndex)
        {
            fValue.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<BString, IBObject>> GetEnumerator()
        {
            return fValue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(KeyValuePair<BString, IBObject> item)
        {
            return fValue.Remove(item);
        }

        public bool Remove(BString key)
        {
            return fValue.Remove(key);
        }

        public bool TryGetValue(BString key, out IBObject value)
        {
            return fValue.TryGetValue(key, out value);
        }

        #endregion
    }
}
