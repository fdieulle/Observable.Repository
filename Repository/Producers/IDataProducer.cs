using System;
using System.Collections.Generic;

namespace Observable.Repository.Producers
{
    public interface IDataProducer : IDisposable
    {
        /// <summary>
        /// Gets a producer multicast subject instance specified by its type and its name.
        /// There is only one producer instance by couple of data type and name in a container.
        /// All producers which are added or removed with double key will be done inside this same instance.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="name">Name of producer.</param>
        /// <returns>Gets <see cref="Producer{T}"/> instance. Returns null if not any producer has been found</returns>
        Producer<T> GetProducer<T>(string name = null);

        /// <summary>
        /// Add a producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="action">Action producer.</param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name.</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        IDataProducer AddProducer<T>(ActionType action, IObservable<T> producer, string name = null);

        /// <summary>
        /// Add a producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published inside a list.</typeparam>
        /// <param name="action">Action from producer.</param>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name.</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        IDataProducer AddProducer<T>(ActionType action, IObservable<List<T>> producer, string name = null);

        /// <summary>
        /// Add a producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name.</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        IDataProducer AddProducer<T>(IObservable<RepositoryNotification<T>> producer, string name = null);

        /// <summary>
        /// Remove producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name.</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        IDataProducer RemoveProducer<T>(IObservable<T> producer, string name = null);

        /// <summary>
        /// Remove producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published inside a list.</typeparam>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name.</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        IDataProducer RemoveProducer<T>(IObservable<List<T>> producer, string name = null);

        /// <summary>
        /// Remove producer publishing data for <see cref="IRepository{TKey, T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="producer">Producer which publishing data.</param>
        /// <param name="name">Producer name.</param>
        /// <returns>The current <see cref="IRepositoryContainer"/>.</returns>
        IDataProducer RemoveProducer<T>(IObservable<RepositoryNotification<T>> producer, string name = null);
    }
}
