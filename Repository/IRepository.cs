using System;
using System.Collections.Generic;
using Observable.Repository.Builders;
using Observable.Repository.Configuration;
using Observable.Repository.Core;

namespace Observable.Repository
{
    /// <summary>
    /// Interface to define a repository.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public interface IRepository<TKey, TValue> : IObservable<RepositoryNotification<KeyValue<TKey, TValue>>>, IEnumerable<KeyValue<TKey, TValue>>, IDisposable
    {
        /// <summary>
        /// Gets the <see cref="IRepository{TKey, TValue}"/> name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the <see cref="IRepository{TKey, TValue}"/> confguration.
        /// </summary>
        IRepositoryConfiguration Configuration { get; }

        /// <summary>
        /// Gets the number of items in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Test if the key is contained in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">Key to test</param>
        /// <returns>Returns true if the <see cref="IRepository{TKey, TValue}"/> contains the key. False else.</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Try get a value from a key in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">Key of the value.</param>
        /// <param name="value">The value getted.</param>
        /// <returns>Returns true if a value can be found. False else.</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Gets a a value from a key in the <see cref="IRepository{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">Key of the value.</param>
        /// <returns>Retuens the value.</returns>
        TValue this[TKey key] { get; }

        /// <summary>
        /// Subscribe on <see cref="IRepository{TKey, TValue}"/> notifications and get the repository snapshot.
        /// </summary>
        /// <param name="action">Observer of notifications.</param>
        /// <param name="selector">Define a selector for notifications.</param>
        /// <param name="filter">Define a filter on notifications.</param>
        /// <param name="withSnapshot">Define if the snapshot should be send during the subscribe</param>
        /// <param name="dispatch">Dispatch the notification.</param>
        /// <returns>Returns result of the suscription. Dispose to release the suscription.</returns>
        IDisposable Subscribe<TSelect>(Action<RepositoryNotification<TSelect>> action, Func<KeyValue<TKey, TValue>, TSelect> selector, Func<KeyValue<TKey, TValue>, bool> filter = null, bool withSnapshot = false, Action<Action> dispatch = null);

        /// <summary>
        /// Subscribe on <see cref="IRepository{TKey, TValue}"/> notifications and get the repository snapshot.
        /// </summary>
        /// <param name="action">Observer of notifications.</param>
        /// <param name="filter">Define a filter on notifications.</param>
        /// <param name="withSnapshot">Define if the snapshot should be send during the subscribe</param>
        /// <param name="dispatch">Dispatch the notification.</param>
        /// <returns>Returns result of the suscription. Dispose to release the suscription.</returns>
        IDisposable Subscribe(Action<RepositoryNotification<KeyValue<TKey, TValue>>> action, Func<KeyValue<TKey, TValue>, bool> filter = null, bool withSnapshot = false, Action<Action> dispatch = null);

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
        IListView<TSelect> Subscribe<TSelect>(IList<TSelect> view, Func<TValue, TSelect> selector, Predicate<TValue> filter = null, bool synchronize = true, Action<Action> viewDispatcher = null);

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
        IRepositoryJoinBuilder<TOKey, TOValue, TValue> Build<TOKey, TOValue>(
            string name, 
            Func<TValue, TOKey> getKey, 
            Action<TOValue, TValue> onUpdate = null, 
            Func<TValue, bool> filter = null, 
            bool disposeWhenValueIsRemoved = false,
            Action<Action> dispatchObservers = null);
    }
}
