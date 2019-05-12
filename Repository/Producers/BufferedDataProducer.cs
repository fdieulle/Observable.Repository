using System;
using System.Collections.Generic;

namespace Observable.Repository.Producers
{
    public class BufferedDataProducer : AbstractDataProducer, IBufferedDataProducer
    {
        private readonly Dictionary<ProducerKey, IBufferedDataProducer> _producers = new Dictionary<ProducerKey, IBufferedDataProducer>();
        private IBufferedDataProducer[] _aProducers = new IBufferedDataProducer[0];

        #region Implementation of IBufferedDataProducer

        public void Flush()
        {
            var array = _aProducers;
            var length = array.Length;

            for (var i = 0; i < length; i++)
                array[i].Flush();
        }

        #endregion

        #region Overrides of AbstractDataProducer

        protected override Producer<T> CreateProducer<T>() => new Producer<T>(null);

        public override IDataProducer AddProducer<T>(ActionType action, IObservable<T> observable, string name = null)
        {
            lock (_mutex)
            {
                var producer = GetOrCreate<T>(name);
                if (producer == null) return this;

                producer.Add(action, observable);
            }

            return this;
        }

        public override IDataProducer AddProducer<T>(ActionType action, IObservable<List<T>> observable, string name = null)
        {
            lock (_mutex)
            {
                var producer = GetOrCreate<T>(name);
                if (producer == null) return this;

                producer.Add(action, observable);
            }

            return this;
        }

        public override IDataProducer AddProducer<T>(IObservable<RepositoryNotification<T>> observable, string name = null)
        {
            lock (_mutex)
            {
                var producer = GetOrCreate<T>(name);
                if (producer == null) return this;

                producer.Add(observable);
            }

            return this;
        }

        public override IDataProducer RemoveProducer<T>(IObservable<T> observable, string name = null)
        {
            lock (_mutex)
            {
                var producer = Get<T>(name);
                if (producer == null) return this;

                producer.Remove(observable);
            }

            return this;
        }

        public override IDataProducer RemoveProducer<T>(IObservable<List<T>> observable, string name = null)
        {
            lock (_mutex)
            {
                var producer = Get<T>(name);
                if (producer == null) return this;

                producer.Remove(observable);
            }

            return this;
        }

        public override IDataProducer RemoveProducer<T>(IObservable<RepositoryNotification<T>> observable, string name = null)
        {
            lock (_mutex)
            {
                var producer = Get<T>(name);
                if (producer == null) return this;

                producer.Remove(observable);
            }

            return this;
        }

        protected override void DisposingSafe()
        {
            var array = _aProducers;
            var length = array.Length;
            for (var i = 0; i < length; i++)
                array[i].Dispose();

            _aProducers = new IBufferedDataProducer[0];

            _producers.Clear();
        }

        #endregion

        private BufferedDataItemProducer<T> GetOrCreate<T>(string name)
        {
            var key = new ProducerKey(name, typeof(T));
            if (!_producers.TryGetValue(key, out var producer))
            {
                _producers.Add(key, producer = new BufferedDataItemProducer<T>(GetProducer<T>(name)));

                var array = _aProducers;
                var length = array.Length;
                var copy = new IBufferedDataProducer[length + 1];
                Array.Copy(array, copy, length);
                copy[length] = producer;
                _aProducers = copy;
            }

            return producer as BufferedDataItemProducer<T>;
        }

        private BufferedDataItemProducer<T> Get<T>(string name)
        {
            var key = new ProducerKey(name, typeof(T));
            if (!_producers.TryGetValue(key, out var producer))
                return null;

            return producer as BufferedDataItemProducer<T>;
        }
    }
}
