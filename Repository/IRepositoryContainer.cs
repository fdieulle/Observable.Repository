using System;
using Observable.Repository.Builders;
using Observable.Repository.Producers;

namespace Observable.Repository
{
    /// <summary>
    /// The container allow to register, build create repositories. And add all producers sources.
    /// To see all methods defined on the <see cref="IRepositoryContainer"/> see also <see cref="RepositoryExtensions"/> class.
    /// </summary>
    public interface IRepositoryContainer : IDisposable
    {
        /// <summary>
        /// Gets the data producer.
        /// </summary>
        IDataProducer DataProducer { get; }

        /// <summary>
        /// Build a new <see cref="IRepository{TKey, TValue}"/> instance.
        /// Follow the builder methods to build correctly the repository.
        /// 
        /// The full build need Build().[Join*()].[DefineCtor()].[AddBehavior()].[Create()|Register()]
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <typeparam name="TLeft">Type of <see cref="IRepository{TKey, TValue}"/> source.</typeparam>
        /// <param name="name"><see cref="IRepository{TKey, TValue}"/> name.</param>
        /// <param name="getKey"><see cref="IRepository{TKey, TValue}"/> key getter.</param>
        /// <param name="onUpdate">Define the update method to avoid create a new instance for each updates. Null by default (Null = disabled)</param>
        /// <param name="leftSourceName">Source name for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="filter">Filter on the source for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="disposeWhenValueIsRemoved">Define if value instances should be disposed when there is removed.</param>
        /// <param name="dispatchNotifications">Dispatcher for all repository notifications.</param>
        /// <returns>The <see cref="IRepository{TKey, TValue}"/> builder.</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft> Build<TKey, TValue, TLeft>(
            string name, 
            Func<TLeft, TKey> getKey, 
            Action<TValue, TLeft> onUpdate = null,
            string leftSourceName = null, 
            Func<TLeft, bool> filter = null, 
            bool disposeWhenValueIsRemoved = false, 
            Action<Action> dispatchNotifications = null);

        /// <summary>
        /// Gets a <see cref="IRepository{TKey, TValue}"/> instance with its name.
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <param name="name"><see cref="IRepository{TKey, TValue}"/> name looking for.</param>
        /// <returns>Returns the <see cref="IRepository{TKey, TValue}"/> instance found.</returns>
        IRepository<TKey, TValue> GetRepository<TKey, TValue>(string name = null);

        /// <summary>
        /// Register a <see cref="IRepository{TKey, TValue}"/> in the container.
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <param name="repository"><see cref="IRepository{TKey, TValue}"/> to register.</param>
        /// <returns>Returns the current <see cref="IRepositoryContainer"/>.</returns>
        IRepositoryContainer Register<TKey, TValue>(IRepository<TKey, TValue> repository);
    }
}
