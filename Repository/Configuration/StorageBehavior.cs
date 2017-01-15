namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Enumerate storage behaviors for repositories.
    /// </summary>
    public enum StorageBehavior
    {
        /// <summary>
        /// Means no one behavior used.
        /// </summary>
        None,
        /// <summary>
        /// Means rolling behavior used.
        /// </summary>
        Rolling,
        /// <summary>
        /// Means time interval behavior used.
        /// </summary>
        TimeInterval,
        /// <summary>
        /// Means rolling then time interval behavior used.
        /// </summary>
        RollingAndTimeInterval
    }
}
