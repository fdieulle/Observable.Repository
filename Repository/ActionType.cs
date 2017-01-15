namespace Observable.Repository
{
    /// <summary>
    /// Action type during a notification.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Means add items notification
        /// </summary>
        Add,
        /// <summary>
        /// Means update items notification
        /// </summary>
        Update,
        /// <summary>
        /// Means remove items notification
        /// </summary>
        Remove,
        /// <summary>
        /// Means reload items notification
        /// </summary>
        Reload,
    }
}
