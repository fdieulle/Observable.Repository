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
        private readonly Pool<LinkedNode<TKey, TValue>> _pool;
        private readonly Dictionary<TKey, LinkedNode<TKey, TValue>> _items = new Dictionary<TKey, LinkedNode<TKey, TValue>>();
        private LinkedNode<TKey, TValue> _first;
        private LinkedNode<TKey, TValue> _last;

        /// <summary>
        /// Gets the first node.
        /// </summary>
        public LinkedNode<TKey, TValue> First => _first;

        /// <summary>
        /// Gets the last node.
        /// </summary>
        public LinkedNode<TKey, TValue> Last => _last;

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pool">A pool to recycle nodes</param>
        public HashLinkedList(Pool<LinkedNode<TKey, TValue>> pool)
        {
            _pool = pool;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>Returns the enumerator</returns>
        public IEnumerator<LinkedNode<TKey, TValue>> GetEnumerator()
        {
            var cursor = _first;
            while (cursor != null)
            {
                yield return cursor;
                cursor = cursor._next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        /// <summary>
        /// Gets or sets an item with its key.
        /// </summary>
        /// <param name="key">Key of the item.</param>
        /// <returns>The value found</returns>
        public TValue this[TKey key]
        {
            get => _items[key]._value;
            set => Add(key, value);
        }

        /// <summary>
        /// Test if the collection contains this key.
        /// </summary>
        /// <param name="key">Key to test</param>
        /// <returns>Returns true if it contains the key, false else.</returns>
        public bool ContainsKey(TKey key) => _items.ContainsKey(key);

        /// <summary>
        /// Add or update a new key value pair.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(TKey key, TValue value)
        {
            if (!_items.TryGetValue(key, out var node))
            {
                _items.Add(key, node = _pool.Get());
                node._key = key;
                node._next = null;    

                if (_first == null)
                    _first = node;

                if (_last != null)
                    _last._next = node;

                node._previous = _last;

                _last = node;
            }

            node._value = value;
        }

        /// <summary>
        /// Remove an entry from its key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if a value has been removed. False else.</returns>
        public bool Remove(TKey key)
        {
            if (!_items.TryGetValue(key, out var node)) return false;

            if (node._previous == null)
                _first = node._next;
            else node._previous._next = node._next;

            if (node._next == null)
                _last = node._previous;
            else node._next._previous = node._previous;

            node._key = default(TKey);
            node._value = default(TValue);
            _pool.Free(node);

            return _items.Remove(key);
        }

        /// <summary>
        /// Try get a value from its key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Return true if a value has been found. False else.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_items.TryGetValue(key, out var node))
            {
                value = node._value;
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

            _items.Clear();
            var cursor = _first;
            while (cursor != null)
            {
                var next = cursor._next;
                
                if (hasAction) cleanItem(cursor._key, cursor._value);

                cursor._key = default(TKey);
                cursor._value = default(TValue);
                _pool.Free(cursor);

                cursor = next;
            }

            _first = null;
            _last = null;
        }

        /// <summary>
        /// Flush all items, make an array copy then clear them.
        /// </summary>
        /// <returns>The items array copy.</returns>
        public KeyValue<TKey, TValue>[] Flush()
        {
            var array = new KeyValue<TKey, TValue>[_items.Count];
            var index = 0;

            _items.Clear();

            var cursor = _first;
            while (cursor != null)
            {
                var next = cursor._next;

                array[index]._key = cursor._key;
                array[index]._value = cursor._value;
                index++;

                cursor._key = default(TKey);
                cursor._value = default(TValue);
                _pool.Free(cursor);

                cursor = next;
            }

            _first = null;
            _last = null;

            return array;
        }

        /// <summary>
        /// Flush values, make an array copy of value then clear all items.
        /// </summary>
        /// <returns>Returns the array of values only.</returns>
        public TValue[] FlushValues()
        {
            var array = new TValue[_items.Count];
            var index = 0;

            _items.Clear();

            var cursor = _first;
            while (cursor != null)
            {
                var next = cursor._next;

                array[index++] = cursor._value;

                cursor._key = default(TKey);
                cursor._value = default(TValue);
                _pool.Free(cursor);

                cursor = next;
            }

            _first = null;
            _last = null;

            return array;
        }

        /// <summary>
        /// Create a copy from the items.
        /// </summary>
        /// <returns>The items copy</returns>
        public KeyValue<TKey, TValue>[] MakeCopy()
        {
            var array = new KeyValue<TKey, TValue>[_items.Count];
            var index = 0;

            var cursor = _first;
            while (cursor != null)
            {
                array[index]._key = cursor._key;
                array[index]._value = cursor._value;
                index++;
                
                cursor = cursor._next;
            }

            return array;
        }
    }
}
