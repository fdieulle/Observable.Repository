using System;
using Observable.Repository.Configuration;

namespace Observable.Repository.Builders
{
    /// <summary>
    /// Builder for <see cref="JoinMode.OneToUpdate"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public class JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight> : IJoinToUpateBuilderNode<TKey, TValue, TLeft, TRight>, IJoinBuilderNode<TKey, TValue, TLeft, TRight>
    {
        /// <summary>
        /// Gets the <see cref="IRepositoryContainer"/>
        /// </summary>
        public IRepositoryContainer Container { get; private set; }

        /// <summary>
        /// Gets the building repository configuration.
        /// </summary>
        public RepositoryConfiguration<TKey, TValue, TLeft> Configuration { get; private set; }

        /// <summary>
        /// Gets the right source name.
        /// </summary>
        public string RightSourceName { get; private set; }

        /// <summary>
        /// Gets the right source filter.
        /// </summary>
        public Func<TRight, bool> RightFilter { get; private set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        /// <param name="rightSourceName">Right source name</param>
        /// <param name="rightFilter">Right source filter</param>
        public JoinOneToUpdateBuilder(
            IRepositoryContainer container,
            RepositoryConfiguration<TKey, TValue, TLeft> configuration,
            string rightSourceName,
            Func<TRight, bool> rightFilter)
        {
            Container = container;
            Configuration = configuration;
            RightSourceName = rightSourceName;
            RightFilter = rightFilter;
        }

        #region Implementation of IJoinToUpateBuilderNode<TKey,TValue,TLeft,TRight>

        /// <summary>
        /// Gets the update methods on the repository values.
        /// </summary>
        public Func<TValue, Action<TRight>> OnUpdate { get; set; }

        /// <summary>
        /// Get next building step.
        /// </summary>
        /// <returns>Returns the next building step</returns>
        public IJoinBuilderNode<TKey, TValue, TLeft, TRight> GetNext()
        {
            return this;
        }

        #endregion

        #region Implementation of IJoinBuilderNode<TKey,TValue,TLeft,TRight>

        /// <summary>
        /// Get next building step.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <returns>Returns the next building step</returns>
        public IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> GetNext<TLinkKey>()
        {
            return new JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight, TLinkKey>(this);
        }

        #endregion
    }

    /// <summary>
    /// Builder for <see cref="JoinMode.OneToUpdate"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources</typeparam>
    public class JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight, TLinkKey> : JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight>, IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="underlying">Underlying builder step.</param>
        public JoinOneToUpdateBuilder(JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight> underlying)
            : base(underlying.Container,
            underlying.Configuration,
            underlying.RightSourceName,
            underlying.RightFilter)
        {
            OnUpdate = underlying.OnUpdate;
        }

        #region Implementation of IJoinBuilderNode<TKey,TValue,TLeft,TRight,TLinkKey>

        /// <summary>
        /// Gets or sets the link key getter from right items source.
        /// </summary>
        public Func<TRight, TLinkKey> GetRightLinkKey { get; set; }

        /// <summary>
        /// Gets or sets the link key getter from left items source.
        /// </summary>
        public Func<TLeft, TLinkKey> GetLeftLinkKey { get; set; }

        /// <summary>
        /// Build the Join configuration.
        /// </summary>
        public void Build()
        {
            Configuration.AddJoin(new JoinOneToUpdateConfiguration<TKey, TValue, TLeft, TRight, TLinkKey>(this));
        }

        #endregion
    }
}
