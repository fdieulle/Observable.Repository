using System;
using System.Collections.Generic;
using Observable.Repository.Collections;
using Observable.Repository.Configuration;
using Observable.Repository.Core;

namespace Observable.Repository.Join
{
    /// <summary>
    /// Store to manage the right source for <see cref="JoinMode.OneToUpdate"/> join.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of main source of the repository.</typeparam>
    /// <typeparam name="TRight">Type of the joined source for the repository.</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources.</typeparam>
    public class StoreOneToUpdate<TKey, TValue, TLeft, TRight, TLinkKey> : IStore<TKey, TValue, TLeft>
    {
        #region Fields

        private readonly Func<TLeft, TLinkKey> _getLeftLinkKey;
        private readonly Func<TRight, TLinkKey> _getRightLinkKey;
        private readonly Func<TRight, bool> _rightFilter;
        private readonly Func<TValue, Action<TRight>> _onUpdate;
        private readonly Action<RepositoryNotification<KeyValue<TKey, TValue>>> _forward;

        private readonly Dictionary<TLinkKey, TRight> _rightItems = new Dictionary<TLinkKey, TRight>();
        private readonly Pool<Dictionary<TKey, TValue>> _pool = new Pool<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>());
        private readonly Dictionary<TLinkKey, Dictionary<TKey, TValue>> _valueItems = new Dictionary<TLinkKey, Dictionary<TKey, TValue>>();
        private readonly Pool<LinkedNode<TKey, TValue>> _pool2 = new Pool<LinkedNode<TKey, TValue>>(() => new LinkedNode<TKey, TValue>());
        private readonly Dictionary<TKey, TLinkKey> _keys = new Dictionary<TKey, TLinkKey>();
        private readonly HashLinkedList<TKey, TValue> _valuesUpdated; 
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
        /// <param name="forward">Forward notifications to repository owner</param>
        public StoreOneToUpdate(
            JoinOneToUpdateConfiguration<TKey, TValue, TLeft, TRight, TLinkKey> configuration,
            IObservable<RepositoryNotification<TRight>> source,
            IEnumerable<TRight> snapshot,
            Mutex mutex,
            Action<RepositoryNotification<KeyValue<TKey, TValue>>> forward)
        {
            this._mutex = mutex ?? new Mutex();
            _valuesUpdated = new HashLinkedList<TKey, TValue>(_pool2);

            _getLeftLinkKey = configuration.LeftLinkKey;
            _getRightLinkKey = configuration.RightLinkKey;
            _rightFilter = configuration.RightFilter;
            _onUpdate = configuration.OnUpdate;
            this._forward = forward;

            if(snapshot != null)
                OnRightItemsReceived(new RepositoryNotification<TRight>(ActionType.Add, null, snapshot));
            if (source != null)
                _subscribesOnRightSource = source.Subscribe(OnRightItemsReceived);
        }

        #region Implementation of IObservable<out RepositoryNotification<TLeft>>

        /// <summary>
        /// 
        /// </summary>
        public IDisposable Subscribe(IObserver<RepositoryNotification<TLeft>> observer)
        {
            throw new NotSupportedException("This store isn't used to build the value");
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if(_subscribesOnRightSource != null)
                _subscribesOnRightSource.Dispose();

            _rightItems.Clear();
            _valueItems.Clear();
            _pool.Clear();
            _pool2.Clear();
            _keys.Clear();
            _valuesUpdated.Clear();
        }

        #endregion

        #region Implementation of IStore<in TKey,in TValue,TLeft>

        /// <summary>
        /// Get the right item from the left.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <returns>The right instance.</returns>
        public object GetRight(TLeft left)
        {
            throw new NotSupportedException("This store isn't used to build the value");
        }

        /// <summary>
        /// Call when the repository added new values.
        /// </summary>
        /// <param name="key">The item key</param>
        /// <param name="left">The source item</param>
        /// <param name="value">The item value</param>
        public void LeftAdded(TKey key, TLeft left, TValue value)
        {
            var linkKey = _getLeftLinkKey(left);

            TLinkKey previousLinkKey;
            Dictionary<TKey, TValue> previousValues;
            if (_keys.TryGetValue(key, out previousLinkKey)
                && !Equals(previousLinkKey, linkKey)
                && _valueItems.TryGetValue(previousLinkKey, out previousValues))
                RemoveValues(key, previousLinkKey, previousValues);
            _keys[key] = linkKey;

            Dictionary<TKey, TValue> values;
            if(!_valueItems.TryGetValue(linkKey, out values))
                _valueItems.Add(linkKey, values = _pool.Get());

            values[key] = value;

            TRight right;
            if (_rightItems.TryGetValue(linkKey, out right))
                _onUpdate(value)(right);
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

            TLinkKey previousLinkKey;
            Dictionary<TKey, TValue> previousValues;
            if (_keys.TryGetValue(key, out previousLinkKey)
                && !Equals(previousLinkKey, linkKey)
                && _valueItems.TryGetValue(previousLinkKey, out previousValues))
                previousValues.Remove(key);
            _keys.Remove(key);

            Dictionary<TKey, TValue> values;
            if (!_valueItems.TryGetValue(linkKey, out values))
                return;

            RemoveValues(key, linkKey, values);
        }

        /// <summary>
        /// Call when the repository cleared values.
        /// </summary>
        public void LeftCleared()
        {
            foreach (var pair in _valueItems)
            {
                pair.Value.Clear();
                _pool.Free(pair.Value);
            }
            _valueItems.Clear();
            _keys.Clear();
        }

        private void RemoveValues(TKey key, TLinkKey linkKey, Dictionary<TKey, TValue> values)
        {
            values.Remove(key);
            if (values.Count != 0) return;

            _valueItems.Remove(linkKey);
            _pool.Free(values);
        }

        #endregion

        #region Manage Right items

        private void OnRightItemsReceived(RepositoryNotification<TRight> e)
        {
            lock (_mutex._output)
            {
                lock (_mutex._input)
                {
                    if (e.Action == ActionType.Reload)
                        ClearRights();
                    else
                    {
                        foreach (var right in e.OldItems)
                            RemoveRight(right);
                    }

                    foreach (var right in e.NewItems)
                        AddOrUpdateRight(right);
                }

                if (_valuesUpdated.Count > 0)
                {
                    var items = _valuesUpdated.Flush();
                    _forward(new RepositoryNotification<KeyValue<TKey, TValue>>(ActionType.Update, items, items));
                }
            }
        }

        private void AddOrUpdateRight(TRight right)
        {
            if (_rightFilter != null && !_rightFilter(right))
            {
                RemoveRight(right);
                return;
            }

            var key = _getRightLinkKey(right);

            _rightItems[key] = right;

            Dictionary<TKey, TValue> values;
            if (!_valueItems.TryGetValue(key, out values))
                return;

            foreach (var pair in values)
            {
                _onUpdate(pair.Value)(right);
                _valuesUpdated[pair.Key] = pair.Value;
            }
        }

        private void RemoveRight(TRight right)
        {
            var key = _getRightLinkKey(right);

            if (!_rightItems.Remove(key))
                return;

            Dictionary<TKey, TValue> values;
            if (!_valueItems.TryGetValue(key, out values))
                return;

            foreach (var pair in values)
            {
                _onUpdate(pair.Value)(default(TRight));
                _valuesUpdated[pair.Key] = pair.Value;
            }
        }

        private void ClearRights()
        {
            foreach (var right in _rightItems)
            {
                Dictionary<TKey, TValue> values;
                if(!_valueItems.TryGetValue(right.Key, out values))
                    continue;

                foreach (var pair in values)
                {
                    _onUpdate(pair.Value)(default(TRight));
                    _valuesUpdated[pair.Key] = pair.Value;
                }
            }
        }

        #endregion 
    }
}
