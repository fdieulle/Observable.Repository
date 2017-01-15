using System;
using System.Collections.Generic;

namespace Observable.Repository
{
    /// <summary>
    /// This interface define the <see cref="IList{T}"/> manager from repository notification.
    /// </summary>
    /// <typeparam name="T">Type of items view</typeparam>
    public interface IListView<T> : IObservable<RepositoryNotification<T>>, IObservable<AtomicNotification<T>>, IDisposable
    {
        /// <summary>
        /// Synchronize the <see cref="IListView{T}"/> with the <see cref="IRepository{TKey, T}"/> source.
        /// </summary>
        void Synchronize();
    }
}
