using System;
using Observable.Repository.Configuration;
using Observable.Repository.Core;

namespace Observable.Repository.Join
{
    /// <summary>
    /// Join configuration interface to create the store by the repository to manage joins.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of repository main source.</typeparam>
    public interface IJoin<TKey, TValue, TLeft> : IJoinConfiguration
    {
        /// <summary>
        /// Create a store instance.
        /// </summary>
        /// <param name="mutex">Mutex used by it's own <see cref="IStore{TKey,TValue,TLeft}"/>.</param>
        /// <param name="forward">Forward notifications to repository.</param>
        /// <returns>Returns <see cref="IRepository{TKey,TValue}"/></returns>
        IStore<TKey, TValue, TLeft> CreateStore(Mutex mutex, Action<RepositoryNotification<KeyValue<TKey, TValue>>> forward);
    }
}
