using System;
using System.Collections.Generic;

namespace Observable.Repository.Producers
{
    public abstract class AbstractDataProducer : IDataProducer
    {
        private readonly Dictionary<ProducerKey, IDisposable> _producers = new Dictionary<ProducerKey, IDisposable>();
        protected readonly object _mutex = new object();

        #region Implementation of IDataProducer

        /// <summary>
        /// Gets a producer multi cast subject instance specified by its type and its name.
        /// There is only one producer instance by couple of data type and name in a container.
        /// All producers which are added or removed with double key will be done inside this same instance.
        /// </summary>
        /// <typeparam name="T">Type of data published.</typeparam>
        /// <param name="name">Name of producer.</param>
        /// <returns>Gets <see cref="Producer{T}"/> instance. Returns null if not any producer has been found</returns>
        public Producer<T> GetProducer<T>(string name = null)
        {
            lock (_mutex)
            {
                var key = new ProducerKey(name, typeof(T));

                if (_producers.TryGetValue(key, out var producer))
                    return (Producer<T>)producer;

                var newProducer = CreateProducer<T>();
                _producers[key] = newProducer;

                return newProducer;
            }
        }

        public virtual IDataProducer AddProducer<T>(ActionType action, IObservable<T> producer, string name = null)
        {
            GetProducer<T>(name).Add(action, producer);
            return this;
        }

        public virtual IDataProducer AddProducer<T>(ActionType action, IObservable<List<T>> producer, string name = null)
        {
            GetProducer<T>(name).Add(action, producer);
            return this;
        }

        public virtual IDataProducer AddProducer<T>(IObservable<RepositoryNotification<T>> producer, string name = null)
        {
            GetProducer<T>(name).Add(producer);
            return this;
        }

        public virtual IDataProducer RemoveProducer<T>(IObservable<T> producer, string name = null)
        {
            GetProducer<T>(name).Remove(producer);
            return this;
        }

        public virtual IDataProducer RemoveProducer<T>(IObservable<List<T>> producer, string name = null)
        {
            GetProducer<T>(name).Remove(producer);
            return this;
        }

        public virtual IDataProducer RemoveProducer<T>(IObservable<RepositoryNotification<T>> producer, string name = null)
        {
            GetProducer<T>(name).Remove(producer);
            return this;
        }

        #endregion

        protected abstract Producer<T> CreateProducer<T>();

        #region Implementation of IDisposable

        protected virtual void DisposingSafe(){}

        public void Dispose()
        {
            lock (_mutex)
            {
                foreach (var pair in _producers)
                    pair.Value.Dispose();

                _producers.Clear();
            }
        }

        #endregion
    }
}
