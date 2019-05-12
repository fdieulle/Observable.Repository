namespace Observable.Repository.Core
{
    /// <summary>
    /// Define a key value pair class.
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    public struct KeyValue<TKey, TValue>
    {
        internal TKey _key;
        /// <summary>
        /// Gets the key.
        /// </summary>
        public TKey Key { get { return _key; } }

        internal TValue _value;
        /// <summary>
        /// Gets the value.
        /// </summary>
        public TValue Value { get { return _value; } }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public KeyValue(TKey key, TValue value)
        {
            this._key = key;
            this._value = value;
        }

        /// <summary>
        /// Display instance
        /// </summary>
        /// <returns>Data format</returns>
        public override string ToString()
        {
            return string.Format("[{0}] {1}", _key, _value);
        }
    }
}
