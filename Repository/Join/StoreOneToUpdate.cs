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

        private readonly Func<TLeft, TLinkKey> getLeftLinkKey;
        private readonly Func<TRight, TLinkKey> getRightLinkKey;
        private readonly Func<TRight, bool> rightFilter;
        private readonly Func<TValue, Action<TRight>> onUpdate;
        private readonly Action<RepositoryNotification<KeyValue<TKey, TValue>>> forward;

        private readonly Dictionary<TLinkKey, TRight> rightItems = new Dictionary<TLinkKey, TRight>();
        private readonly Pool<Dictionary<TKey, TValue>> pool = new Pool<Dictionary<TKey, TValue>>(() => new Dictionary<TKey, TValue>());
        private readonly Dictionary<TLinkKey, Dictionary<TKey, TValue>> valueItems = new Dictionary<TLinkKey, Dictionary<TKey, TValue>>();
        private readonly Pool<LinkedNode<TKey, TValue>> pool2 = new Pool<LinkedNode<TKey, TValue>>(() => new LinkedNode<TKey, TValue>());
        private readonly Dictionary<TKey, TLinkKey> keys = new Dictionary<TKey, TLinkKey>();
        private readonly HashLinkedList<TKey, TValue> valuesUpdated; 
        private readonly Mutex mutex;
        private readonly IDisposable subscribesOnRightSource;

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
            this.mutex = mutex ?? new Mutex();
            valuesUpdated = new HashLinkedList<TKey, TValue>(pool2);

            getLeftLinkKey = configuration.LeftLinkKey;
            getRightLinkKey = configuration.RightLinkKey;
            rightFilter = configuration.RightFilter;
            onUpdate = configuration.OnUpdate;
            this.forward = forward;

            if(snapshot != null)
                OnRightItemsReceived(new RepositoryNotification<TRight>(ActionType.Add, null, snapshot));
            if (source != null)
                subscribesOnRightSource = source.Subscribe(OnRightItemsReceived);
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
            if(subscribesOnRightSource != null)
                subscribesOnRightSource.Dispose();

            rightItems.Clear();
            valueItems.Clear();
            pool.Clear();
            pool2.Clear();
            keys.Clear();
            valuesUpdated.Clear();
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
            var linkKey = getLeftLinkKey(left);

            TLinkKey previousLinkKey;
            Dictionary<TKey, TValue> previousValues;
            if (keys.TryGetValue(key, out previousLinkKey)
                && !Equals(previousLinkKey, linkKey)
                && valueItems.TryGetValue(previousLinkKey, out previousValues))
                RemoveValues(key, previousLinkKey, previousValues);
            keys[key] = linkKey;

            Dictionary<TKey, TValue> values;
            if(!valueItems.TryGetValue(linkKey, out values))
                valueItems.Add(linkKey, values = pool.Get());

            values[key] = value;

            TRight right;
            if (rightItems.TryGetValue(linkKey, out right))
                onUpdate(value)(right);
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

            TLinkKey previousLinkKey;
            Dictionary<TKey, TValue> previousValues;
            if (keys.TryGetValue(key, out previousLinkKey)
                && !Equals(previousLinkKey, linkKey)
                && valueItems.TryGetValue(previousLinkKey, out previousValues))
                previousValues.Remove(key);
            keys.Remove(key);

            Dictionary<TKey, TValue> values;
            if (!valueItems.TryGetValue(linkKey, out values))
                return;

            RemoveValues(key, linkKey, values);
        }

        /// <summary>
        /// Call when the repository cleared values.
        /// </summary>
        public void LeftCleared()
        {
            foreach (var pair in valueItems)
            {
                pair.Value.Clear();
                pool.Free(pair.Value);
            }
            valueItems.Clear();
            keys.Clear();
        }

        private void RemoveValues(TKey key, TLinkKey linkKey, Dictionary<TKey, TValue> values)
        {
            values.Remove(key);
            if (values.Count != 0) return;

            valueItems.Remove(linkKey);
            pool.Free(values);
        }

        #endregion

        #region Manage Right items

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

                if (valuesUpdated.Count > 0)
                {
                    var items = valuesUpdated.Flush();
                    forward(new RepositoryNotification<KeyValue<TKey, TValue>>(ActionType.Update, items, items));
                }
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

            Dictionary<TKey, TValue> values;
            if (!valueItems.TryGetValue(key, out values))
                return;

            foreach (var pair in values)
            {
                onUpdate(pair.Value)(right);
                valuesUpdated[pair.Key] = pair.Value;
            }
        }

        private void RemoveRight(TRight right)
        {
            var key = getRightLinkKey(right);

            if (!rightItems.Remove(key))
                return;

            Dictionary<TKey, TValue> values;
            if (!valueItems.TryGetValue(key, out values))
                return;

            foreach (var pair in values)
            {
                onUpdate(pair.Value)(default(TRight));
                valuesUpdated[pair.Key] = pair.Value;
            }
        }

        private void ClearRights()
        {
            foreach (var right in rightItems)
            {
                Dictionary<TKey, TValue> values;
                if(!valueItems.TryGetValue(right.Key, out values))
                    continue;

                foreach (var pair in values)
                {
                    onUpdate(pair.Value)(default(TRight));
                    valuesUpdated[pair.Key] = pair.Value;
                }
            }
        }

        #endregion 
    }
}
