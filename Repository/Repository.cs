using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Observable.Anonymous;
using Observable.Repository.Builders;
using Observable.Repository.Collections;
using Observable.Repository.Configuration;
using Observable.Repository.Core;
using Observable.Repository.Join;

namespace Observable.Repository
{
    /// <summary>
    /// Repository.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of main source of the repository.</typeparam>
    public class Repository<TKey, TValue, TLeft> : IRepository<TKey, TValue>
    {
        #region Fields

        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;
        private readonly Subject<RepositoryNotification<KeyValue<TKey, TValue>>> _subject = new Subject<RepositoryNotification<KeyValue<TKey, TValue>>>();
        private readonly IDisposable _subscribeOnSource;

        private readonly Func<TLeft, TKey> _getLeftKey;
        private readonly Func<TLeft, bool> _leftFilter;
        private readonly Func<TLeft, object[], TValue> _ctor;
        private readonly Action<TValue, TLeft> _updateValue;

        // Build value
        private readonly int _nbCtorArguments;
        private readonly object[] _ctorArguments;
        private readonly bool _isDisposable;

        // Joins
        private readonly int _nbStores;
        private readonly IStore<TKey, TValue, TLeft>[] _stores;
        private readonly int _nbStoresToBuild;
        private readonly IStore<TKey, TValue, TLeft>[] _storesToBuild;
        private readonly IDisposable[] _storesSubscriptions;

        // Behavior
        private readonly bool _hasBehavior;
        private readonly bool _hasRollingBehavior;
        private readonly bool _hasTimeIntervalBehavior;
        private readonly Pool<LinkedNode<TKey, TLeft>> _leftPool;
        private readonly HashLinkedList<TKey, TLeft> _leftItems;
        private readonly int _rollingCount;
        private readonly TimeSpan _timeInterval;
        private readonly Func<TValue, DateTime> _getTimestamp;

        // Items and notifications
        private readonly Pool<LinkedNode<TKey, TValue>> _pool = new Pool<LinkedNode<TKey, TValue>>(() => new LinkedNode<TKey, TValue>());
        private readonly HashLinkedList<TKey, TValue> _items;
        private readonly HashLinkedList<TKey, TValue> _itemsAdded;
        private readonly HashLinkedList<TKey, TValue> _itemsUpdated;
        private readonly HashLinkedList<TKey, TValue> _itemsReplaced;
        private readonly HashLinkedList<TKey, TValue> _itemsRemoved;
        private readonly HashLinkedList<TKey, TValue> _itemsCleared;
        private IEnumerable<KeyValue<TKey, TValue>> _lazyItems;

        private readonly Action<Action> _dispatcher;
        private readonly Mutex _mutex = new Mutex();

        #endregion Fields

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container"><see cref="IRepositoryContainer"/> owner</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="source">Source</param>
        /// <param name="snapshot">Snapshot to populate the repository</param>
        public Repository(
            IRepositoryContainer container,
            RepositoryConfiguration<TKey, TValue, TLeft> configuration,
            IObservable<RepositoryNotification<TLeft>> source,
            IEnumerable<TLeft> snapshot)
        {
            _container = container;
            _configuration = configuration;

            _getLeftKey = configuration.GetKey ?? (p => default(TKey));
            _updateValue = configuration.OnUpdate;
            _leftFilter = configuration.LeftFilter ?? (p => true);
            
            var valueType = configuration.ValueType;
            _isDisposable = configuration.DisposeWhenValueIsRemoved && valueType.GetInterfaces().Any(t => t == typeof(IDisposable));

            _dispatcher = configuration.Dispatcher;

            #region Initialize Ctor

            _ctor = configuration.Ctor;
            if (_ctor == null)
            {
                var ctorArgs = new List<Type> { typeof(TLeft) };
                if (valueType.IsBaseType(configuration.LeftType))
                    _ctor = GetItSelf;
                else
                {
                    ctorArgs.AddRange(configuration.Joins
                        .Where(p => p.Mode == JoinMode.OneToBuild)
                        .Select(p => p.RightType));

                    _ctor = ctorArgs.CreateCtor<TLeft, TValue>();
                }

                configuration.Ctor = _ctor;
                configuration.CtorArguments = new ReadOnlyCollection<Type>(ctorArgs);
            }

            _nbCtorArguments = configuration.CtorArguments.Count - 1;
            _ctorArguments = new object[_nbCtorArguments];

            #endregion

            #region Initialize Stores

            var joins = configuration.Joins;
            _nbStores = joins.Count;
            _stores = new IStore<TKey, TValue, TLeft>[_nbStores];

            var filteredStores = new List<IStore<TKey, TValue, TLeft>>();
            for (var i = 0; i < _nbStores; i++)
            {
                var join = joins[i];

                var store = joins[i].CreateStore(_mutex, Forward);
                _stores[i] = store;

                if (join.Mode == JoinMode.OneToBuild)
                    filteredStores.Add(store);
            }

            _nbStoresToBuild = filteredStores.Count;
            _storesToBuild = new IStore<TKey, TValue, TLeft>[_nbStoresToBuild];
            _storesSubscriptions = new IDisposable[_nbStoresToBuild];

            for (var i = 0; i < _nbStoresToBuild; i++)
            {
                var store = filteredStores[i];
                _storesToBuild[i] = store;
                _storesSubscriptions[i] = store.Subscribe(OnItemsReceived);
            }

            #endregion // Initialize Stores

            #region Initialize Behaviors

            _rollingCount = configuration.RollingCount;
            _timeInterval = configuration.TimeInterval;
            _getTimestamp = configuration.GetTimestamp;

            var behavior = configuration.Behavior;
            _hasRollingBehavior = (behavior == StorageBehavior.Rolling || behavior == StorageBehavior.RollingAndTimeInterval) && _rollingCount > 0;
            _hasTimeIntervalBehavior = (behavior == StorageBehavior.TimeInterval || behavior == StorageBehavior.RollingAndTimeInterval) && _timeInterval > TimeSpan.Zero && _getTimestamp != null;

            _hasBehavior = _hasRollingBehavior || _hasTimeIntervalBehavior;

            if (_hasBehavior)
            {
                _leftPool = new Pool<LinkedNode<TKey, TLeft>>(() => new LinkedNode<TKey, TLeft>());
                _leftItems = new HashLinkedList<TKey, TLeft>(_leftPool);
            }

            #endregion

            _items = new HashLinkedList<TKey, TValue>(_pool);
            _itemsAdded = new HashLinkedList<TKey, TValue>(_pool);
            _itemsUpdated = new HashLinkedList<TKey, TValue>(_pool);
            _itemsReplaced = new HashLinkedList<TKey, TValue>(_pool);
            _itemsRemoved = new HashLinkedList<TKey, TValue>(_pool);
            _itemsCleared = new HashLinkedList<TKey, TValue>(_pool);

            if (snapshot != null)
                OnItemsReceived(new RepositoryNotification<TLeft>(ActionType.Add, null, snapshot));

            if (source != null)
                _subscribeOnSource = source.Subscribe(OnItemsReceived);
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Gets repository enumerator.
        /// </summary>
        /// <returns>Returns the enumerator.</returns>
        public IEnumerator<KeyValue<TKey, TValue>> GetEnumerator()
        {
            var lazy = _lazyItems;
            if (lazy == null)
            {
                lock (_mutex._input)
                {
                    lazy = _lazyItems = _items.MakeCopy();
                }
            }

            return lazy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Implementation of IObservable<out RepositoryNotification<KeyValue<TKey,TValue>>>

        /// <summary>
        /// Subscribes on <see cref="IRepository{TKey, TValue}"/> notifications.
        /// </summary>
        /// <param name="observer">Observer of notifications.</param>
        /// <returns>Returns result of the subscription. Dispose to release the subscription.</returns>
        public IDisposable Subscribe(IObserver<RepositoryNotification<KeyValue<TKey, TValue>>> observer) 
            => _subject.Subscribe(observer);

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _subscribeOnSource?.Dispose();

            lock (_mutex._input)
            {
                for (var i = 0; i < _nbStores; i++)
                {
                    _stores[i].Dispose();
                    if (i < _nbStoresToBuild)
                        _storesSubscriptions[i].Dispose();
                }

                _items.Clear((k, v) => Dispose(v));
                _lazyItems = null;
                _itemsAdded.Clear();
                _itemsRemoved.Clear();
                _itemsReplaced.Clear();
                _itemsUpdated.Clear();
                _itemsCleared.Clear();
                _pool.Clear();

                if (_hasBehavior)
                {
                    _leftItems.Clear();
                    _leftPool.Clear();
                }
            }

            _subject.OnCompleted();
        }

        #endregion

        #region Implementation of IRepository<TKey,TValue>

        /// <summary>
        /// Gets the <see cref="IRepository{TKey, TValue}"/> name.
        /// </summary>
        public string Name => _configuration.Name;

        /// <summary>
        /// Gets the <see cref="IRepository{TKey, TValue}"/> configuration.
        /// </summary>
        public IRepositoryConfiguration Configuration => _configuration;

        /// <summary>
        /// Gets the number of items in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_mutex._input)
                {
                    return _items.Count;   
                }
            }
        }

        /// <summary>
        /// Test if the key is contained in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">Key to test</param>
        /// <returns>Returns true if the <see cref="IRepository{TKey, TValue}"/> contains the key. False else.</returns>
        public bool ContainsKey(TKey key)
        {
            lock (_mutex._input)
            {
                return _items.ContainsKey(key);   
            }
        }

        /// <summary>
        /// Try get a value from a key in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">Key of the value.</param>
        /// <param name="value">The value getted.</param>
        /// <returns>Returns true if a value can be found. False else.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_mutex._input)
            {
                return _items.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Gets a a value from a key in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">Key of the value.</param>
        /// <returns>Returns the value.</returns>
        public TValue this[TKey key]
        {
            get
            {
                lock (_mutex._input)
                {
                    return _items[key];   
                }
            }
        }

        public IDisposable Subscribe(Action<RepositoryNotification<KeyValue<TKey, TValue>>> action, Func<KeyValue<TKey, TValue>, bool> filter, bool withSnapshot = false, Action<Action> dispatch = null)
        {
            if (action == null) return AnonymousDisposable.Empty;

            lock (_mutex._input)
            {
                return new Subscription(this, action, filter, withSnapshot, dispatch);
            }
        }
        
        private class Subscription : IDisposable
        {
            private Action<RepositoryNotification<KeyValue<TKey, TValue>>> _action;
            private Func<KeyValue<TKey, TValue>, bool> _filter;
            private Action<Action> _dispatch;
            private IDisposable _subscription;

            public Subscription(Repository<TKey, TValue, TLeft> repository, 
                Action<RepositoryNotification<KeyValue<TKey, TValue>>> action,
                Func<KeyValue<TKey, TValue>, bool> filter = null, 
                bool withSnapshot = false,
                Action<Action> dispatch = null)
            {
                _action = action;
                _filter = filter;
                _dispatch = dispatch;

                if (withSnapshot)
                {
                    var e = new RepositoryNotification<KeyValue<TKey, TValue>>(ActionType.Reload, null, repository);
                    if (dispatch == null) OnNext(e);
                    else DispatchOnNext(e);
                }

                _subscription = dispatch == null 
                    ? repository._subject.Subscribe(OnNext)
                    : repository._subject.Subscribe(DispatchOnNext);
            }

            private void OnNext(RepositoryNotification<KeyValue<TKey, TValue>> e)
            {
                if(_filter != null)
                    e = new RepositoryNotification<KeyValue<TKey, TValue>>(e.Action, e.OldItems.Where(_filter), e.NewItems.Where(_filter));
                _action(e);
            }

            private void DispatchOnNext(RepositoryNotification<KeyValue<TKey, TValue>> e) 
                => _dispatch(() => OnNext(e));

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (_subscription == null) return;
                _subscription.Dispose();
                _subscription = null;
                _action = null;
                _filter = null;
                _dispatch = null;
            }

            #endregion
        }

        public IDisposable Subscribe<TSelect>(Action<RepositoryNotification<TSelect>> action, Func<KeyValue<TKey, TValue>, TSelect> selector, Func<KeyValue<TKey, TValue>, bool> filter = null, bool withSnapshot = false, Action<Action> dispatch = null)
        {
            if(action == null || selector == null) return AnonymousDisposable.Empty;

            lock (_mutex._input)
            {
                return new Subscription<TSelect>(this, action, selector, filter, withSnapshot, dispatch);
            }
        }

        private class Subscription<TSelect> : IDisposable
        {
            private Action<RepositoryNotification<TSelect>> _action;
            private Func<KeyValue<TKey, TValue>, TSelect> _selector;
            private Func<KeyValue<TKey, TValue>, bool> _filter;
            private Action<Action> _dispatch;
            private IDisposable _subscription;

            public Subscription(Repository<TKey, TValue, TLeft> repository,
                Action<RepositoryNotification<TSelect>> action,
                Func<KeyValue<TKey, TValue>, TSelect> selector,
                Func<KeyValue<TKey, TValue>, bool> filter = null,
                bool withSnapshot = false,
                Action<Action> dispatch = null)
            {
                _action = action;
                _selector = selector;
                _filter = filter;
                _dispatch = dispatch;

                if (withSnapshot)
                {
                    var e = new RepositoryNotification<KeyValue<TKey, TValue>>(ActionType.Reload, null, repository);
                    if (dispatch == null) OnNext(e);
                    else DispatchOnNext(e);
                }

                _subscription = dispatch == null
                    ? repository._subject.Subscribe(OnNext)
                    : repository._subject.Subscribe(DispatchOnNext);
            }

            private void OnNext(RepositoryNotification<KeyValue<TKey, TValue>> e)
            {
                if (_filter != null)
                    e = new RepositoryNotification<KeyValue<TKey, TValue>>(e.Action, e.OldItems.Where(_filter), e.NewItems.Where(_filter));

                _action(new RepositoryNotification<TSelect>(e.Action, e.OldItems.Select(_selector), e.NewItems.Select(_selector)));
            }

            private void DispatchOnNext(RepositoryNotification<KeyValue<TKey, TValue>> e) 
                => _dispatch(() => OnNext(e));

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (_subscription == null) return;
                _subscription.Dispose();
                _subscription = null;
                _selector = null;
                _action = null;
                _filter = null;
                _dispatch = null;
            }

            #endregion
        }

        /// <summary>
        /// Subscribe a <see cref="IList{TSelect}"/> on the repository.
        /// </summary>
        /// <typeparam name="TSelect">Type of the items list.</typeparam>
        /// <param name="view">Instance of the <see cref="IList{TSelect}"/>.</param>
        /// <param name="selector">Define a selector for the <see cref="IList{TSelect}"/>.</param>
        /// <param name="filter">Filter values from <see cref="IRepository{TKey, TValue}"/>.</param>
        /// <param name="synchronize">Define if the <see cref="IList{TSelect}"/> have to be synchronized with the <see cref="IRepository{TKey, TValue}"/> during the souscription.</param>
        /// <param name="viewDispatcher">Define the dispatcher where the <see cref="IList{TSelect}"/> will be managed.</param>
        /// <returns>Returns the <see cref="IListView{TSelect}"/> instance. Dispose it to release the <see cref="IList{TSelect}"/> instance.</returns>
        public IListView<TSelect> Subscribe<TSelect>(IList<TSelect> view, Func<TValue, TSelect> selector, Predicate<TValue> filter = null, bool synchronize = true, Action<Action> viewDispatcher = null)
        {
            lock (_mutex._input)
            {
                return new ListView<TKey, TValue, TSelect>(
                   this,
                   view,
                   filter,
                   selector,
                   synchronize,
                   viewDispatcher);   
            }
        }

        /// <summary>
        /// Build a new <see cref="IRepository{TKey, TValue}"/> instance.
        /// Follow the builder methods to build correctly the repository.
        /// The source will be the current repository.
        /// 
        /// The full build need Build().[Join*()].[DefineCtor()].Configure().[Create()|Register()]
        /// </summary>
        /// <typeparam name="TOKey">Type for <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TOValue">Type for <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <param name="name"><see cref="IRepository{TKey, TValue}"/> name.</param>
        /// <param name="getKey"><see cref="IRepository{TKey, TValue}"/> key getter.</param>
        /// <param name="onUpdate">Define the update method to avoid create a new instance for each updates. Null by default (Null = disabled)</param>
        /// <param name="filter">Filter on the source for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="disposeWhenValueIsRemoved">Define if value instances should be disposed when there is removed.</param>
        /// <param name="dispatchObservers">Dispatcher which it use to notify all repository observers.</param>
        /// <returns>The <see cref="IRepository{TKey, TValue}"/> builder.</returns>
        public IRepositoryJoinBuilder<TOKey, TOValue, TValue> Build<TOKey, TOValue>(
            string name, 
            Func<TValue, TOKey> getKey, 
            Action<TOValue, TValue> onUpdate = null,
            Func<TValue, bool> filter = null, 
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchObservers = null) 
            => _container.Build(name, getKey, onUpdate, Name, filter, disposeWhenValueIsRemoved, dispatchObservers);

        #endregion

        #region Manage Items

        private void OnItemsReceived(RepositoryNotification<TLeft> e)
        {
            KeyValue<TKey, TValue>[] notifyItemsRemoved = null;
            KeyValue<TKey, TValue>[] notifyItemsUpdated = null;
            KeyValue<TKey, TValue>[] notifyItemsReplaced = null;
            KeyValue<TKey, TValue>[] notifyItemsAdded = null;
            KeyValue<TKey, TValue>[] notifyItemsCleared = null;

            lock (_mutex._output)
            {
                bool hasItemsRemoved, hasItemsUpdated, hasItemsAdded, hasItemsReloaded;

                lock (_mutex._input)
                {
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
                            ReloadItems(e.NewItems);

                            _itemsRemoved.Clear();
                            _itemsUpdated.Clear();
                            _itemsReplaced.Clear();

                            break;
                    }

                    hasItemsRemoved = _itemsRemoved.Count > 0;
                    if (hasItemsRemoved)
                    {
                        notifyItemsRemoved = _itemsRemoved.Flush();
                        _lazyItems = null;
                    }

                    hasItemsUpdated = _itemsUpdated.Count > 0;
                    if (hasItemsUpdated)
                    {
                        notifyItemsReplaced = _itemsReplaced.Flush();
                        notifyItemsUpdated = _itemsUpdated.Flush();
                        _lazyItems = null;
                    }

                    hasItemsAdded = _itemsAdded.Count > 0;
                    if (hasItemsAdded)
                    {
                        notifyItemsAdded = _itemsAdded.Flush();
                        _lazyItems = null;
                    }

                    hasItemsReloaded = _itemsCleared.Count > 0;
                    if (hasItemsReloaded)
                    {
                        notifyItemsCleared = _itemsCleared.Flush();
                        _lazyItems = null;
                    }
                }

                if (_dispatcher == null)
                {
                    if (hasItemsRemoved)
                        Notify(ActionType.Remove, notifyItemsRemoved, null);

                    if (hasItemsUpdated)
                        Notify(ActionType.Update, notifyItemsReplaced, notifyItemsUpdated);

                    if (hasItemsReloaded)
                        Notify(ActionType.Reload, notifyItemsCleared, notifyItemsAdded);
                    else if (hasItemsAdded)
                        Notify(ActionType.Add, null, notifyItemsAdded);
                }
                else
                {
                    _dispatcher(() =>
                    {
                        if (hasItemsRemoved)
                            Notify(ActionType.Remove, notifyItemsRemoved, null);

                        if (hasItemsUpdated)
                            Notify(ActionType.Update, notifyItemsReplaced, notifyItemsUpdated);

                        if (hasItemsReloaded)
                            Notify(ActionType.Reload, notifyItemsCleared, notifyItemsAdded);
                        else if (hasItemsAdded)
                            Notify(ActionType.Add, null, notifyItemsAdded);
                    });
                }
            }
        }

        private void AddItems(IEnumerable<TLeft> list)
        {
            if (list == null) return;

            foreach (var item in list)
            {
                var key = _getLeftKey(item);

                if (!_items.TryGetValue(key, out var replace)) // Add
                {
                    if (!_leftFilter(item)) continue;

                    // Build the new instance
                    for (var i = 0; i < _nbCtorArguments; i++)
                        _ctorArguments[i] = _storesToBuild[i].GetRight(item);
                    var value = _ctor(item, _ctorArguments);

                    _items.Add(key, value);
                    
                    for (var i = 0; i < _nbStores; i++)
                        _stores[i].LeftAdded(key, item, value);

                    if (!_itemsRemoved.ContainsKey(key))
                        _itemsAdded.Add(key, value);
                    else
                    {
                        _itemsReplaced[key] = _itemsRemoved[key];
                        _itemsUpdated[key] = value;
                        _itemsRemoved.Remove(key);
                    }
                }
                else // Update
                {
                    if (!_leftFilter(item))
                    {
                        // Remove if it doesn't match the filter
                        RemoveValue(key, item, replace);

                        // If the value has not been added during this loop add it to notify
                        if (!_itemsAdded.ContainsKey(key))
                        {
                            if (!_itemsUpdated.ContainsKey(key))
                                _itemsRemoved[key] = replace;
                            else _itemsRemoved[key] = _itemsReplaced[key];

                            _itemsUpdated.Remove(key);
                            _itemsReplaced.Remove(key);
                        }
                        else _itemsAdded.Remove(key);

                        continue;
                    }

                    if (_updateValue == null)
                    {
                        // Build the new instance
                        for (var i = 0; i < _nbCtorArguments; i++)
                            _ctorArguments[i] = _storesToBuild[i].GetRight(item);
                        var value = _ctor(item, _ctorArguments);

                        _items[key] = value;
                        
                        for (var i = 0; i < _nbStores; i++)
                            _stores[i].LeftAdded(key, item, value);

                        // Keep the last updated item during this loop to notify
                        if (_itemsAdded.ContainsKey(key))
                            _itemsAdded[key] = value;
                        else
                        {
                            // Keep the first replaced item during this loop to notify
                            if (!_itemsReplaced.ContainsKey(key))
                                _itemsReplaced.Add(key, replace);

                            _itemsUpdated[key] = value;
                        }
                    }
                    else
                    {
                        _itemsReplaced[key] = replace;

                        // Update the current instance
                        _updateValue(replace, item);
                        
                        _itemsUpdated[key] = replace;
                    }
                }

                if (_hasBehavior)
                    _leftItems[key] = item;
            }

            // Apply behaviors if there are configured
            if (_hasRollingBehavior)
                ApplyRollingBehavior();
            if (_hasTimeIntervalBehavior)
                ApplyTimeIntervalBehavior();
        }

        private void RemoveItems(IEnumerable<TLeft> list)
        {
            if (_items == null) return;

            foreach (var item in list)
            {
                var key = _getLeftKey(item);

                if (!_items.TryGetValue(key, out var value))
                    continue;

                _itemsRemoved[key] = value;
                RemoveValue(key, item, value);
            }
        }

        private void RemoveValue(TKey key, TLeft item, TValue value)
        {
            Dispose(value);
            if (_items.Remove(key))
                _lazyItems = null;

            for (var i = 0; i < _nbStores; i++)
                _stores[i].LeftRemoved(key, item, value);

            if (_hasBehavior)
                _leftItems.Remove(key);
        }

        private void ReloadItems(IEnumerable<TLeft> list)
        {
            _items.Clear(CleanValue);
            _lazyItems = null;

            for (var i = 0; i < _nbStores; i++)
                _stores[i].LeftCleared();

            if (_hasBehavior)
                _leftItems.Clear();

            AddItems(list);
        }

        private void CleanValue(TKey key, TValue value)
        {
            Dispose(value);
            _itemsCleared.Add(key, value);
        }

        private void Dispose(TValue value)
        {
            if (!_isDisposable) return;

            var disposable = value as IDisposable;
            disposable?.Dispose();
        }

        #endregion

        #region Manage Behaviors

        private void ApplyRollingBehavior()
        {
            var count = _items.Count;
            if (count <= _rollingCount) return;

            var delta = count - _rollingCount;

            var cursor = _items.First;
            while (delta > 0 && cursor != null)
            {
                var next = cursor._next;

                RemoveFromBehavior(cursor);

                cursor = next;
                delta--;
            }
        }

        private void ApplyTimeIntervalBehavior()
        {
            var last = _items.Last;
            if (last == null) return;

            var threshold = _getTimestamp(last._value) - _timeInterval;

            var cursor = _items.First;
            while (cursor != null && _getTimestamp(cursor._value) <= threshold)
            {
                var next = cursor._next;

                RemoveFromBehavior(cursor);

                cursor = next;
            }
        }

        private void RemoveFromBehavior(LinkedNode<TKey, TValue> node)
        {
            if (!_itemsAdded.ContainsKey(node._key))
                _itemsRemoved[node._key] = node._value;
            else _itemsAdded.Remove(node._key);

            _itemsUpdated.Remove(node._key);
            _itemsReplaced.Remove(node._key);

            // Todo : Maybe we can iterate on leftItems as items because this 2 list have to be synchronized
            var left = _leftItems[node._key];

            RemoveValue(node._key, left, node._value);
            _leftItems.Remove(node._key);
        }

        #endregion

        private void Notify(ActionType action, IEnumerable<KeyValue<TKey, TValue>> oldItems, IEnumerable<KeyValue<TKey, TValue>> newItems) 
            => _subject.OnNext(new RepositoryNotification<KeyValue<TKey, TValue>>(action, oldItems, newItems));

        private void Forward(RepositoryNotification<KeyValue<TKey, TValue>> e)
        {
            if (_dispatcher == null)
                _subject.OnNext(e);
            else _dispatcher(() => _subject.OnNext(e));
        }

        private static TValue GetItSelf(TLeft left, object[] array) 
            => (TValue)(object)left;
    }
}
