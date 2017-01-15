using System;
using System.Collections.Generic;

namespace Observable.Repository.Producers
{
    public class BufferedDataProducer : AbstractDataProducer, IBufferedDataProducer
    {
        private readonly Dictionary<ProducerKey, IBufferedDataProducer> producers = new Dictionary<ProducerKey, IBufferedDataProducer>();
        private IBufferedDataProducer[] aProducers = new IBufferedDataProducer[0];

        #region Implementation of IBufferedDataProducer

        public void Flush()
        {
            var array = aProducers;
            var length = array.Length;

            for (var i = 0; i < length; i++)
                array[i].Flush();
        }

        #endregion

        #region Overrides of AbstractDataProducer

        protected override Producer<T> CreateProducer<T>()
        {
            return new Producer<T>(null);
        }

        public override IDataProducer AddProducer<T>(ActionType action, IObservable<T> observable, string name = null)
        {
            lock (mutex)
            {
                var producer = GetOrCreate<T>(name);
                if (producer == null) return this;

                producer.Add(action, observable);
            }

            return this;
        }

        public override IDataProducer AddProducer<T>(ActionType action, IObservable<List<T>> observable, string name = null)
        {
            lock (mutex)
            {
                var producer = GetOrCreate<T>(name);
                if (producer == null) return this;

                producer.Add(action, observable);
            }

            return this;
        }

        public override IDataProducer AddProducer<T>(IObservable<RepositoryNotification<T>> observable, string name = null)
        {
            lock (mutex)
            {
                var producer = GetOrCreate<T>(name);
                if (producer == null) return this;

                producer.Add(observable);
            }

            return this;
        }

        public override IDataProducer RemoveProducer<T>(IObservable<T> observable, string name = null)
        {
            lock (mutex)
            {
                var producer = Get<T>(name);
                if (producer == null) return this;

                producer.Remove(observable);
            }

            return this;
        }

        public override IDataProducer RemoveProducer<T>(IObservable<List<T>> observable, string name = null)
        {
            lock (mutex)
            {
                var producer = Get<T>(name);
                if (producer == null) return this;

                producer.Remove(observable);
            }

            return this;
        }

        public override IDataProducer RemoveProducer<T>(IObservable<RepositoryNotification<T>> observable, string name = null)
        {
            lock (mutex)
            {
                var producer = Get<T>(name);
                if (producer == null) return this;

                producer.Remove(observable);
            }

            return this;
        }

        protected override void DisposingSafe()
        {
            var array = aProducers;
            var length = array.Length;
            for (var i = 0; i < length; i++)
                array[i].Dispose();

            aProducers = new IBufferedDataProducer[0];

            producers.Clear();
        }

        #endregion

        private BufferedDataItemProducer<T> GetOrCreate<T>(string name)
        {
            var key = new ProducerKey(name, typeof(T));
            IBufferedDataProducer producer;
            if (!producers.TryGetValue(key, out producer))
            {
                producers.Add(key, producer = new BufferedDataItemProducer<T>(GetProducer<T>(name)));

                var array = aProducers;
                var length = array.Length;
                var copy = new IBufferedDataProducer[length + 1];
                Array.Copy(array, copy, length);
                copy[length] = producer;
                aProducers = copy;
            }

            return producer as BufferedDataItemProducer<T>;
        }

        private BufferedDataItemProducer<T> Get<T>(string name)
        {
            var key = new ProducerKey(name, typeof(T));
            IBufferedDataProducer producer;
            if (!producers.TryGetValue(key, out producer))
                return null;

            return producer as BufferedDataItemProducer<T>;
        }
    }
}
