namespace Observable.Repository.Core
{
    /// <summary>
    /// Define a key value pair class.
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    public struct KeyValue<TKey, TValue>
    {
        internal TKey key;
        /// <summary>
        /// Gets the key.
        /// </summary>
        public TKey Key { get { return key; } }

        internal TValue value;
        /// <summary>
        /// Gets the value.
        /// </summary>
        public TValue Value { get { return value; } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public KeyValue(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        /// <summary>
        /// Display instance
        /// </summary>
        /// <returns>Data format</returns>
        public override string ToString()
        {
            return string.Format("[{0}] {1}", key, value);
        }
    }
}
