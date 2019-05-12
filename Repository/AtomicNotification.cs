namespace Observable.Repository
{
    /// <summary>
    /// An atomic item notification container.
    /// </summary>
    /// <typeparam name="T">Type of item notified.</typeparam>
    public struct AtomicNotification<T>
    {
        private readonly ActionType _action;
        private readonly T _oldItem;
        private readonly T _newItem;

        /// <summary>
        /// Gets action type of the notification.
        /// </summary>
        public ActionType Action { get { return _action; } }

        /// <summary>
        /// Gets new item.
        /// </summary>
        public T NewItem { get { return _newItem; } }

        /// <summary>
        /// Gets old item.
        /// </summary>
        public T OldItem { get { return _oldItem; } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="action">Subject action.</param>
        /// <param name="oldItem">Item published.</param>
        /// <param name="newItem">Item published.</param>
        public AtomicNotification(ActionType action, T oldItem, T newItem)
        {
            this._action = action;
            this._oldItem = oldItem;
            this._newItem = newItem;
        }

        public override string ToString()
        {
            return string.Format("[{0}] Old: {1}, New: {2}", _action, _oldItem, _newItem);
        }
    }
}
