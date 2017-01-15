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

        private readonly IRepositoryContainer container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> configuration;
        private readonly Subject<RepositoryNotification<KeyValue<TKey, TValue>>> subject = new Subject<RepositoryNotification<KeyValue<TKey, TValue>>>();
        private readonly IDisposable subscribeOnSource;

        private readonly Func<TLeft, TKey> getLeftKey;
        private readonly Func<TLeft, bool> leftFilter;
        private readonly Func<TLeft, object[], TValue> ctor;
        private readonly Action<TValue, TLeft> updateValue;

        // Build value
        private readonly int nbCtorArguments;
        private readonly object[] ctorArguments;
        private readonly bool isDisposable;

        // Joins
        private readonly int nbStores;
        private readonly IStore<TKey, TValue, TLeft>[] stores;
        private readonly int nbStoresToBuild;
        private readonly IStore<TKey, TValue, TLeft>[] storesToBuild;
        private readonly IDisposable[] storesSuscriptions;

        // Behavior
        private readonly bool hasBehavior;
        private readonly bool hasRollingBehavior;
        private readonly bool hasTimeIntervalBehavior;
        private readonly Pool<LinkedNode<TKey, TLeft>> leftPool;
        private readonly HashLinkedList<TKey, TLeft> leftItems;
        private readonly int rollingCount;
        private readonly TimeSpan timeInterval;
        private readonly Func<TValue, DateTime> getTimestamp;

        // Items and notifications
        private readonly Pool<LinkedNode<TKey, TValue>> pool = new Pool<LinkedNode<TKey, TValue>>(() => new LinkedNode<TKey, TValue>());
        private readonly HashLinkedList<TKey, TValue> items;
        private readonly HashLinkedList<TKey, TValue> itemsAdded;
        private readonly HashLinkedList<TKey, TValue> itemsUpdated;
        private readonly HashLinkedList<TKey, TValue> itemsReplaced;
        private readonly HashLinkedList<TKey, TValue> itemsRemoved;
        private readonly HashLinkedList<TKey, TValue> itemsCleared;
        private IEnumerable<KeyValue<TKey, TValue>> lazyItems;

        private readonly Action<Action> dispatcher;
        private readonly Mutex mutex = new Mutex();

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
            this.container = container;
            this.configuration = configuration;

            getLeftKey = configuration.GetKey ?? (p => default(TKey));
            updateValue = configuration.OnUpdate;
            leftFilter = configuration.LeftFilter ?? (p => true);
            
            var valueType = configuration.ValueType;
            isDisposable = configuration.DisposeWhenValueIsRemoved && valueType.GetInterfaces().Any(t => t == typeof(IDisposable));

            dispatcher = configuration.Dispatcher;

            #region Initialize Ctor

            ctor = configuration.Ctor;
            if (ctor == null)
            {
                var ctorArgs = new List<Type> { typeof(TLeft) };
                if (valueType.IsBaseType(configuration.LeftType))
                    ctor = GetItSelf;
                else
                {
                    ctorArgs.AddRange(configuration.Joins
                        .Where(p => p.Mode == JoinMode.OneToBuild)
                        .Select(p => p.RightType));

                    ctor = ctorArgs.CreateCtor<TLeft, TValue>();
                }

                configuration.Ctor = ctor;
                configuration.CtorArguments = new ReadOnlyCollection<Type>(ctorArgs);
            }

            nbCtorArguments = configuration.CtorArguments.Count - 1;
            ctorArguments = new object[nbCtorArguments];

            #endregion

            #region Initialize Stores

            var joins = configuration.Joins;
            nbStores = joins.Count;
            stores = new IStore<TKey, TValue, TLeft>[nbStores];

            var filteredStores = new List<IStore<TKey, TValue, TLeft>>();
            for (var i = 0; i < nbStores; i++)
            {
                var join = joins[i];

                var store = joins[i].CreateStore(mutex, Forward);
                stores[i] = store;

                if (join.Mode == JoinMode.OneToBuild)
                    filteredStores.Add(store);
            }

            nbStoresToBuild = filteredStores.Count;
            storesToBuild = new IStore<TKey, TValue, TLeft>[nbStoresToBuild];
            storesSuscriptions = new IDisposable[nbStoresToBuild];

            for (var i = 0; i < nbStoresToBuild; i++)
            {
                var store = filteredStores[i];
                storesToBuild[i] = store;
                storesSuscriptions[i] = store.Subscribe(OnItemsReceived);
            }

            #endregion // Initialize Stores

            #region Initialize Behaviors

            rollingCount = configuration.RollingCount;
            timeInterval = configuration.TimeInterval;
            getTimestamp = configuration.GetTimestamp;

            var behavior = configuration.Behavior;
            hasRollingBehavior = (behavior == StorageBehavior.Rolling || behavior == StorageBehavior.RollingAndTimeInterval) && rollingCount > 0;
            hasTimeIntervalBehavior = (behavior == StorageBehavior.TimeInterval || behavior == StorageBehavior.RollingAndTimeInterval) && timeInterval > TimeSpan.Zero && getTimestamp != null;

            hasBehavior = hasRollingBehavior || hasTimeIntervalBehavior;

            if (hasBehavior)
            {
                leftPool = new Pool<LinkedNode<TKey, TLeft>>(() => new LinkedNode<TKey, TLeft>());
                leftItems = new HashLinkedList<TKey, TLeft>(leftPool);
            }

            #endregion

            items = new HashLinkedList<TKey, TValue>(pool);
            itemsAdded = new HashLinkedList<TKey, TValue>(pool);
            itemsUpdated = new HashLinkedList<TKey, TValue>(pool);
            itemsReplaced = new HashLinkedList<TKey, TValue>(pool);
            itemsRemoved = new HashLinkedList<TKey, TValue>(pool);
            itemsCleared = new HashLinkedList<TKey, TValue>(pool);

            if (snapshot != null)
                OnItemsReceived(new RepositoryNotification<TLeft>(ActionType.Add, null, snapshot));

            if (source != null)
                subscribeOnSource = source.Subscribe(OnItemsReceived);
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Gets repository enumerator.
        /// </summary>
        /// <returns>Returns the enumerator.</returns>
        public IEnumerator<KeyValue<TKey, TValue>> GetEnumerator()
        {
            var lazy = lazyItems;
            if (lazy == null)
            {
                lock (mutex.input)
                {
                    lazy = lazyItems = items.MakeCopy();
                }
            }

            return lazy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IObservable<out RepositoryNotification<KeyValue<TKey,TValue>>>

        /// <summary>
        /// Subscrive on <see cref="IRepository{TKey, TValue}"/> notifications.
        /// </summary>
        /// <param name="observer">Observer of notifications.</param>
        /// <returns>Returns result of the suscription. Dispose to release the suscription.</returns>
        public IDisposable Subscribe(IObserver<RepositoryNotification<KeyValue<TKey, TValue>>> observer)
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
            if (subscribeOnSource != null)
                subscribeOnSource.Dispose();

            lock (mutex.input)
            {
                for (var i = 0; i < nbStores; i++)
                {
                    stores[i].Dispose();
                    if (i < nbStoresToBuild)
                        storesSuscriptions[i].Dispose();
                }

                items.Clear((k, v) => Dispose(v));
                lazyItems = null;
                itemsAdded.Clear();
                itemsRemoved.Clear();
                itemsReplaced.Clear();
                itemsUpdated.Clear();
                itemsCleared.Clear();
                pool.Clear();

                if (hasBehavior)
                {
                    leftItems.Clear();
                    leftPool.Clear();
                }
            }

            subject.OnCompleted();
        }

        #endregion

        #region Implementation of IRepository<TKey,TValue>

        /// <summary>
        /// Gets the <see cref="IRepository{TKey, TValue}"/> name.
        /// </summary>
        public string Name { get { return configuration.Name; } }

        /// <summary>
        /// Gets the <see cref="IRepository{TKey, TValue}"/> confguration.
        /// </summary>
        public IRepositoryConfiguration Configuration { get { return configuration; } }

        /// <summary>
        /// Gets the number of items in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                lock (mutex.input)
                {
                    return items.Count;   
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
            lock (mutex.input)
            {
                return items.ContainsKey(key);   
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
            lock (mutex.input)
            {
                return items.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Gets a a value from a key in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">Key of the value.</param>
        /// <returns>Retuens the value.</returns>
        public TValue this[TKey key]
        {
            get
            {
                lock (mutex.input)
                {
                    return items[key];   
                }
            }
        }

        public IDisposable Subscribe(Action<RepositoryNotification<KeyValue<TKey, TValue>>> action, Func<KeyValue<TKey, TValue>, bool> filter, bool withSnapshot = false, Action<Action> dispatch = null)
        {
            if (action == null) return AnonymousDisposable.Empty;

            lock (mutex.input)
            {
                return new Suscription(this, action, filter, withSnapshot, dispatch);
            }
        }
        
        private class Suscription : IDisposable
        {
            private Action<RepositoryNotification<KeyValue<TKey, TValue>>> action;
            private Func<KeyValue<TKey, TValue>, bool> filter;
            private Action<Action> dispatch;
            private IDisposable suscription;

            public Suscription(Repository<TKey, TValue, TLeft> repository, 
                Action<RepositoryNotification<KeyValue<TKey, TValue>>> action,
                Func<KeyValue<TKey, TValue>, bool> filter = null, 
                bool withSnapshot = false,
                Action<Action> dispatch = null)
            {
                this.action = action;
                this.filter = filter;
                this.dispatch = dispatch;

                if (withSnapshot)
                {
                    var e = new RepositoryNotification<KeyValue<TKey, TValue>>(ActionType.Reload, null, repository);
                    if (dispatch == null) OnNext(e);
                    else DispatchOnNext(e);
                }

                suscription = dispatch == null 
                    ? repository.subject.Subscribe(OnNext)
                    : repository.subject.Subscribe(DispatchOnNext);
            }

            private void OnNext(RepositoryNotification<KeyValue<TKey, TValue>> e)
            {
                if(filter != null)
                    e = new RepositoryNotification<KeyValue<TKey, TValue>>(e.Action, e.OldItems.Where(filter), e.NewItems.Where(filter));
                action(e);
            }

            private void DispatchOnNext(RepositoryNotification<KeyValue<TKey, TValue>> e)
            {
                dispatch(() => OnNext(e));
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (suscription == null) return;
                suscription.Dispose();
                suscription = null;
                action = null;
                filter = null;
                dispatch = null;
            }

            #endregion
        }

        public IDisposable Subscribe<TSelect>(Action<RepositoryNotification<TSelect>> action, Func<KeyValue<TKey, TValue>, TSelect> selector, Func<KeyValue<TKey, TValue>, bool> filter = null, bool withSnapshot = false, Action<Action> dispatch = null)
        {
            if(action == null || selector == null) return AnonymousDisposable.Empty;

            lock (mutex.input)
            {
                return new Suscription<TSelect>(this, action, selector, filter, withSnapshot, dispatch);
            }
        }

        private class Suscription<TSelect> : IDisposable
        {
            private Action<RepositoryNotification<TSelect>> action;
            private Func<KeyValue<TKey, TValue>, TSelect> selector;
            private Func<KeyValue<TKey, TValue>, bool> filter;
            private Action<Action> dispatch;
            private IDisposable suscription;

            public Suscription(Repository<TKey, TValue, TLeft> repository,
                Action<RepositoryNotification<TSelect>> action,
                Func<KeyValue<TKey, TValue>, TSelect> selector,
                Func<KeyValue<TKey, TValue>, bool> filter = null,
                bool withSnapshot = false,
                Action<Action> dispatch = null)
            {
                this.action = action;
                this.selector = selector;
                this.filter = filter;
                this.dispatch = dispatch;

                if (withSnapshot)
                {
                    var e = new RepositoryNotification<KeyValue<TKey, TValue>>(ActionType.Reload, null, repository);
                    if (dispatch == null) OnNext(e);
                    else DispatchOnNext(e);
                }

                suscription = dispatch == null
                    ? repository.subject.Subscribe(OnNext)
                    : repository.subject.Subscribe(DispatchOnNext);
            }

            private void OnNext(RepositoryNotification<KeyValue<TKey, TValue>> e)
            {
                if (filter != null)
                    e = new RepositoryNotification<KeyValue<TKey, TValue>>(e.Action, e.OldItems.Where(filter), e.NewItems.Where(filter));

                action(new RepositoryNotification<TSelect>(e.Action, e.OldItems.Select(selector), e.NewItems.Select(selector)));
            }

            private void DispatchOnNext(RepositoryNotification<KeyValue<TKey, TValue>> e)
            {
                dispatch(() => OnNext(e));
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (suscription == null) return;
                suscription.Dispose();
                suscription = null;
                selector = null;
                action = null;
                filter = null;
                dispatch = null;
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
            lock (mutex.input)
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
        {
            return container.Build(name, getKey, onUpdate, Name, filter, disposeWhenValueIsRemoved, dispatchObservers);
        }

        #endregion

        #region Manage Items

        private void OnItemsReceived(RepositoryNotification<TLeft> e)
        {
            KeyValue<TKey, TValue>[] notifyItemsRemoved = null;
            KeyValue<TKey, TValue>[] notifyItemsUpdated = null;
            KeyValue<TKey, TValue>[] notifyItemsReplaced = null;
            KeyValue<TKey, TValue>[] notifyItemsAdded = null;
            KeyValue<TKey, TValue>[] notifyItemsCleared = null;

            lock (mutex.output)
            {
                bool hasItemsRemoved, hasItemsUpdated, hasItemsAdded, hasItemsReloaded;

                lock (mutex.input)
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

                            itemsRemoved.Clear();
                            itemsUpdated.Clear();
                            itemsReplaced.Clear();

                            break;
                    }

                    hasItemsRemoved = itemsRemoved.Count > 0;
                    if (hasItemsRemoved)
                    {
                        notifyItemsRemoved = itemsRemoved.Flush();
                        lazyItems = null;
                    }

                    hasItemsUpdated = itemsUpdated.Count > 0;
                    if (hasItemsUpdated)
                    {
                        notifyItemsReplaced = itemsReplaced.Flush();
                        notifyItemsUpdated = itemsUpdated.Flush();
                        lazyItems = null;
                    }

                    hasItemsAdded = itemsAdded.Count > 0;
                    if (hasItemsAdded)
                    {
                        notifyItemsAdded = itemsAdded.Flush();
                        lazyItems = null;
                    }

                    hasItemsReloaded = itemsCleared.Count > 0;
                    if (hasItemsReloaded)
                    {
                        notifyItemsCleared = itemsCleared.Flush();
                        lazyItems = null;
                    }
                }

                if (dispatcher == null)
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
                    dispatcher(() =>
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
                var key = getLeftKey(item);

                TValue replace;
                if (!items.TryGetValue(key, out replace)) // Add
                {
                    if (!leftFilter(item)) continue;

                    // Build the new instance
                    for (var i = 0; i < nbCtorArguments; i++)
                        ctorArguments[i] = storesToBuild[i].GetRight(item);
                    var value = ctor(item, ctorArguments);

                    items.Add(key, value);
                    
                    for (var i = 0; i < nbStores; i++)
                        stores[i].LeftAdded(key, item, value);

                    if (!itemsRemoved.ContainsKey(key))
                        itemsAdded.Add(key, value);
                    else
                    {
                        itemsReplaced[key] = itemsRemoved[key];
                        itemsUpdated[key] = value;
                        itemsRemoved.Remove(key);
                    }
                }
                else // Update
                {
                    if (!leftFilter(item))
                    {
                        // Remove if it doesn't match the filter
                        RemoveValue(key, item, replace);

                        // If the value has not been added during this loop add it to notify
                        if (!itemsAdded.ContainsKey(key))
                        {
                            if (!itemsUpdated.ContainsKey(key))
                                itemsRemoved[key] = replace;
                            else itemsRemoved[key] = itemsReplaced[key];

                            itemsUpdated.Remove(key);
                            itemsReplaced.Remove(key);
                        }
                        else itemsAdded.Remove(key);

                        continue;
                    }

                    if (updateValue == null)
                    {
                        // Build the new instance
                        for (var i = 0; i < nbCtorArguments; i++)
                            ctorArguments[i] = storesToBuild[i].GetRight(item);
                        var value = ctor(item, ctorArguments);

                        items[key] = value;
                        
                        for (var i = 0; i < nbStores; i++)
                            stores[i].LeftAdded(key, item, value);

                        // Keep the last updated item during this loop to notify
                        if (itemsAdded.ContainsKey(key))
                            itemsAdded[key] = value;
                        else
                        {
                            // Keep the first replaced item during this loop to notify
                            if (!itemsReplaced.ContainsKey(key))
                                itemsReplaced.Add(key, replace);

                            itemsUpdated[key] = value;
                        }
                    }
                    else
                    {
                        itemsReplaced[key] = replace;

                        // Update the current instance
                        updateValue(replace, item);
                        
                        itemsUpdated[key] = replace;
                    }
                }

                if (hasBehavior)
                    leftItems[key] = item;
            }

            // Apply behaviors if there are configured
            if (hasRollingBehavior)
                ApplyRollingBehavior();
            if (hasTimeIntervalBehavior)
                ApplyTimeIntervalBehavior();
        }

        private void RemoveItems(IEnumerable<TLeft> list)
        {
            if (items == null) return;

            foreach (var item in list)
            {
                var key = getLeftKey(item);

                TValue value;
                if (!items.TryGetValue(key, out value))
                    continue;

                itemsRemoved[key] = value;
                RemoveValue(key, item, value);
            }
        }

        private void RemoveValue(TKey key, TLeft item, TValue value)
        {
            Dispose(value);
            if (items.Remove(key))
                lazyItems = null;

            for (var i = 0; i < nbStores; i++)
                stores[i].LeftRemoved(key, item, value);

            if (hasBehavior)
                leftItems.Remove(key);
        }

        private void ReloadItems(IEnumerable<TLeft> list)
        {
            items.Clear(CleanValue);
            lazyItems = null;

            for (var i = 0; i < nbStores; i++)
                stores[i].LeftCleared();

            if (hasBehavior)
                leftItems.Clear();

            AddItems(list);
        }

        private void CleanValue(TKey key, TValue value)
        {
            Dispose(value);
            itemsCleared.Add(key, value);
        }

        private void Dispose(TValue value)
        {
            if (!isDisposable) return;

            var disposable = value as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        #endregion

        #region Manage Behaviors

        private void ApplyRollingBehavior()
        {
            var count = items.Count;
            if (count <= rollingCount) return;

            var delta = count - rollingCount;

            var cursor = items.First;
            while (delta > 0 && cursor != null)
            {
                var next = cursor.next;

                RemoveFromBehavior(cursor);

                cursor = next;
                delta--;
            }
        }

        private void ApplyTimeIntervalBehavior()
        {
            var last = items.Last;
            if (last == null) return;

            var threshold = getTimestamp(last.value) - timeInterval;

            var cursor = items.First;
            while (cursor != null && getTimestamp(cursor.value) <= threshold)
            {
                var next = cursor.next;

                RemoveFromBehavior(cursor);

                cursor = next;
            }
        }

        private void RemoveFromBehavior(LinkedNode<TKey, TValue> node)
        {
            if (!itemsAdded.ContainsKey(node.key))
                itemsRemoved[node.key] = node.value;
            else itemsAdded.Remove(node.key);

            itemsUpdated.Remove(node.key);
            itemsReplaced.Remove(node.key);

            // Todo : Maybe we can iterate on leftItems as items because this 2 list have to be synchronized
            var left = leftItems[node.key];

            RemoveValue(node.key, left, node.value);
            leftItems.Remove(node.key);
        }

        #endregion

        private void Notify(ActionType action, IEnumerable<KeyValue<TKey, TValue>> oldItems, IEnumerable<KeyValue<TKey, TValue>> newItems)
        {
            subject.OnNext(new RepositoryNotification<KeyValue<TKey, TValue>>(action, oldItems, newItems));
        }

        private void Forward(RepositoryNotification<KeyValue<TKey, TValue>> e)
        {
            if (dispatcher == null)
                subject.OnNext(e);
            else dispatcher(() => subject.OnNext(e));
        }

        private static TValue GetItSelf(TLeft left, object[] array)
        {
            return (TValue)(object)left;
        }
    }
}
