using System;
using Observable.Repository.Configuration;

namespace Observable.Repository.Builders
{
    /// <summary>
    /// Builder for <see cref="JoinMode.OneToBuild"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public class JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight> : IJoinBuilderNode<TKey, TValue, TLeft, TRight>
    {
        /// <summary>
        /// Gets the <see cref="IRepositoryContainer"/>
        /// </summary>
        public IRepositoryContainer Container { get; }

        /// <summary>
        /// Gets the building repository configuration.
        /// </summary>
        public RepositoryConfiguration<TKey, TValue, TLeft> Configuration { get; }

        /// <summary>
        /// Gets the right source name.
        /// </summary>
        public string RightSourceName { get; }

        /// <summary>
        /// Gets the right source filter.
        /// </summary>
        public Func<TRight, bool> RightFilter { get; }

        /// <summary>
        /// Ctos
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        /// <param name="rightSourceName">Right source name</param>
        /// <param name="rightFilter">Right source filter</param>
        public JoinOneToBuildBuilder(
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

        #region Implementation of IJoinBuilderNode<TKey, TValue, TLeft,TRight>

        /// <summary>
        /// Get next building step.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <returns>Returns the next building step</returns>
        public IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> GetNext<TLinkKey>() 
            => new JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight, TLinkKey>(this);

        #endregion
    }

    /// <summary>
    /// Builder for <see cref="JoinMode.OneToBuild"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    public class JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight, TLinkKey> :
        JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight>, IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="underlaying">the underlying builder</param>
        public JoinOneToBuildBuilder(JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight> underlaying)
            : base(underlaying.Container,
                   underlaying.Configuration,
                   underlaying.RightSourceName,
                   underlaying.RightFilter) { }

        #region Implementation of IJoinBuilderNode<TKey,TValue,TLeft,TRight,TLinkKey>

        /// <summary>
        /// Gets the getter link key from right source items.
        /// </summary>
        public Func<TRight, TLinkKey> GetRightLinkKey { get; set; }

        /// <summary>
        /// Gets the getter link key from left source items.
        /// </summary>
        public Func<TLeft, TLinkKey> GetLeftLinkKey { get; set; }

        /// <summary>
        /// Build the join configuration.
        /// </summary>
        public void Build() 
            => Configuration.AddJoin(new JoinOneToBuildConfiguration<TKey, TValue, TLeft, TRight, TLinkKey>(this));

        #endregion
    }
}