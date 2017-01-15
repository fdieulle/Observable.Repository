namespace Observable.Repository
{
    /// <summary>
    /// An atomic item notification container.
    /// </summary>
    /// <typeparam name="T">Type of item notified.</typeparam>
    public struct AtomicNotification<T>
    {
        private readonly ActionType action;
        private readonly T oldItem;
        private readonly T newItem;

        /// <summary>
        /// Gets action type of the notification.
        /// </summary>
        public ActionType Action { get { return action; } }

        /// <summary>
        /// Gets new item.
        /// </summary>
        public T NewItem { get { return newItem; } }

        /// <summary>
        /// Gets old item.
        /// </summary>
        public T OldItem { get { return oldItem; } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="action">Subject action.</param>
        /// <param name="oldItem">Item published.</param>
        /// <param name="newItem">Item published.</param>
        public AtomicNotification(ActionType action, T oldItem, T newItem)
        {
            this.action = action;
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        public override string ToString()
        {
            return string.Format("[{0}] Old: {1}, New: {2}", action, oldItem, newItem);
        }
    }
}
