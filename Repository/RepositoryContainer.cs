using System;
using System.Collections.Generic;
using Observable.Repository.Builders;
using Observable.Repository.Configuration;
using Observable.Repository.Producers;

namespace Observable.Repository
{
    /// <summary>
    /// The container allow to register, build create repositories. And add all producers sources.
    /// To see all methods definie on the <see cref="IRepositoryContainer"/> see also <see cref="RepositoryExtensions"/> class.
    /// </summary>
    public class RepositoryContainer : IRepositoryContainer
    {
        private readonly IDataProducer _dataProducer;
        private readonly Action<Type, string, object> _iocRegister;
        private readonly Dictionary<RepositoryKey, IDisposable> _repositories = new Dictionary<RepositoryKey, IDisposable>(RepositoryKey.Comparer);
        private readonly object _mutex = new object();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dataProducer">Define the <see cref="IDataProducer"/>. If Null use the <see cref="DefaultDataProducer"/></param>
        /// <param name="iocRegister">Define an IOC (Inversion Of Control) register methods where all <see cref="IRepository{TKey, TValue}"/> instance will be registered.</param>
        public RepositoryContainer(IDataProducer dataProducer = null, Action<Type, string, object> iocRegister = null)
        {
            this._dataProducer = dataProducer ?? new DefaultDataProducer(null);
            this._iocRegister = iocRegister;
        }

        #region Implementation of IRepositoryContainer

        /// <summary>
        /// Gets the data producer.
        /// </summary>
        public IDataProducer DataProducer { get { return _dataProducer; } }

        /// <summary>
        /// Build a new <see cref="IRepository{TKey, TValue}"/> instance.
        /// Follow the builder methods to build correctly the repository.
        /// 
        /// The full build need Build().[Join*()].[DefineCtor()].[AddBehavior()].[Create()|Register()]
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <typeparam name="TLeft">Type of <see cref="IRepository{TKey, TValue}"/> source.</typeparam>
        /// <param name="name"><see cref="IRepository{TKey, TValue}"/> name.</param>
        /// <param name="getKey"><see cref="IRepository{TKey, TValue}"/> key getter.</param>
        /// <param name="onUpdate">Define the update method to avoid create a new instance for each updates. Null by default (Null = disabled)</param>
        /// <param name="leftSourceName">Source name for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="filter">Filter on the source for the <see cref="IRepository{TKey, TValue}"/></param>
        /// <param name="disposeWhenValueIsRemoved">Define if value instances should be disposed when there is removed.</param>
        /// <param name="dispatchNotifications">Dispatcher for all repository notifications.</param>
        /// <returns>The <see cref="IRepository{TKey, TValue}"/> builder.</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft> Build<TKey, TValue, TLeft>(
            string name, 
            Func<TLeft, TKey> getKey,
            Action<TValue, TLeft> onUpdate = null,
            string leftSourceName = null, 
            Func<TLeft, bool> filter = null, 
            bool disposeWhenValueIsRemoved = false, 
            Action<Action> dispatchNotifications = null)
        {
            return new RepositoryJoinBuilder<TKey, TValue, TLeft>(
                this,
                new RepositoryConfiguration<TKey, TValue, TLeft>(
                    name ?? string.Empty,
                    getKey,
                    onUpdate,
                    leftSourceName,
                    filter,
                    disposeWhenValueIsRemoved,
                    dispatchNotifications));
        }

        /// <summary>
        /// Gets a <see cref="IRepository{TKey, TValue}"/> instance with its name.
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <param name="name"><see cref="IRepository{TKey, TValue}"/> name looking for.</param>
        /// <returns>Returns the <see cref="IRepository{TKey, TValue}"/> instance found.</returns>
        public IRepository<TKey, TValue> GetRepository<TKey, TValue>(string name = null)
        {
            lock (_mutex)
            {
                var key = new RepositoryKey(name, typeof(TKey), typeof(TValue));
                IDisposable repository;
                if (_repositories.TryGetValue(key, out repository))
                    return (IRepository<TKey, TValue>)repository;

                return null;
            }
        }

        /// <summary>
        /// Register a <see cref="IRepository{TKey, TValue}"/> in the container.
        /// </summary>
        /// <typeparam name="TKey">Type of <see cref="IRepository{TKey, TValue}"/> keys.</typeparam>
        /// <typeparam name="TValue">Type of <see cref="IRepository{TKey, TValue}"/> values.</typeparam>
        /// <param name="repository"><see cref="IRepository{TKey, TValue}"/> to register.</param>
        /// <returns>Returns the current <see cref="IRepositoryContainer"/>.</returns>
        public IRepositoryContainer Register<TKey, TValue>(IRepository<TKey, TValue> repository)
        {
            if (repository == null) return this;

            lock (_mutex)
            {
                var key = new RepositoryKey(repository.Name, typeof(TKey), typeof(TValue));
                _repositories[key] = repository;

                if (_iocRegister != null)
                    _iocRegister(typeof(IRepository<TKey, TValue>), repository.Name, repository);

                return this;
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            lock (_mutex)
            {
                _dataProducer.Dispose();

                foreach (var pair in _repositories)
                    pair.Value.Dispose();

                _repositories.Clear();
            }
        }

        #endregion

        #region Nested types

        private struct RepositoryKey : IEquatable<RepositoryKey>
        {
            private readonly string _name;
            private readonly Type _keyType;
            private readonly Type _valueType;
            private readonly int _hashCode;

            public RepositoryKey(string name, Type keyType, Type valueType)
            {
                this._name = name ?? string.Empty;
                this._keyType = keyType ?? typeof(object);
                this._valueType = valueType ?? typeof(object);

                unchecked
                {
                    _hashCode = this._name.GetHashCode();
                    _hashCode = (_hashCode * 397) ^ this._valueType.GetHashCode();
                    _hashCode = (_hashCode * 397) ^ this._keyType.GetHashCode();
                }
            }

            #region Equality members

            public bool Equals(RepositoryKey other)
            {
                return string.Equals(_name, other._name)
                    && _valueType == other._valueType
                    && _keyType == other._keyType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj.GetType() == GetType()
                    && Equals((RepositoryKey)obj);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            public static bool operator ==(RepositoryKey left, RepositoryKey right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(RepositoryKey left, RepositoryKey right)
            {
                return !Equals(left, right);
            }

            private sealed class RepositoryKeyEqualityComparer : IEqualityComparer<RepositoryKey>
            {
                public bool Equals(RepositoryKey x, RepositoryKey y)
                {
                    return string.Equals(x._name, y._name)
                        && x._keyType == y._keyType
                        && x._valueType == y._valueType;
                }

                public int GetHashCode(RepositoryKey obj)
                {
                    return obj._hashCode;
                }
            }

            private static readonly IEqualityComparer<RepositoryKey> comparer = new RepositoryKeyEqualityComparer();

            public static IEqualityComparer<RepositoryKey> Comparer
            {
                get { return comparer; }
            }

            #endregion
        }

        #endregion
    }
}
