using System;
using System.Collections;
using System.Collections.Generic;
using Observable.Repository.Collections;

namespace Observable.Repository.Core
{
    /// <summary>
    /// collection structure which combine a <see cref="Dictionary{TKey, TValue}"/> and a linked list to store items.
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TValue">Type of values</typeparam>
    public class HashLinkedList<TKey, TValue> : IEnumerable<LinkedNode<TKey, TValue>>
    {
        private readonly Pool<LinkedNode<TKey, TValue>> pool;
        private readonly Dictionary<TKey, LinkedNode<TKey, TValue>> items = new Dictionary<TKey, LinkedNode<TKey, TValue>>();
        private LinkedNode<TKey, TValue> first;
        private LinkedNode<TKey, TValue> last;

        /// <summary>
        /// Gets the first node.
        /// </summary>
        public LinkedNode<TKey, TValue> First { get { return first; } }

        /// <summary>
        /// Gets the last node.
        /// </summary>
        public LinkedNode<TKey, TValue> Last { get { return last; } } 

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        public int Count { get { return items.Count; } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pool">A pool to recycle nodes</param>
        public HashLinkedList(Pool<LinkedNode<TKey, TValue>> pool)
        {
            this.pool = pool;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>Returns the enumerator</returns>
        public IEnumerator<LinkedNode<TKey, TValue>> GetEnumerator()
        {
            var cursor = first;
            while (cursor != null)
            {
                yield return cursor;
                cursor = cursor.next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets or sets an item with its key.
        /// </summary>
        /// <param name="key">Key of the item.</param>
        /// <returns>The value found</returns>
        public TValue this[TKey key]
        {
            get { return items[key].value; }
            set { Add(key, value); }
        }

        /// <summary>
        /// Test if the collection contains this key.
        /// </summary>
        /// <param name="key">Key to test</param>
        /// <returns>Returns true if it contains the key, false else.</returns>
        public bool ContainsKey(TKey key)
        {
            return items.ContainsKey(key);
        }

        /// <summary>
        /// Add or update a new key value pair.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(TKey key, TValue value)
        {
            LinkedNode<TKey, TValue> node;
            if (!items.TryGetValue(key, out node))
            {
                items.Add(key, node = pool.Get());
                node.key = key;
                node.next = null;    

                if (first == null)
                    first = node;

                if (last != null)
                    last.next = node;

                node.previous = last;

                last = node;
            }

            node.value = value;
        }

        /// <summary>
        /// Remove an entry from its key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if a value has been removed. False else.</returns>
        public bool Remove(TKey key)
        {
            LinkedNode<TKey, TValue> node;
            if (!items.TryGetValue(key, out node)) return false;

            if (node.previous == null)
                first = node.next;
            else node.previous.next = node.next;

            if (node.next == null)
                last = node.previous;
            else node.next.previous = node.previous;

            node.key = default(TKey);
            node.value = default(TValue);
            pool.Free(node);

            return items.Remove(key);
        }

        /// <summary>
        /// Try get a value from its key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Return true if a value has been found. False else.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            LinkedNode<TKey, TValue> node;
            if (items.TryGetValue(key, out node))
            {
                value = node.value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Clear all pairs.
        /// </summary>
        /// <param name="cleanItem">Define an optional task on each items during cleaning.</param>
        public void Clear(Action<TKey, TValue> cleanItem = null)
        {
            var hasAction = cleanItem != null;

            items.Clear();
            var cursor = first;
            while (cursor != null)
            {
                var next = cursor.next;
                
                if (hasAction) cleanItem(cursor.key, cursor.value);

                cursor.key = default(TKey);
                cursor.value = default(TValue);
                pool.Free(cursor);

                cursor = next;
            }

            first = null;
            last = null;
        }

        /// <summary>
        /// Flush all items, make an array copy then clear them.
        /// </summary>
        /// <returns>The items aray copy.</returns>
        public KeyValue<TKey, TValue>[] Flush()
        {
            var array = new KeyValue<TKey, TValue>[items.Count];
            var index = 0;

            items.Clear();

            var cursor = first;
            while (cursor != null)
            {
                var next = cursor.next;

                array[index].key = cursor.key;
                array[index].value = cursor.value;
                index++;

                cursor.key = default(TKey);
                cursor.value = default(TValue);
                pool.Free(cursor);

                cursor = next;
            }

            first = null;
            last = null;

            return array;
        }

        /// <summary>
        /// Flush values, make an array copy of value then clear all items.
        /// </summary>
        /// <returns>Returns the array of values only.</returns>
        public TValue[] FlushValues()
        {
            var array = new TValue[items.Count];
            var index = 0;

            items.Clear();

            var cursor = first;
            while (cursor != null)
            {
                var next = cursor.next;

                array[index++] = cursor.value;

                cursor.key = default(TKey);
                cursor.value = default(TValue);
                pool.Free(cursor);

                cursor = next;
            }

            first = null;
            last = null;

            return array;
        }

        /// <summary>
        /// Create a copy from the items.
        /// </summary>
        /// <returns>The items copy</returns>
        public KeyValue<TKey, TValue>[] MakeCopy()
        {
            var array = new KeyValue<TKey, TValue>[items.Count];
            var index = 0;

            var cursor = first;
            while (cursor != null)
            {
                array[index].key = cursor.key;
                array[index].value = cursor.value;
                index++;
                
                cursor = cursor.next;
            }

            return array;
        }
    }
}
