using System;
using System.Collections.Generic;
using Observable.Repository.Collections;
using Observable.Repository.Configuration;
using Observable.Repository.Core;

namespace Observable.Repository.Join
{
    /// <summary>
    /// Store to manage the right source for <see cref="JoinMode.Many"/> join.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of main source of the repository.</typeparam>
    /// <typeparam name="TRight">Type of the joined source for the repository.</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources.</typeparam>
    /// <typeparam name="TRightKey">Type of right key to populate the list.</typeparam>
    public class StoreMany<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey> : IStore<TKey, TValue, TLeft>
    {
        #region Fields

        private readonly Func<TLeft, TLinkKey> _getLeftLinkKey;
        private readonly Func<TRight, TLinkKey> _getRightLinkKey;
        private readonly Func<TRight, bool> _rightFilter;
        private readonly Func<TRight, TRightKey> _getRightKey;

        private readonly Dictionary<TRightKey, TLinkKey> _rightItems = new Dictionary<TRightKey, TLinkKey>();
        private readonly Dictionary<TLinkKey, ListManager> _managers = new Dictionary<TLinkKey, ListManager>();
        private readonly Pool<ListManager> _managersPool;
        private readonly Pool<LinkedNode<int, TRight>> _rightNodesPool = new Pool<LinkedNode<int, TRight>>(() => new LinkedNode<int, TRight>());
        private readonly Mutex _mutex;
        private readonly IDisposable _subscribesOnRightSource;

        #endregion // Fields

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration">Join configuration.</param>
        /// <param name="source">Joined source.</param>
        /// <param name="snapshot">Joined source snapshot.</param>
        /// <param name="mutex">Mutex object to be thread safe.</param>
        public StoreMany(
            JoinManyConfiguration<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey> configuration,
            IObservable<RepositoryNotification<TRight>> source,
            IEnumerable<TRight> snapshot,
            Mutex mutex)
        {
            _mutex = mutex ?? new Mutex();

            _getLeftLinkKey = configuration.LeftLinkKey;
            _getRightLinkKey = configuration.RightLinkKey;
            _rightFilter = configuration.RightFilter;
            _getRightKey = configuration.GetRightKey;

            _managersPool = new Pool<ListManager>(() => new ListManager(configuration.GetList, _rightNodesPool));

            if (snapshot != null)
                OnRightItemsReceived(new RepositoryNotification<TRight>(ActionType.Add, null, snapshot));
            if (source != null)
                _subscribesOnRightSource = source.Subscribe(OnRightItemsReceived);
        }

        #region Implementation of IObservable<out RepositoryNotification<TLeft>>

        /// <summary>
        /// Do not use
        /// </summary>
        public IDisposable Subscribe(IObserver<RepositoryNotification<TLeft>> observer) 
            => throw new NotSupportedException("This store isn't used to build the value");

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _subscribesOnRightSource?.Dispose();

            _rightItems.Clear();
            _managers.Clear();
            _managersPool.Clear();
            _rightNodesPool.Clear();
        }

        #endregion

        #region Implementation of IStore<in TKey,in TValue,TLeft>

        /// <summary>
        /// Get the right item from the left.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <returns>The right instance.</returns>
        public object GetRight(TLeft left) 
            => throw new NotSupportedException("This store isn't used to build the value");

        /// <summary>
        /// Call when the repository added new values.
        /// </summary>
        /// <param name="key">The item key</param>
        /// <param name="left">The source item</param>
        /// <param name="value">The item value</param>
        public void LeftAdded(TKey key, TLeft left, TValue value)
        {
            var linkKey = _getLeftLinkKey(left);

            if(!_managers.TryGetValue(linkKey, out var manager))
                _managers.Add(linkKey, manager = _managersPool.Get());

            manager.AddValue(key, value);
        }

        /// <summary>
        /// Call when the repository removed old values.
        /// </summary>
        /// <param name="key">The item key</param>
        /// <param name="left">The source item</param>
        /// <param name="value">The item value</param>
        public void LeftRemoved(TKey key, TLeft left, TValue value)
        {
            var linkKey = _getLeftLinkKey(left);

            if (!_managers.TryGetValue(linkKey, out var manager))
                return;

            manager.RemoveValue(key);

            if (manager.IsEmpty)
            {
                _managers.Remove(linkKey);
                _managersPool.Free(manager);
            }
        }

        /// <summary>
        /// Call when the repository cleared values.
        /// </summary>
        public void LeftCleared()
        {
            var entriesToRemove = new KeyValuePair<TLinkKey, ListManager>[_managers.Count];
            var count = 0;
            foreach (var pair in _managers)
            {
                pair.Value.ClearValues();
                if (pair.Value.IsEmpty)
                    entriesToRemove[count++] = pair;
            }

            for (var i = 0; i < count; i++)
            {
                _managers.Remove(entriesToRemove[i].Key);
                _managersPool.Free(entriesToRemove[i].Value);
            }
        }

        #endregion

        #region Manage Right items

        private void OnRightItemsReceived(RepositoryNotification<TRight> e)
        {
            lock (_mutex._input)
            {
                if (e.Action == ActionType.Reload)
                    ClearRight();
                else
                {
                    foreach (var right in e.OldItems)
                        RemoveRight(right);
                }

                foreach (var right in e.NewItems)
                    AddOrUpdateRight(right);   
            }
        }

        private void AddOrUpdateRight(TRight right)
        {
            if (_rightFilter != null && !_rightFilter(right))
            {
                RemoveRight(right);
                return;
            }

            var rightKey = _getRightKey(right);
            var linkKey = _getRightLinkKey(right);

            if (_rightItems.TryGetValue(rightKey, out var oldLinkKey))
            {
                // In case of the right instance has changed its link key
                if (!linkKey.Equals(oldLinkKey) && _managers.ContainsKey(oldLinkKey))
                    _managers[oldLinkKey].RemoveRight(rightKey);
            }

            _rightItems[rightKey] = linkKey;

            if(!_managers.TryGetValue(linkKey, out var manager))
                _managers.Add(linkKey, manager = _managersPool.Get());

            manager.AddRight(rightKey, right);
        }

        private void RemoveRight(TRight right)
        {
            var rightKey = _getRightKey(right);

            if (!_rightItems.Remove(rightKey))
                return;

            var linkKey = _getRightLinkKey(right);

            if (!_managers.TryGetValue(linkKey, out var manager))
                return;

            manager.RemoveRight(rightKey);

            if (manager.IsEmpty)
            {
                _managers.Remove(linkKey);
                _managersPool.Free(manager);
            }
        }

        private void ClearRight()
        {
            var entriesToRemove = new KeyValuePair<TLinkKey, ListManager>[_managers.Count];
            var count = 0;
            foreach (var pair in _managers)
            {
                pair.Value.ClearRights();
                if (pair.Value.IsEmpty)
                    entriesToRemove[count++] = pair;
            }

            for (var i = 0; i < count; i++)
            {
                _managers.Remove(entriesToRemove[i].Key);
                _managersPool.Free(entriesToRemove[i].Value);
            }

            _rightItems.Clear();
        }

        private class ListManager
        {
            private readonly Func<TValue, IList<TRight>> _getList;
            private readonly Pool<LinkedNode<int, TRight>> _pool;
            private readonly Dictionary<TRightKey, LinkedNode<int, TRight>> _rightIndices = new Dictionary<TRightKey, LinkedNode<int, TRight>>();
            private readonly Dictionary<TKey, IList<TRight>> _lists = new Dictionary<TKey, IList<TRight>>();
            private LinkedNode<int, TRight> _last;
            private LinkedNode<int, TRight> _first;

            public bool IsEmpty => _rightIndices.Count == 0 && _lists.Count == 0;

            public ListManager(Func<TValue, IList<TRight>> getList, Pool<LinkedNode<int, TRight>> pool)
            {
                _getList = getList;
                _pool = pool;
            }

            public void AddRight(TRightKey rightKey, TRight right)
            {
                if (_rightIndices.TryGetValue(rightKey, out var node)) // Update lists
                {
                    node._value = right;

                    foreach (var pair in _lists)
                        pair.Value[node._key] = right;
                }
                else // Add right item in lists
                {
                    node = _pool.Get();
                    node._key = _rightIndices.Count;
                    node._next = null;

                    if (_first == null)
                        _first = node;
                    if (_last != null)
                        _last._next = node;

                    node._previous = _last;
                    _last = node;

                    _rightIndices.Add(rightKey, node);

                    node._value = right;

                    foreach (var pair in _lists)
                        pair.Value.Add(right);
                }
            }

            public void RemoveRight(TRightKey rightKey)
            {
                if (!_rightIndices.TryGetValue(rightKey, out var node)) 
                    return;

                if (node._previous == null)
                    _first = node._next;
                else node._previous._next = node._next;

                if (node._next == null)
                    _last = node._previous;
                else node._next._previous = node._previous;

                // Update indices
                var cursor = node._next;
                while (cursor != null)
                {
                    cursor._key -= 1;
                    cursor = cursor._next;
                }

                // Remove from lists
                var index = node._key;
                foreach (var pair in _lists)
                    pair.Value.RemoveAt(index);

                // Free right node
                _rightIndices.Remove(rightKey);
                node._value = default(TRight);
                _pool.Free(node);
            }

            public void ClearRights()
            {
                _rightIndices.Clear();
                
                var cursor = _first;
                while (cursor != null)
                {
                    var next = cursor._next;
                    cursor._value = default(TRight);
                    _pool.Free(cursor);

                    cursor = next;
                }
                _first = null;
                _last = null;

                foreach (var pair in _lists)
                    pair.Value.Clear();
            }

            public void AddValue(TKey key, TValue value)
            {
                var list = _getList(value);
                if (list == null) return;

                _lists[key] = list;

                var cursor = _first;
                while (cursor != null)
                {
                    list.Add(cursor._value);
                    cursor = cursor._next;
                }
            }

            public void RemoveValue(TKey key)
            {
                if (!_lists.TryGetValue(key, out var list))
                    return;

                list.Clear();
                _lists.Remove(key);
            }

            public void ClearValues()
            {
                foreach (var pair in _lists)
                    pair.Value.Clear();
                
                _lists.Clear();
            }
        }

        #endregion
    }
}
