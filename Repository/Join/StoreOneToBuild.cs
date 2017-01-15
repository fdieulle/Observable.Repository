using System;
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

        private readonly Subject<RepositoryNotification<TLeft>> subject = new Subject<RepositoryNotification<TLeft>>();
        private readonly IDisposable subscribesOnRightSource;

        private readonly Func<TLeft, TLinkKey> getLeftLinkKey;
        private readonly Func<TRight, TLinkKey> getRightLinkKey;
        private readonly Func<TRight, bool> rightFilter;

        private readonly Dictionary<TLinkKey, TRight> rightItems = new Dictionary<TLinkKey, TRight>();
        private readonly Pool<Dictionary<TKey, TLeft>> poolForLefts = new Pool<Dictionary<TKey, TLeft>>(() => new Dictionary<TKey, TLeft>());
        private readonly Dictionary<TLinkKey, Dictionary<TKey, TLeft>> leftItems = new Dictionary<TLinkKey, Dictionary<TKey, TLeft>>();
        private readonly Dictionary<TKey, TLinkKey> keys = new Dictionary<TKey, TLinkKey>(); 
        private readonly Pool<LinkedNode<TKey, TLeft>> poolForUpdates = new Pool<LinkedNode<TKey, TLeft>>(() => new LinkedNode<TKey, TLeft>());
        private readonly HashLinkedList<TKey, TLeft> leftsUpdated;
        private readonly Mutex mutex;

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
            this.mutex = mutex ?? new Mutex();

            getLeftLinkKey = configuration.LeftLinkKey;
            getRightLinkKey = configuration.RightLinkKey;
            rightFilter = configuration.RightFilter ?? (p => true);

            leftsUpdated = new HashLinkedList<TKey, TLeft>(poolForUpdates);

            if (snapshot != null)
                OnRightItemsReceived(new RepositoryNotification<TRight>(ActionType.Add, null, snapshot));
            if (source != null)
                subscribesOnRightSource = source.Subscribe(OnRightItemsReceived);
        }

        #region Implementation of IObservable<out RepositoryNotification<TLeft>>

        /// <summary>
        /// Subscribe on the store notifications.
        /// </summary>
        /// <param name="observer">Observer</param>
        /// <returns>Returns the result of suscription.</returns>
        public IDisposable Subscribe(IObserver<RepositoryNotification<TLeft>> observer)
        {
            return subject.Subscribe(observer);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (subscribesOnRightSource != null)
                subscribesOnRightSource.Dispose();

            rightItems.Clear();
            leftItems.Clear();
            keys.Clear();
            leftsUpdated.Clear();
            poolForUpdates.Clear();
            poolForLefts.Clear();

            subject.OnCompleted();
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
            var key = getLeftLinkKey(left);
            TRight right;
            rightItems.TryGetValue(key, out right);
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
            var linkKey = getLeftLinkKey(left);

            TLinkKey previousLinkKey;
            Dictionary<TKey, TLeft> previousLefts;
            if (keys.TryGetValue(key, out previousLinkKey)
                && !Equals(previousLinkKey, linkKey)
                && leftItems.TryGetValue(previousLinkKey, out previousLefts))
                RemoveLefts(key, previousLinkKey, previousLefts);
            keys[key] = linkKey;

            Dictionary<TKey, TLeft> lefts;
            if (!leftItems.TryGetValue(linkKey, out lefts))
                leftItems.Add(linkKey, lefts = poolForLefts.Get());

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
            var linkKey = getLeftLinkKey(left);

            Dictionary<TKey, TLeft> lefts;
            if (!leftItems.TryGetValue(linkKey, out lefts))
                return;

            RemoveLefts(key, linkKey, lefts);
        }

        /// <summary>
        /// Call when the repository cleared values.
        /// </summary>
        public void LeftCleared()
        {
            foreach (var pair in leftItems)
            {
                pair.Value.Clear();
                poolForLefts.Free(pair.Value);
            }

            leftItems.Clear();
            keys.Clear();
        }

        private void RemoveLefts(TKey key, TLinkKey linkKey, Dictionary<TKey, TLeft> lefts)
        {
            lefts.Remove(key);
            if (lefts.Count == 0)
            {
                leftItems.Remove(linkKey);
                poolForLefts.Free(lefts);
            }
        }

        #endregion

        #region Manage Rigth items

        private void OnRightItemsReceived(RepositoryNotification<TRight> e)
        {
            lock (mutex.output)
            {
                lock (mutex.input)
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

                if (leftsUpdated.Count > 0)
                    subject.OnNext(new RepositoryNotification<TLeft>(ActionType.Update, null, leftsUpdated.FlushValues()));
            }
        }

        private void AddOrUpdateRight(TRight right)
        {
            if (rightFilter != null && !rightFilter(right))
            {
                RemoveRight(right);
                return;
            }

            var key = getRightLinkKey(right);
            rightItems[key] = right;

            Dictionary<TKey, TLeft> lefts;
            if (!leftItems.TryGetValue(key, out lefts))
                return;

            foreach (var left in lefts)
                leftsUpdated[left.Key] = left.Value;
        }

        private void RemoveRight(TRight right)
        {
            var key = getRightLinkKey(right);

            if (!rightItems.Remove(key))
                return;

            Dictionary<TKey, TLeft> lefts;
            if (!leftItems.TryGetValue(key, out lefts))
                return;

            foreach (var left in lefts)
                leftsUpdated[left.Key] = left.Value;
        }

        private void ClearRights()
        {
            foreach (var right in rightItems)
            {
                Dictionary<TKey, TLeft> lefts;
                if (!leftItems.TryGetValue(right.Key, out lefts))
                    continue;

                foreach (var left in lefts)
                    leftsUpdated[left.Key] = left.Value;
            }

            rightItems.Clear();
        }

        #endregion
    }
}
