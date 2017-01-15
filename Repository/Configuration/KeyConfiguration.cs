using System;

namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Define a key getter configuration.
    /// </summary>
    public class KeyConfiguration
    {
        /// <summary>
        /// Gets the type to get the property value.
        /// </summary>
        public Type FromType { get; private set; }
        /// <summary>
        /// Gets the type of the key value.
        /// </summary>
        public Type KeyType { get; private set; }
        /// <summary>
        /// Gets the key getter delegate.
        /// </summary>
        public Delegate GetKey { get; private set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="fromType">From type</param>
        /// <param name="keyType">Key type</param>
        /// <param name="getKey">Key getter delegate</param>
        public KeyConfiguration(Type fromType, Type keyType, Delegate getKey)
        {
            FromType = fromType;
            KeyType = keyType;
            GetKey = getKey;
        }
    }

    /// <summary>
    /// Define a key getter configuration.
    /// </summary>
    /// <typeparam name="T">Type to get the key value.</typeparam>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    public class KeyConfiguration<T, TKey> : KeyConfiguration
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="getKey">Key getter delegate</param>
        public KeyConfiguration(Func<T, TKey> getKey)
            : base(typeof(T), typeof(TKey), getKey) { }
    }
}
