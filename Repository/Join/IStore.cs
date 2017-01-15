using System;

namespace Observable.Repository.Join
{
    /// <summary>
    /// Define the interface of a store owned by a repository to join another source.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of repository main source.</typeparam>
    public interface IStore<in TKey, in TValue, TLeft> : IObservable<RepositoryNotification<TLeft>>, IDisposable
    {
        /// <summary>
        /// Get the right item from the left.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <returns>The right instance.</returns>
        object GetRight(TLeft left);

        /// <summary>
        /// Call when the repository added new values.
        /// </summary>
        /// <param name="key">The item key</param>
        /// <param name="left">The source item</param>
        /// <param name="value">The item value</param>
        void LeftAdded(TKey key, TLeft left, TValue value);

        /// <summary>
        /// Call when the repository removed old values.
        /// </summary>
        /// <param name="key">The item key</param>
        /// <param name="left">The source item</param>
        /// <param name="value">The item value</param>
        void LeftRemoved(TKey key, TLeft left, TValue value);

        /// <summary>
        /// Call when the repository cleared values.
        /// </summary>
        void LeftCleared();
    }
}
