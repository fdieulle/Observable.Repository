﻿using System;
using System.Collections.Generic;
using Observable.Repository.Collections;
using Observable.Repository.Configuration;
using Observable.Repository.Core;

namespace Observable.Repository.Join
{
    /// <summary>
    /// Store to manage the right source for <see cref="JoinMode.OneToBuild"/> join.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of main source of the repository.</typeparam>
    /// <typeparam name="TRight">Type of the joined source for the repository.</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources.</typeparam>
    public class StoreOneToBuild<TKey, TValue, TLeft, TRight, TLinkKey> : IStore<TKey, TValue, TLeft>
    {
        #region Fields

        private readonly Subject<RepositoryNotification<TLeft>> _subject = new Subject<RepositoryNotification<TLeft>>();
        private readonly IDisposable _subscribesOnRightSource;

        private readonly Func<TLeft, TLinkKey> _getLeftLinkKey;
        private readonly Func<TRight, TLinkKey> _getRightLinkKey;
        private readonly Func<TRight, bool> _rightFilter;

        private readonly Dictionary<TLinkKey, TRight> _rightItems = new Dictionary<TLinkKey, TRight>();
        private readonly Pool<Dictionary<TKey, TLeft>> _poolForLefts = new Pool<Dictionary<TKey, TLeft>>(() => new Dictionary<TKey, TLeft>());
        private readonly Dictionary<TLinkKey, Dictionary<TKey, TLeft>> _leftItems = new Dictionary<TLinkKey, Dictionary<TKey, TLeft>>();
        private readonly Dictionary<TKey, TLinkKey> _keys = new Dictionary<TKey, TLinkKey>(); 
        private readonly Pool<LinkedNode<TKey, TLeft>> _poolForUpdates = new Pool<LinkedNode<TKey, TLeft>>(() => new LinkedNode<TKey, TLeft>());
        private readonly HashLinkedList<TKey, TLeft> _leftsUpdated;
        private readonly Mutex _mutex;

        #endregion // Fields

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration">Join configuration.</param>
        /// <param name="source">Joined source.</param>
        /// <param name="snapshot">Joined source snapshot.</param>
        /// <param name="mutex">Mutex object to be thread safe.</param>
        public StoreOneToBuild(
            JoinOneToBuildConfiguration<TKey, TValue, TLeft, TRight, TLinkKey> configuration,
            IObservable<RepositoryNotification<TRight>> source,
            IEnumerable<TRight> snapshot,
            Mutex mutex)
        {
            _mutex = mutex ?? new Mutex();

            _getLeftLinkKey = configuration.LeftLinkKey;
            _getRightLinkKey = configuration.RightLinkKey;
            _rightFilter = configuration.RightFilter ?? (p => true);

            _leftsUpdated = new HashLinkedList<TKey, TLeft>(_poolForUpdates);

            if (snapshot != null)
                OnRightItemsReceived(new RepositoryNotification<TRight>(ActionType.Add, null, snapshot));
            if (source != null)
                _subscribesOnRightSource = source.Subscribe(OnRightItemsReceived);
        }

        #region Implementation of IObservable<out RepositoryNotification<TLeft>>

        /// <summary>
        /// Subscribe on the store notifications.
        /// </summary>
        /// <param name="observer">Observer</param>
        /// <returns>Returns the result of subscription.</returns>
        public IDisposable Subscribe(IObserver<RepositoryNotification<TLeft>> observer) 
            => _subject.Subscribe(observer);

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _subscribesOnRightSource?.Dispose();

            _rightItems.Clear();
            _leftItems.Clear();
            _keys.Clear();
            _leftsUpdated.Clear();
            _poolForUpdates.Clear();
            _poolForLefts.Clear();

            _subject.OnCompleted();
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
            var key = _getLeftLinkKey(left);
            _rightItems.TryGetValue(key, out var right);
            return right;
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

            if (_keys.TryGetValue(key, out var previousLinkKey)
                && !Equals(previousLinkKey, linkKey)
                && _leftItems.TryGetValue(previousLinkKey, out var previousLefts))
                RemoveLefts(key, previousLinkKey, previousLefts);
            _keys[key] = linkKey;

            if (!_leftItems.TryGetValue(linkKey, out var lefts))
                _leftItems.Add(linkKey, lefts = _poolForLefts.Get());

            lefts[key] = left;
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

            if (!_leftItems.TryGetValue(linkKey, out var lefts))
                return;

            RemoveLefts(key, linkKey, lefts);
        }

        /// <summary>
        /// Call when the repository cleared values.
        /// </summary>
        public void LeftCleared()
        {
            foreach (var pair in _leftItems)
            {
                pair.Value.Clear();
                _poolForLefts.Free(pair.Value);
            }

            _leftItems.Clear();
            _keys.Clear();
        }

        private void RemoveLefts(TKey key, TLinkKey linkKey, Dictionary<TKey, TLeft> lefts)
        {
            lefts.Remove(key);
            if (lefts.Count == 0)
            {
                _leftItems.Remove(linkKey);
                _poolForLefts.Free(lefts);
            }
        }

        #endregion

        #region Manage Rigth items

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

                if (_leftsUpdated.Count > 0)
                    _subject.OnNext(new RepositoryNotification<TLeft>(ActionType.Update, null, _leftsUpdated.FlushValues()));
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

            if (!_leftItems.TryGetValue(key, out var lefts))
                return;

            foreach (var left in lefts)
                _leftsUpdated[left.Key] = left.Value;
        }

        private void RemoveRight(TRight right)
        {
            var key = _getRightLinkKey(right);

            if (!_rightItems.Remove(key))
                return;

            if (!_leftItems.TryGetValue(key, out var lefts))
                return;

            foreach (var left in lefts)
                _leftsUpdated[left.Key] = left.Value;
        }

        private void ClearRights()
        {
            foreach (var right in _rightItems)
            {
                if (!_leftItems.TryGetValue(right.Key, out var lefts))
                    continue;

                foreach (var left in lefts)
                    _leftsUpdated[left.Key] = left.Value;
            }

            _rightItems.Clear();
        }

        #endregion
    }
}
