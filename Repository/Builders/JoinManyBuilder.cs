using System;
using System.Collections.Generic;
using Observable.Repository.Configuration;

namespace Observable.Repository.Builders
{
    /// <summary>
    /// Builder for <see cref="JoinMode.Many"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public class JoinManyBuilder<TKey, TValue, TLeft, TRight> : IJoinManyBuilderNode<TKey, TValue, TLeft, TRight>
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
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        /// <param name="rightSourceName">Right source name</param>
        /// <param name="rightFilter">Right source filter</param>
        public JoinManyBuilder(
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

        #region Implementation of IJoinManyBuilderNode<TKey,TValue,TLeft,TRight>

        /// <summary>
        /// Gets or sets the property list getter delegate.
        /// </summary>
        public Func<TValue, IList<TRight>> GetList { get; set; }

        /// <summary>
        /// Gets the next building step.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right primary key used to populate the list.</typeparam>
        /// <returns>Returns the next builder step.</returns>
        public IJoinManyBuilderNode<TKey, TValue, TLeft, TRight, TRightKey> GetNext<TRightKey>() 
            => new JoinManyBuilder<TKey, TValue, TLeft, TRight, TRightKey>(this);

        #endregion
    }

    /// <summary>
    /// Builder for <see cref="JoinMode.Many"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRightKey">Type of right key.</typeparam>
    public class JoinManyBuilder<TKey, TValue, TLeft, TRight, TRightKey> : JoinManyBuilder<TKey, TValue, TLeft, TRight>, IJoinManyBuilderNode<TKey, TValue, TLeft, TRight, TRightKey>, IJoinBuilderNode<TKey, TValue, TLeft, TRight>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="underlying">Underlying builder step.</param>
        public JoinManyBuilder(JoinManyBuilder<TKey, TValue, TLeft, TRight> underlying)
            : base(underlying.Container,
                   underlying.Configuration,
                   underlying.RightSourceName,
                   underlying.RightFilter)
        {
            GetList = underlying.GetList;
        }

        #region Implementation of IJoinManyBuilderNode<TKey,TValue,TLeft,TRight,TRightKey>

        /// <summary>
        /// Gets or sets the right key getter delegate
        /// </summary>
        public Func<TRight, TRightKey> GetRightKey { get; set; }

        /// <summary>
        /// Gets the next building step.
        /// </summary>
        /// <returns>Returns the next builder step.</returns>
        public IJoinBuilderNode<TKey, TValue, TLeft, TRight> GetNext() => this;

        #endregion

        #region Implementation of IJoinBuilderNode<TKey,TValue,TLeft,TRight>

        /// <summary>
        /// Get next building step.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <returns>Returns the next building step</returns>
        IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> IJoinBuilderNode<TKey, TValue, TLeft, TRight>.GetNext<TLinkKey>() 
            => new JoinManyBuilder<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey>(this);

        #endregion
    }

    ///<summary>
    /// Builder for <see cref="JoinMode.Many"/> mode.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources</typeparam>
    /// <typeparam name="TRightKey">Type of right key.</typeparam>
    public class JoinManyBuilder<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey> : JoinManyBuilder<TKey, TValue, TLeft, TRight, TRightKey>, IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="underlying">Underlying builder step.</param>
        public JoinManyBuilder(JoinManyBuilder<TKey, TValue, TLeft, TRight, TRightKey> underlying)
            : base(underlying)
        {
            GetList = underlying.GetList;
            GetRightKey = underlying.GetRightKey;
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
            => Configuration.AddJoin(new JoinManyConfiguration<TKey, TValue, TLeft, TRight, TLinkKey, TRightKey>(this));

        #endregion
    }
}
