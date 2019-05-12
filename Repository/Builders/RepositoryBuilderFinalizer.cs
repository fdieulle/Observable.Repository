using System;
using System.Linq;
using Observable.Repository.Configuration;

namespace Observable.Repository.Builders
{
    /// <summary>
    /// Builder finalizer which implements <see cref="IRepositoryBehaviorBuilder{TKey, TValue}"/> and <see cref="IRepositoryBuilderFinalizer{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of repository main source.</typeparam>
    public class RepositoryBuilderFinalizer<TKey, TValue, TLeft> : IRepositoryBehaviorBuilder<TKey, TValue>
    {
        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration.</param>
        public RepositoryBuilderFinalizer(IRepositoryContainer container, RepositoryConfiguration<TKey, TValue, TLeft> configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        #region Implementation of IRepositoryBehaviorBuilder<TKey,TValue>

        /// <summary>
        /// Configure a storage with rolling behavior. Which add a maximum number of items contains in the repository.
        /// </summary>
        /// <param name="rollingCount">The maximum number of items to roll.</param>
        /// <returns>Return the next building step.</returns>
        public IRepositoryBuilderFinalizer<TKey, TValue> AddRollingBehavior(int rollingCount)
        {
            _configuration.Behavior = StorageBehavior.Rolling;
            _configuration.RollingCount = rollingCount;
            return this;
        }

        /// <summary>
        /// Configure a storage with time interval behavior. 
        /// This behavior works for time series data.
        /// </summary>
        /// <param name="timeSpan">Time interval.</param>
        /// <param name="getTimestamp">The Timestamp property to apply the behavior</param>
        /// <returns>Return the next building step.</returns>
        public IRepositoryBuilderFinalizer<TKey, TValue> AddTimeIntervalBehavior(TimeSpan timeSpan, Func<TValue, DateTime> getTimestamp)
        {
            _configuration.Behavior = StorageBehavior.TimeInterval;
            _configuration.TimeInterval = timeSpan;
            _configuration.GetTimestamp = getTimestamp;
            return this;
        }

        /// <summary>
        /// Configure a storage with rolling and time interval behavior.
        /// This behavior apply rolling first then time interval.
        /// </summary>
        /// <param name="rollingCount">The maximum number of items to roll.</param>
        /// <param name="timeSpan">Time interval.</param>
        /// <param name="getTimestamp">The timestamp property to apply the time interval behavior.</param>
        /// <returns>Return the next building step.</returns>
        public IRepositoryBuilderFinalizer<TKey, TValue> AddRollingAndTimeIntervalBehavior(int rollingCount, TimeSpan timeSpan, Func<TValue, DateTime> getTimestamp)
        {
            _configuration.Behavior = StorageBehavior.RollingAndTimeInterval;
            _configuration.RollingCount = rollingCount;
            _configuration.TimeInterval = timeSpan;
            _configuration.GetTimestamp = getTimestamp;
            return this;
        }

        #endregion

        #region Implementation of IRepositoryBuilderFinalizer<TKey,TValue>

        /// <summary>
        /// Create the instance of the building repository.
        /// </summary>
        /// <returns>An instance of <see cref="IRepository{TKey, TValue}"/></returns>
        public IRepository<TKey, TValue> Create()
        {
            var repository = _container.GetRepository<TKey, TLeft>(_configuration.LeftSourceName);
            return repository != null 
                ? new Repository<TKey, TValue, TLeft>(_container, _configuration, repository.SelectValues(), repository.Select(p => p.Value)) 
                : new Repository<TKey, TValue, TLeft>(_container, _configuration, _container.GetProducer<TLeft>(_configuration.LeftSourceName), null);
        }

        /// <summary>
        /// Create an instance and register the building repository in the <see cref="IRepositoryContainer"/>
        /// </summary>
        /// <returns>Returns the <see cref="IRepositoryContainer"/> where the instance is registered.</returns>
        public IRepositoryContainer Register()
        {
            var repository = Create();
            _container.Register(repository);
            return _container;
        }

        #endregion
    }
}
