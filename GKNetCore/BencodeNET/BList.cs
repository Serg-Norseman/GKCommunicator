using System;
using System.Collections;
using System.Collections.Generic;

namespace BencodeNET
{
    /// <summary>
    /// Represents a bencoded list of <see cref="IBObject"/>.
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="IList{IBObject}"/>.
    /// </remarks>
    public class BList : BObject<IList<IBObject>>, IList<IBObject>
    {
        private readonly IList<IBObject> fValue;

        /// <summary>
        /// The underlying list.
        /// </summary>
        public override IList<IBObject> Value
        {
            get { return fValue; }
        }

        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public BList()
        {
            fValue = new List<IBObject>();
        }

        /// <summary>
        /// Creates a list from en <see cref="IEnumerable{T}"/> of <see cref="IBObject"/>.
        /// </summary>
        /// <param name="objects"></param>
        public BList(IEnumerable<IBObject> objects)
        {
            fValue = new List<IBObject>(objects);
        }

        /// <summary>
        /// Adds a string to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(BString value)
        {
            Add((IBObject)value);
        }

        /// <summary>
        /// Adds a number (integer or long) to the list.
        /// </summary>
        /// <param name="value"></param>
        public void Add(BNumber value)
        {
            Add((IBObject)value);
        }

        /// <summary>
        /// Appends a list to the end of this instance.
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(BList list)
        {
            foreach (var obj in list) {
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

        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write('l');
            foreach (var item in this) {
                item.EncodeTo(stream);
            }
            stream.Write('e');
        }

        #region IList<IBObject> Members

        public int Count
        {
            get { return fValue.Count; }
        }

        public bool IsReadOnly
        {
            get { return fValue.IsReadOnly; }
        }

        public IBObject this[int index]
        {
            get { return fValue[index]; }
            set {
                if (value == null) throw new ArgumentNullException("value");
                fValue[index] = value;
            }
        }

        public void Add(IBObject item)
        {
            if (item == null) throw new ArgumentNullException("item");
            fValue.Add(item);
        }

        public void Clear()
        {
            fValue.Clear();
        }

        public bool Contains(IBObject item)
        {
            return fValue.Contains(item);
        }

        public void CopyTo(IBObject[] array, int arrayIndex)
        {
            fValue.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IBObject> GetEnumerator()
        {
            return fValue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IBObject item)
        {
            return fValue.IndexOf(item);
        }

        public void Insert(int index, IBObject item)
        {
            fValue.Insert(index, item);
        }

        public bool Remove(IBObject item)
        {
            return fValue.Remove(item);
        }

        public void RemoveAt(int index)
        {
            fValue.RemoveAt(index);
        }

        #endregion
    }
}
