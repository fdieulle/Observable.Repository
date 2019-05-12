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

        private readonly IRepository<TKey, T> _repository;
        private readonly IList<TSelect> _view;
        private readonly Predicate<T> _filter;
        private readonly Func<T, TSelect> _selector;
        private readonly Action<Action> _dispatcher;
        private readonly IDisposable _subscribesOnRepository;

        private readonly Dictionary<TKey, LinkedNode<int, TSelect>> _indices = new Dictionary<TKey, LinkedNode<int, TSelect>>();
        private readonly Pool<LinkedNode<int, TSelect>> _pool = new Pool<LinkedNode<int, TSelect>>(() => new LinkedNode<int, TSelect>());
        private LinkedNode<int, TSelect> _first;
        private LinkedNode<int, TSelect> _last;

        private readonly HashLinkedList<TKey, TSelect> _newItems;
        private readonly HashLinkedList<TKey, TSelect> _oldItems;
        private readonly Pool<LinkedNode<TKey, TSelect>> _poolToNotify = new Pool<LinkedNode<TKey, TSelect>>(() => new LinkedNode<TKey, TSelect>());
        private readonly Subject<RepositoryNotification<TSelect>> _subject = new Subject<RepositoryNotification<TSelect>>();
        private int _subscribersCount;
        private readonly Subject<AtomicNotification<TSelect>> _atomicSubject = new Subject<AtomicNotification<TSelect>>();
        private int _atomicSubscribersCount;

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
            _repository = repository;
            _view = view;
            _filter = filter;
            _selector = selector;
            _dispatcher = dispatcher;

            _newItems = new HashLinkedList<TKey, TSelect>(_poolToNotify);
            _oldItems = new HashLinkedList<TKey, TSelect>(_poolToNotify);

            if (synchronize)
                Synchronize();
            if (repository != null)
                _subscribesOnRepository = repository.Subscribe(OnItemsReceived);
        }

        #region Implementation of IListView<T>

        /// <summary>
        /// Synchronize the <see cref="IListView{T}"/> with the <see cref="IRepository{TKey, T}"/> source.
        /// </summary>
        public void Synchronize() => OnItemsReceived(new RepositoryNotification<KeyValue<TKey, T>>(ActionType.Reload, null, _repository));

        #endregion

        #region Implementation of IObservable<out RepositoryNotification<TSelect>>

        /// <summary>
        /// Subscribes on <see cref="IListView{TSelect}"/> notifications.
        /// </summary>
        /// <param name="observer">Observer of notifications.</param>
        /// <returns>Returns result of the subscription. Dispose to release the subscription.</returns>
        public IDisposable Subscribe(IObserver<RepositoryNotification<TSelect>> observer)
        {
            var result = _subject.Subscribe(observer);
            Interlocked.Increment(ref _subscribersCount);

            return new AnonymousDisposable(() =>
            {
                Interlocked.Decrement(ref _subscribersCount);
                result.Dispose();
            });
        }

        #endregion

        #region Implementation of IObservable<out AtomicNotification<TSelect>>

        /// <summary>
        /// Subscribes on <see cref="IListView{TSelect}"/> atomic notifications.
        /// </summary>
        /// <param name="observer">Observer of notifications.</param>
        /// <returns>Returns result of the subscription. Dispose to release the subscription.</returns>
        public IDisposable Subscribe(IObserver<AtomicNotification<TSelect>> observer)
        {
            var result = _atomicSubject.Subscribe(observer);
            Interlocked.Increment(ref _atomicSubscribersCount);

            return new AnonymousDisposable(() =>
            {
                Interlocked.Decrement(ref _atomicSubscribersCount);
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
            _subscribesOnRepository?.Dispose();

            if (_dispatcher != null)
                _dispatcher(Disposing);
            else Disposing();
        }

        private void Disposing()
        {
            var removed = Clear();
            if (_subscribersCount > 0)
                _subject.OnNext(new RepositoryNotification<TSelect>(ActionType.Reload, removed, null));

            _newItems.Clear();
            _oldItems.Clear();
            _poolToNotify.Clear();
            _pool.Clear();

            _subject.OnCompleted();
            _atomicSubject.OnCompleted();
        }

        #endregion

        #region Manage view

        private void OnItemsReceived(RepositoryNotification<KeyValue<TKey, T>> e)
        {
            if (_dispatcher != null)
                _dispatcher(() => ItemsReceivedDispatched(e));
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

                    if (_view is IResetableList<TSelect> resetable)
                    {
                        added = Reset(e.NewItems);
                        resetable.Reset(added);
                    }
                    else AddItems(e.NewItems);
                    break;
            }

            if (_subscribersCount > 0)
                _subject.OnNext(new RepositoryNotification<TSelect>(
                    e.Action,
                    removed ?? _oldItems.FlushValues(),
                    added ?? _newItems.FlushValues()));
        }

        private void AddItems(IEnumerable<KeyValue<TKey, T>> items)
        {
            foreach (var pair in items)
            {
                var value = pair.Value;
                if (_filter != null && !_filter(value))
                {
                    Remove(pair);
                    continue;
                }

                var key = pair.Key;
                var select = _selector(value);

                if (!_indices.TryGetValue(key, out var node))
                {
                    _indices.Add(key, node = _pool.Get());

                    node._next = null;

                    if (_first == null)
                        _first = node;

                    if (_last != null)
                        _last._next = node;

                    node._previous = _last;
                    _last = node;

                    node._value = select;
                    node._key = _view.Count;

                    _view.Add(select);

                    if (_subscribersCount > 0)
                        _newItems.Add(key, select);
                    if (_atomicSubscribersCount > 0)
                        _atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Add, default(TSelect), select));
                }
                else
                {
                    var oldValue = node._value;
                    
                    node._value = select;
                    
                    _view[node._key] = select;

                    if (_subscribersCount > 0)
                    {
                        _oldItems[key] = oldValue;
                        _newItems[key] = select;
                    }
                    if (_atomicSubscribersCount > 0)
                        _atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Update, oldValue, select));
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

            if (!_indices.TryGetValue(key, out var node))
                return;

            if (node._previous == null)
                _first = node._next;
            else node._previous._next = node._next;

            if (node._next == null)
                _last = node._previous;
            else node._next._previous = node._previous;

            var oldValue = node._value;
            node._value = default(TSelect);

            _indices.Remove(key);
            
            _view.RemoveAt(node._key);

            // Decrease indices for all next nodes
            var cursor = node._next;
            while (cursor != null)
            {
                cursor._key -= 1;
                cursor = cursor._next;
            }

            // Release node
            _pool.Free(node);

            if (_subscribersCount > 0)
                _oldItems[key] = oldValue;
            if (_atomicSubscribersCount > 0)
                _atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Remove, oldValue, default(TSelect)));
        }

        private IEnumerable<TSelect> Clear()
        {
            var notify = _subscribersCount > 0;
            var removed = notify ? new TSelect[_view.Count] : null;

            _indices.Clear();
            _view.Clear();

            var idx = 0;
            var cursor = _first;
            while (cursor != null)
            {
                var next = cursor._next;

                if (notify)
                    removed[idx++] = cursor._value;

                if(_atomicSubscribersCount > 0)
                    _atomicSubject.OnNext(new AtomicNotification<TSelect>(ActionType.Remove, cursor._value, default(TSelect)));

                cursor._value = default(TSelect);
                _pool.Free(cursor);

                cursor = next;
            }
            _first = null;
            _last = null;

            return removed;
        }

        private IEnumerable<TSelect> Reset(IEnumerable<KeyValue<TKey, T>> items)
        {
            foreach (var pair in items)
            {
                var value = pair.Value;
                if (_filter != null && !_filter(value)) continue;

                var key = pair.Key;
                var select = _selector(value);
                var oldValue = default(TSelect);
                var action = ActionType.Add;

                if (!_indices.TryGetValue(key, out var node))
                {
                    _indices.Add(key, node = _pool.Get());

                    node._key = _view.Count;
                    node._next = null;

                    if (_first == null)
                        _first = node;
                    if (_last != null)
                        _last._next = node;

                    node._previous = _last;
                    _last = node;
                }
                else
                {
                    oldValue = node._value;
                    action = ActionType.Update;
                }

                node._value = select;
                _newItems[key] = select;

                if (_atomicSubscribersCount > 0)
                    _atomicSubject.OnNext(new AtomicNotification<TSelect>(action, oldValue, select));
            }

            return _newItems.FlushValues();
        }

        #endregion
    }
}
