namespace Observable.Repository.Core
{
    /// <summary>
    /// Link node used by the <see cref="HashLinkedList{TKey,TValue}"/> structure.
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    public class LinkedNode<TKey, TValue>
    {
        /// <summary>
        /// Gets the next node
        /// </summary>
        public LinkedNode<TKey, TValue> _next;
        /// <summary>
        /// Gets the previous node
        /// </summary>
        public LinkedNode<TKey, TValue> _previous;
        /// <summary>
        /// Gets the key.
        /// </summary>
        public TKey _key;
        /// <summary>
        /// Gets the value.
        /// </summary>
        public TValue _value;

        /// <summary>
        /// Format the instance.
        /// </summary>
        /// <returns>Formatted instance.</returns>
        public override string ToString()
        {
            return $"[{_key}, {_value}]";
        }
    }
}
