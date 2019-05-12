namespace Observable.Repository
{
    /// <summary>
    /// An atomic item notification container.
    /// </summary>
    /// <typeparam name="T">Type of item notified.</typeparam>
    public struct AtomicNotification<T>
    {
        /// <summary>
        /// Gets action type of the notification.
        /// </summary>
        public ActionType Action { get; }

        /// <summary>
        /// Gets new item.
        /// </summary>
        public T NewItem { get; }

        /// <summary>
        /// Gets old item.
        /// </summary>
        public T OldItem { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="action">Subject action.</param>
        /// <param name="oldItem">Item published.</param>
        /// <param name="newItem">Item published.</param>
        public AtomicNotification(ActionType action, T oldItem, T newItem)
        {
            Action = action;
            OldItem = oldItem;
            NewItem = newItem;
        }

        public override string ToString()
        {
            return $"[{Action}] Old: {OldItem}, New: {NewItem}";
        }
    }
}
