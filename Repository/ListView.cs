using System;
using System.Collections.Generic;
using System.Threading;
using Observable.Anonymous;
using Observable.Repository.Collections;
using Observable.Repository.Core;

namespace Observable.Repository
{
    /// <summary>
    /// This class manage a <see cref="IList{TSelect}"/> from a <see cref="IRepository{TKey, TValue}"/> notifications.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="T">Type of repository values.</typeparam>
    /// <typeparam name="TSelect">Type of view values.</typeparam>
    public class ListView<TKey, T, TSelect> : IListView<TSelect>
    {
        #region Fields

        private readonly IRepository<TKey, T> repository;
        private readonly IList<TSelect> view;
        private readonly Predicate<T> filter;
        private readonly Func<T, TSelect> selector;
        private readonly Action<Action> dispatcher;
        private readonly IDisposable subscribesOnRepository;

        private readonly Dictionary<TKey, LinkedNode<int, TSelect>> indices = new Dictionary<TKey, LinkedNode<int, TSelect>>();
        private readonly Pool<LinkedNode<int, TSelect>> pool = new Pool<LinkedNode<int, TSelect>>(() => new LinkedNode<int, TSelect>());
        private LinkedNode<int, TSelect> first;
        private LinkedNode<int, TSelect> last;

        private readonly HashLinkedList<TKey, TSelect> newItems;
        private readonly HashLinkedList<TKey, TSelect> oldItems;
        private readonly Pool<LinkedNode<TKey, TSelect>> poolToNotify = new Pool<LinkedNode<TKey, TSelect>>(() => new LinkedNode<TKey, TSelect>());
        private readonly Subject<RepositoryNotification<TSelect>> subject = new Subject<RepositoryNotification<TSelect>>();
        private int subscribersCount;
        private readonly Subject<AtomicNotification<TSelect>> atomicSubject = new Subject<AtomicNotification<TSelect>>();
        private int atomicSubscribersCount;

        #endregion

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="repository">Repository source.</param>
        /// <param name="view">View to manage</param>
        /// <param name="filter">Filter on the repository.</param>
        /// <param name="selector">Selector to transform value from repository to value for the view.</param>
        /// <param name="synchronize">Indicate if the view have to be synchronized with the repository when subscribing</param>
        /// <param name="dispatcher"></param>
        public ListView(
            IRepository<TKey, T> repository,
            IList<TSelect> view,
            Predicate<T> filter,
            Func<T, TSelect> selector,
            bool synchronize,
            Action<Action> dispatcher)
        {
            this.repository = repository;
            this.view = view;
            this.filter = filter;
            this.selector = selector;
            this.dispatcher = dispatcher;

            newItems = new HashLinkedList<TKey, TSelect>(poolToNotify);
            oldItems = new HashLinkedList<TKey, TSelect>(poolToNotify);

            if (synchronize)
                Synchronize();
            if (repository != null)
                subscribesOnRepository = repository.Subscribe(OnItemsReceived);
        }

        #region Implementation of IListView<T>

        /// <summary>
        /// Synchronize the <see cref="IListView{T}"/> with the <see cref="IRepository{TKey, T}"/> source.
        /// </summary>
        public void Synchronize()
        {
            OnItemsReceived(new RepositoryNotification<KeyValue<TKey, T>>(ActionType.Reload, null, repository));
        }

        #endregion

        #region Implementation of IObservable<out RepositoryNotification<TSelect>>

        /// <summary>
        /// Subscrive on <see cref="IListView{TSelect}"/> notifications.
        /// </summary>
        /// <param name="observer">Observer of notifications.</param>
        /// <returns>Returns result of the suscription. Dispose to release the suscription.</returns>
        public IDisposable Subscribe(IObserver<RepositoryNotification<TSelect>> observer)
        {
            var result = subject.Subscribe(observer);
            Interlocked.Increment(ref subscribersCount);

            return new AnonymousDisposable(() =>
            {
                Interlocked.Decrement(ref subscribersCount);
                result.Dispose();
            });
        }

        #endregion

        #region Implementation of IObservable<out AtomicNotification<TSelect>>

        /// <summary>
        /// Subscrive on <see cref="IListView{TSelect}"/> atomic notifications.
        /// </summary>
        /// <param name="observer">Observer of notifications.</param>
        /// <returns>Returns result of the suscription. Dispose to release the suscription.</returns>
        public IDisposable Subscribe(IObserver<AtomicNotification<TSelect>> observer)
        {
            var result = atomicSubject.Subscribe(observer);
            Interlocked.Increment(ref atomicSubscribersCount);

            return new AnonymousDisposable(() =>
            {
                Interlocked.Decrement(ref atomicSubscribersCount);
                result.Dispose();
            });
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (subscribesOnRepository != null)
                subscribesOnRepository.Dispose();

            if (dispatcher != null)
                dispatcher(Disposing);
            else Disposing();
        }

        private void Disposing()
        {
            var removed = Clear();
            if (subscribersCount > 0)
                subject.OnNext(new RepositoryNotification<TSelect>(ActionType.Reload, removed, null));

            newItems.Clear();
            oldItems.Clear();
            poolToNotify.Clear();
            pool.Clear();

            subject.OnCompleted();
            atomicSubject.OnCompleted();
        }

        #endregion

        #region Manage view

        private void OnItemsReceived(RepositoryNotification<KeyValue<TKey, T>> e)
        {
            if (dispatcher != null)
                dispatcher(() => ItemsReceivedDispatched(e));
            else ItemsReceivedDispatched(e);
        }

        private void ItemsReceivedDispatched(RepositoryNotification<KeyValue<TKey, T>> e)
        {
            IEnumerable<TSelect> added = null;
            IEnumerable<TSelect> removed = null;

            switch (e.Action)
            {
                case ActionType.Add:
                case ActionType.Update:
                    AddItems(e.NewItems);
                    break;
                case ActionType.Remove:
                    RemoveItems(e.OldItems);
                    break;
                case ActionType.Reload:
                    removed = Clear();

                    var resetable = view as IResetableList<TSelect>;
                    if (resetable != null)
                    {
                        added = Reset(e.NewItems);
                        resetable.Reset(added);
                    }
                    else AddItems(e.NewItems);
                    break;
            }

            if (subscribersCount > 0)
                subject.OnNext(new RepositoryNotification<TSelect>(
                    e.Action,
                    removed ?? oldItems.FlushValues(),
                    added ?? newItems.FlushValues()));
        }

        private void AddItems(IEnumerable<KeyValue<TKey, T>> items)
        {
            foreach (var pair in items)
            {
                var value = pair.Value;
                if (filter != null && !filter(value))
                {
                    Remove(pair);
                    continue;
                }

                var key = pair.Key;
                var select = selector(value);

                LinkedNode<int, TSelect> node;
                if (!indices.TryGetValue(key, out node))
                {
                    indices.Add(key, node = pool.Get());

                    node.next = null;

                    if (first == null)
                        first = node;

                    if (last != null)
                        last.next = node;

                    node.previous = last;
                    last = node;

                    node.value = select;
                    node.key = view.Count;

                    view.Add(select);

                    if (subscribersCount > 0)
                        newItems.Add(key, select);
                    if (atomicSubscribersCount > 0)
                        atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Add, default(TSelect), select));
                }
                else
                {
                    var oldValue = node.value;
                    
                    node.value = select;
                    
                    view[node.key] = select;

                    if (subscribersCount > 0)
                    {
                        oldItems[key] = oldValue;
                        newItems[key] = select;
                    }
                    if (atomicSubscribersCount > 0)
                        atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Update, oldValue, select));
                }
            }
        }

        private void RemoveItems(IEnumerable<KeyValue<TKey, T>> items)
        {
            foreach (var pair in items)
                Remove(pair);
        }

        private void Remove(KeyValue<TKey, T> pair)
        {
            var key = pair.Key;

            LinkedNode<int, TSelect> node;
            if (!indices.TryGetValue(key, out node))
                return;

            if (node.previous == null)
                first = node.next;
            else node.previous.next = node.next;

            if (node.next == null)
                last = node.previous;
            else node.next.previous = node.previous;

            var oldValue = node.value;
            node.value = default(TSelect);

            indices.Remove(key);
            
            view.RemoveAt(node.key);

            // Decrease indices for all next nodes
            var cursor = node.next;
            while (cursor != null)
            {
                cursor.key -= 1;
                cursor = cursor.next;
            }

            // Release node
            pool.Free(node);

            if (subscribersCount > 0)
                oldItems[key] = oldValue;
            if (atomicSubscribersCount > 0)
                atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Remove, oldValue, default(TSelect)));
        }

        private IEnumerable<TSelect> Clear()
        {
            var notify = subscribersCount > 0;
            var removed = notify ? new TSelect[view.Count] : null;

            indices.Clear();
            view.Clear();

            var idx = 0;
            var cursor = first;
            while (cursor != null)
            {
                var next = cursor.next;

                if (notify)
                    removed[idx++] = cursor.value;

                if(atomicSubscribersCount > 0)
                    atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Remove, cursor.value, default(TSelect)));

                cursor.value = default(TSelect);
                pool.Free(cursor);

                cursor = next;
            }
            first = null;
            last = null;

            return removed;
        }

        private IEnumerable<TSelect> Reset(IEnumerable<KeyValue<TKey, T>> items)
        {
            foreach (var pair in items)
            {
                var value = pair.Value;
                if (filter != null && !filter(value)) continue;

                var key = pair.Key;
                var select = selector(value);
                var oldValue = default(TSelect);
                var action = ActionType.Add;

                LinkedNode<int, TSelect> node;
                if (!indices.TryGetValue(key, out node))
                {
                    indices.Add(key, node = pool.Get());

                    node.key = view.Count;
                    node.next = null;

                    if (first == null)
                        first = node;
                    if (last != null)
                        last.next = node;

                    node.previous = last;
                    last = node;
                }
                else
                {
                    oldValue = node.value;
                    action = ActionType.Update;
                }

                node.value = select;
                newItems[key] = select;

                if (atomicSubscribersCount > 0)
                    atomicSubject.OnNext(new AtomicNotification<TSelect>(action, oldValue, select));
            }

            return newItems.FlushValues();
        }

        #endregion
    }
}
