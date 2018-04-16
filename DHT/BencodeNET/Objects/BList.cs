﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BencodeNET.IO;

namespace BencodeNET.Objects
{
    /// <summary>
    /// Represents a bencoded list of <see cref="IBObject"/>.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="IList{IBObject}"/>.
    /// </remarks>
    public class BList : BObject<IList<IBObject>>, IList<IBObject>
    {
        /// <summary>
        /// The underlying list.
        /// </summary>
        public override IList<IBObject> Value { get; } = new List<IBObject>();

        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public BList()
        { }

        /// <summary>
        /// Creates a list from strings using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <param name="strings"></param>
        public BList(IEnumerable<string> strings)
            : this(strings, Encoding.UTF8)
        { }

        /// <summary>
        /// Creates a list from strings using the specified encoding.
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="encoding"></param>
        public BList(IEnumerable<string> strings, Encoding encoding)
        {
            foreach (var str in strings)
            {
                Add(str, encoding);
            }
        }

        /// <summary>
        /// Creates a list from en <see cref="IEnumerable{T}"/> of <see cref="IBObject"/>.
        /// </summary>
        /// <param name="objects"></param>
        public BList(IEnumerable<IBObject> objects)
        {
            Value = new List<IBObject>(objects);
        }

        /// <summary>
        /// Adds a string to the list using <see cref="Encoding.UTF8"/>.
        /// </summary>
        /// <param name="value"></param>
        public void Add(string value)
        {
            Add(new BString(value));
        }

        /// <summary>
        /// Adds a string to the list using the specified encoding.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public void Add(string value, Encoding encoding)
        {
            Add(new BString(value, encoding));
        }

        /// <summary>
        /// Adds an integer to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(int value)
        {
            Add((IBObject) new BNumber(value));
        }

        /// <summary>
        /// Adds a long to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(long value)
        {
            Add((IBObject) new BNumber(value));
        }

        /// <summary>
        /// Appends a list to the end of this instance.
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(BList list)
        {
            foreach (var obj in list)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Gets the object at the specified index as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to cast the object to.</typeparam>
        /// <param name="index">The index in the list to get the object from.</param>
        /// <returns>The object at the specified index as the specified type or null if the object is not of that type.</returns>
        public T Get<T>(int index) where T : class, IBObject
        {
            return this[index] as T;
        }

        /// <summary>
        /// Assumes all elements are <see cref="BString"/>
        /// and returns an enumerable of their string representation.
        /// </summary>
        public IEnumerable<string> AsStrings()
        {
            return AsStrings(Encoding.UTF8);
        }

        /// <summary>
        /// Assumes all elements are <see cref="BString"/> and returns
        /// an enumerable of their string representation using the specified encoding.
        /// </summary>
        public IEnumerable<string> AsStrings(Encoding encoding)
        {
            IList<BString> bstrings = this.AsType<BString>();
            return bstrings.Select(x => x.ToString(encoding));
        }

        /// <summary>
        /// Assumes all elements are <see cref="BNumber"/>
        /// and returns an enumerable of their <c>long</c> value.
        /// </summary>
        public IEnumerable<long> AsNumbers()
        {
            IList<BNumber> bnumbers = this.AsType<BNumber>();
            return bnumbers.Select(x => x.Value);
        }

        /// <summary>
        /// Attempts to cast all elements to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">
        /// An element is not of type <typeparamref name="T"/>.
        /// </exception>
        public BList<T> AsType<T>() where T : class, IBObject
        {
            try
            {
                return new BList<T>(this.Cast<T>());
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException($"Not all elements are of type '{typeof(T).FullName}'.", ex);
            }
        }

#pragma warning disable 1591
        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write('l');
            foreach (var item in this)
            {
                item.EncodeTo(stream);
            }
            stream.Write('e');
        }
#pragma warning restore 1591

        #region IList<IBObject> Members
#pragma warning disable 1591

        public int Count => Value.Count;

        public bool IsReadOnly => Value.IsReadOnly;

        public IBObject this[int index]
        {
            get { return Value[index]; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                Value[index] = value;
            }
        }

        public void Add(IBObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Value.Add(item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public bool Contains(IBObject item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(IBObject[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IBObject> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IBObject item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, IBObject item)
        {
            Value.Insert(index, item);
        }

        public bool Remove(IBObject item)
        {
            return Value.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Value.RemoveAt(index);
        }

#pragma warning restore 1591
        #endregion
    }

    /// <summary>
    /// Represents a bencoded list of type <typeparamref name="T"/> which implements <see cref="IBObject"/> .
    /// </summary>
    public class BList<T> : BList, IList<T> where T : class, IBObject
    {
        /// <summary>
        /// The underlying list.
        /// </summary>
        public override IList<IBObject> Value { get; } = new List<IBObject>();

        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public BList()
        { }

        /// <summary>
        /// Creates a list from the specified objects.
        /// </summary>
        /// <param name="objects"></param>
        public BList(IEnumerable<T> objects)
        {
            Value = objects.Cast<IBObject>().ToList();
        }

        #region IList<T> Members
#pragma warning disable 1591

        public new T this[int index]
        {
            get
            {
                var obj = Value[index] as T;
                if (obj == null) throw new InvalidCastException($"The object at index {index} is not of type {typeof (T).FullName}");
                return obj;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                Value[index] = value;
            }
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Value.Add(item);
        }

        public bool Contains(T item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Value.CopyTo(array.Cast<IBObject>().ToArray(), arrayIndex);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            var i = 0;
            var enumerator = Value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var obj = enumerator.Current as T;
                if (obj == null) throw new InvalidCastException($"The object at index {i} is not of type {typeof(T).FullName}");
                yield return (T) enumerator.Current;
                i++;
            }
        }

        public int IndexOf(T item)
        {
            return Value.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Value.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return Value.Remove(item);
        }

#pragma warning restore 1591
        #endregion
    }
}
