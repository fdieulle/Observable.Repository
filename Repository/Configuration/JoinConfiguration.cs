using System;
using System.Collections.Generic;
using System.Linq;
using Observable.Repository.Core;
using Observable.Repository.Join;

namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Join configuration base class to store all join parameters.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of repository main source.</typeparam>
    /// <typeparam name="TRight">Type of the joined source for the repository.</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources.</typeparam>
    public abstract class JoinConfiguration<TKey, TValue, TLeft, TRight, TLinkKey> : IJoin<TKey, TValue, TLeft>
    {
        private readonly IRepositoryContainer container;
        private readonly KeyConfiguration leftLinkKey;
        private readonly KeyConfiguration rightLinkKey;

        /// <summary>
        /// Gets the right source filter.
        /// </summary>
        public Func<TRight, bool> RightFilter { get; private set; }

        /// <summary>
        /// Gets or sets the link key getter from left items source.
        /// </summary>
        public Func<TLeft, TLinkKey> LeftLinkKey { get; private set; }

        /// <summary>
        /// Gets or sets the link key getter from right items source.
        /// </summary>
        public Func<TRight, TLinkKey> RightLinkKey { get; private set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="mode">Join mode</param>
        /// <param name="rightSourceName">Right source name</param>
        /// <param name="rightFilter">Right source filter</param>
        /// <param name="rightLinkKey">Right link key getter.</param>
        /// <param name="leftLinkKey">Left link key getter.</param>
        protected JoinConfiguration(
            IRepositoryContainer container, 
            JoinMode mode,
            string rightSourceName,
            Func<TRight, bool> rightFilter,
            Func<TRight, TLinkKey> rightLinkKey,
            Func<TLeft, TLinkKey> leftLinkKey)
        {
            this.container = container;

            ValueType = typeof (TValue);
            LeftType = typeof (TLeft);
            RightType = typeof (TRight);
            LinkKeyType = typeof (TLinkKey);

            Mode = mode;
            RightSourceName = rightSourceName;
            RightFilter = rightFilter;
            RightLinkKey = rightLinkKey;
            this.rightLinkKey = new KeyConfiguration<TRight, TLinkKey>(rightLinkKey);
            LeftLinkKey = leftLinkKey;
            this.leftLinkKey = new KeyConfiguration<TLeft, TLinkKey>(leftLinkKey);
        }

        #region Implementation of IJoinConfiguration

        /// <summary>
        /// Gets the type of repository values.
        /// </summary>
        public Type ValueType { get; private set; }

        /// <summary>
        /// Gets the type of repository left items source.
        /// </summary>
        public Type LeftType { get; private set; }

        /// <summary>
        /// Gets the type of repository right items source.
        /// </summary>
        public Type RightType { get; private set; }

        /// <summary>
        /// Gets the type of repository join link key.
        /// </summary>
        public Type LinkKeyType { get; private set; }

        /// <summary>
        /// Gets the right source name.
        /// </summary>
        public string RightSourceName { get; private set; }

        /// <summary>
        /// Gets the right source filter.
        /// </summary>
        Delegate IJoinConfiguration.RightFilter { get { return RightFilter; } }

        /// <summary>
        /// Gets the join mode.
        /// </summary>
        public JoinMode Mode { get; private set; }

        /// <summary>
        /// Gets the left link key configuration
        /// </summary>
        KeyConfiguration IJoinConfiguration.LeftLinkKey { get { return leftLinkKey; } }

        /// <summary>
        /// Gets the right link key configuration
        /// </summary>
        KeyConfiguration IJoinConfiguration.RightLinkKey { get { return rightLinkKey; } }

        #endregion

        #region Implementation of IJoin<in TKey,TValue,in TLeft>

        /// <summary>
        /// Create a store instance.
        /// </summary>
        /// <param name="mutex">Mutex used by it's own <see cref="IStore{TKey,TValue,TLeft}"/>.</param>
        /// <param name="forward">Forward notifications to repository owner</param>
        /// <returns>Returns <see cref="IRepository{TKey,TValue}"/></returns>
        public IStore<TKey, TValue, TLeft> CreateStore(Mutex mutex, Action<RepositoryNotification<KeyValue<TKey, TValue>>> forward)
        {
            var repository = container.GetRepository<TLinkKey, TRight>(RightSourceName);
            if (repository != null)
                return CreateStore(
                    repository.SelectValues(), 
                    repository.Select(p => p.Value),
                    mutex, forward);

            var producer = container.GetProducer<TRight>(RightSourceName);
            return CreateStore(producer, null, mutex, forward);
        }

        #endregion

        /// <summary>
        /// Create a store instance.
        /// </summary>
        /// <param name="source">Source of the store.</param>
        /// <param name="snapshot">Snapshot for the source.</param>
        /// <param name="mutex">Mutex to be thread safe</param>
        /// <param name="forward">Forward notifications to repository owner</param>
        /// <returns>Returns the created store.</returns>
        protected abstract IStore<TKey, TValue, TLeft> CreateStore(
            IObservable<RepositoryNotification<TRight>> source, 
            IEnumerable<TRight> snapshot, 
            Mutex mutex,
            Action<RepositoryNotification<KeyValue<TKey, TValue>>> forward);
    }
}
