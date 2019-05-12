using System;
using System.Collections.Generic;
using Observable.Repository.Builders;
using Observable.Repository.Core;
using Observable.Repository.Join;

namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Join configuration to manage the right source for <see cref="JoinMode.OneToBuild"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of main source of the repository.</typeparam>
    /// <typeparam name="TRight">Type of the joined source for the repository.</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources.</typeparam>
    /// <typeparam name="TRightKey">Type of right key to populate the list.</typeparam>
    public class JoinManyConfiguration<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey> : JoinConfiguration<TKey, TValue, TLeft, TRight, TLinkKey>
    {
        /// <summary>
        /// Gets or sets the property list getter delegate.
        /// </summary>
        public Func<TValue, IList<TRight>> GetList { get; }

        /// <summary>
        /// Gets or sets the right key getter delegate
        /// </summary>
        public Func<TRight, TRightKey> GetRightKey { get; } 

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Join builder</param>
        public JoinManyConfiguration(JoinManyBuilder<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey> builder)
            : base(builder.Container,
                   JoinMode.Many,
                   builder.RightSourceName,
                   builder.RightFilter,
                   builder.GetRightLinkKey,
                   builder.GetLeftLinkKey)
        {
            GetList = builder.GetList;
            GetRightKey = builder.GetRightKey;
        }

        #region Overrides of JoinConfiguration<TKey,TValue,TLeft,TRight,TLinkKey>

        /// <summary>
        /// Create a store instance.
        /// </summary>
        /// <param name="source">Source of the store.</param>
        /// <param name="snapshot">Snapshot for the source.</param>
        /// <param name="mutex">Mutex to be thread safe</param>
        /// <param name="forward">Forward notifications to repository owner</param>
        /// <returns>Returns the created store.</returns>
        protected override IStore<TKey, TValue, TLeft> CreateStore(IObservable<RepositoryNotification<TRight>> source, IEnumerable<TRight> snapshot, Mutex mutex,
            Action<RepositoryNotification<KeyValue<TKey, TValue>>> forward) 
            => new StoreMany<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey>(this, source, snapshot, mutex);

        #endregion
    }
}
