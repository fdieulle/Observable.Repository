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

        private readonly Func<TLeft, TLinkKey> getLeftLinkKey;
        private readonly Func<TRight, TLinkKey> getRightLinkKey;
        private readonly Func<TRight, bool> rightFilter;
        private readonly Func<TValue, IList<TRight>> getList;
        private readonly Func<TRight, TRightKey> getRightKey;

        private readonly Dictionary<TRightKey, TLinkKey> rightItems = new Dictionary<TRightKey, TLinkKey>();
        private readonly Dictionary<TLinkKey, ListManager> managers = new Dictionary<TLinkKey, ListManager>();
        private readonly Pool<ListManager> managersPool;
        private readonly Pool<LinkedNode<int, TRight>> rightNodesPool = new Pool<LinkedNode<int, TRight>>(() => new LinkedNode<int, TRight>());
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
        public StoreMany(
            JoinManyConfiguration<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey> configuration,
            IObservable<RepositoryNotification<TRight>> source,
            IEnumerable<TRight> snapshot,
            Mutex mutex)
        {
            this.mutex = mutex ?? new Mutex();

            getLeftLinkKey = configuration.LeftLinkKey;
            getRightLinkKey = configuration.RightLinkKey;
            rightFilter = configuration.RightFilter;
            getList = configuration.GetList;
            getRightKey = configuration.GetRightKey;

            managersPool = new Pool<ListManager>(() => new ListManager(getList, rightNodesPool));

            if (snapshot != null)
                OnRightItemsReceived(new RepositoryNotification<TRight>(ActionType.Add, null, snapshot));
            if (source != null)
                subscribesOnRightSource = source.Subscribe(OnRightItemsReceived);
        }

        #region Implementation of IObservable<out RepositoryNotification<TLeft>>

        /// <summary>
        /// Do not use
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
            managers.Clear();
            managersPool.Clear();
            rightNodesPool.Clear();
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

            ListManager manager;
            if(!managers.TryGetValue(linkKey, out manager))
                managers.Add(linkKey, manager = managersPool.Get());

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
            var linkKey = getLeftLinkKey(left);

            ListManager manager;
            if (!managers.TryGetValue(linkKey, out manager))
                return;

            manager.RemoveValue(key);

            if (manager.IsEmpty)
            {
                managers.Remove(linkKey);
                managersPool.Free(manager);
            }
        }

        /// <summary>
        /// Call when the repository cleared values.
        /// </summary>
        public void LeftCleared()
        {
            var entriesToRemove = new KeyValuePair<TLinkKey, ListManager>[managers.Count];
            var count = 0;
            foreach (var pair in managers)
            {
                pair.Value.ClearValues();
                if (pair.Value.IsEmpty)
                    entriesToRemove[count++] = pair;
            }

            for (var i = 0; i < count; i++)
            {
                managers.Remove(entriesToRemove[i].Key);
                managersPool.Free(entriesToRemove[i].Value);
            }
        }

        #endregion

        #region Manage Right items

        private void OnRightItemsReceived(RepositoryNotification<TRight> e)
        {
            lock (mutex.input)
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
            if (rightFilter != null && !rightFilter(right))
            {
                RemoveRight(right);
                return;
            }

            var rightKey = getRightKey(right);
            var linkKey = getRightLinkKey(right);

            TLinkKey oldLinkKey;
            if (rightItems.TryGetValue(rightKey, out oldLinkKey))
            {
                // In case of the right instance has changed its link key
                if (!linkKey.Equals(oldLinkKey) && managers.ContainsKey(oldLinkKey))
                    managers[oldLinkKey].RemoveRight(rightKey);
            }

            rightItems[rightKey] = linkKey;

            ListManager manager;
            if(!managers.TryGetValue(linkKey, out manager))
                managers.Add(linkKey, manager = managersPool.Get());

            manager.AddRight(rightKey, right);
        }

        private void RemoveRight(TRight right)
        {
            var rightKey = getRightKey(right);

            if (!rightItems.Remove(rightKey))
                return;

            var linkKey = getRightLinkKey(right);

            ListManager manager;
            if (!managers.TryGetValue(linkKey, out manager))
                return;

            manager.RemoveRight(rightKey);

            if (manager.IsEmpty)
            {
                managers.Remove(linkKey);
                managersPool.Free(manager);
            }
        }

        private void ClearRight()
        {
            var entriesToRemove = new KeyValuePair<TLinkKey, ListManager>[managers.Count];
            var count = 0;
            foreach (var pair in managers)
            {
                pair.Value.ClearRights();
                if (pair.Value.IsEmpty)
                    entriesToRemove[count++] = pair;
            }

            for (var i = 0; i < count; i++)
            {
                managers.Remove(entriesToRemove[i].Key);
                managersPool.Free(entriesToRemove[i].Value);
            }

            rightItems.Clear();
        }

        private class ListManager
        {
            private readonly Func<TValue, IList<TRight>> getList;
            private readonly Pool<LinkedNode<int, TRight>> pool;
            private readonly Dictionary<TRightKey, LinkedNode<int, TRight>> rightIndices = new Dictionary<TRightKey, LinkedNode<int, TRight>>();
            private readonly Dictionary<TKey, IList<TRight>> lists = new Dictionary<TKey, IList<TRight>>();
            private LinkedNode<int, TRight> last;
            private LinkedNode<int, TRight> first;

            public bool IsEmpty
            {
                get { return rightIndices.Count == 0 && lists.Count == 0; }
            }

            public ListManager(Func<TValue, IList<TRight>> getList, Pool<LinkedNode<int, TRight>> pool)
            {
                this.getList = getList;
                this.pool = pool;
            }

            public void AddRight(TRightKey rightKey, TRight right)
            {
                LinkedNode<int, TRight> node;
                if (rightIndices.TryGetValue(rightKey, out node)) // Update lists
                {
                    node.value = right;

                    foreach (var pair in lists)
                        pair.Value[node.key] = right;
                }
                else // Add right item in lists
                {
                    node = pool.Get();
                    node.key = rightIndices.Count;
                    node.next = null;

                    if (first == null)
                        first = node;
                    if (last != null)
                        last.next = node;

                    node.previous = last;
                    last = node;

                    rightIndices.Add(rightKey, node);

                    node.value = right;

                    foreach (var pair in lists)
                        pair.Value.Add(right);
                }
            }

            public void RemoveRight(TRightKey rightKey)
            {
                LinkedNode<int, TRight> node;
                if (!rightIndices.TryGetValue(rightKey, out node)) 
                    return;

                if (node.previous == null)
                    first = node.next;
                else node.previous.next = node.next;

                if (node.next == null)
                    last = node.previous;
                else node.next.previous = node.previous;

                // Update indices
                var cursor = node.next;
                while (cursor != null)
                {
                    cursor.key -= 1;
                    cursor = cursor.next;
                }

                // Remove from lists
                var index = node.key;
                foreach (var pair in lists)
                    pair.Value.RemoveAt(index);

                // Free right node
                rightIndices.Remove(rightKey);
                node.value = default(TRight);
                pool.Free(node);
            }

            public void ClearRights()
            {
                rightIndices.Clear();
                
                var cursor = first;
                while (cursor != null)
                {
                    var next = cursor.next;
                    cursor.value = default(TRight);
                    pool.Free(cursor);

                    cursor = next;
                }
                first = null;
                last = null;

                foreach (var pair in lists)
                    pair.Value.Clear();
            }

            public void AddValue(TKey key, TValue value)
            {
                var list = getList(value);
                if (list == null) return;

                lists[key] = list;

                var cursor = first;
                while (cursor != null)
                {
                    list.Add(cursor.value);
                    cursor = cursor.next;
                }
            }

            public void RemoveValue(TKey key)
            {
                IList<TRight> list;
                if (!lists.TryGetValue(key, out list))
                    return;

                list.Clear();
                lists.Remove(key);
            }

            public void ClearValues()
            {
                foreach (var pair in lists)
                    pair.Value.Clear();
                
                lists.Clear();
            }
        }

        #endregion
    }
}
