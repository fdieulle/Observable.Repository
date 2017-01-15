using System;

namespace Observable.Repository.Builders
{
    /// <summary>
    /// Builder step to configuration storage behavior.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    public interface IRepositoryBehaviorBuilder<TKey, TValue> : IRepositoryBuilderFinalizer<TKey, TValue>
    {
        /// <summary>
        /// Configure a storage with rolling behavior. Which add a maximum number of items contains in the repository.
        /// </summary>
        /// <param name="rollingCount">The maximum number of items to roll.</param>
        /// <returns>Return the next building step.</returns>
        IRepositoryBuilderFinalizer<TKey, TValue> AddRollingBehavior(int rollingCount);

        /// <summary>
        /// Configure a storage with time interval behavior. 
        /// This behavior works for time series data.
        /// </summary>
        /// <param name="timeSpan">Time interval.</param>
        /// <param name="getTimestamp">The Timestamp property to apply the behavior</param>
        /// <returns>Return the next building step.</returns>
        IRepositoryBuilderFinalizer<TKey, TValue> AddTimeIntervalBehavior(TimeSpan timeSpan, Func<TValue, DateTime> getTimestamp);

        /// <summary>
        /// Configure a storage with rolling and time interval behavior.
        /// This behavior apply rolling first then time interval.
        /// </summary>
        /// <param name="rollingCount">The maximum number of items to roll.</param>
        /// <param name="timeSpan">Time interval.</param>
        /// <param name="getTimestamp">The timestamp property to apply the time interval behavior.</param>
        /// <returns>Return the next building step.</returns>
        IRepositoryBuilderFinalizer<TKey, TValue> AddRollingAndTimeIntervalBehavior(int rollingCount, TimeSpan timeSpan, Func<TValue, DateTime> getTimestamp);
    }
}
