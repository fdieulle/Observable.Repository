using System;
using System.Collections.Generic;
using System.Linq;

namespace Observable
{
    public class ZipSubject<T> : Subject<T>
    {
        private readonly List<KeyValuePair<IObservable<T>, IDisposable>> producers = new List<KeyValuePair<IObservable<T>, IDisposable>>();

        public int Count
        {
            get
            {
                lock (producers)
                {
                    return producers.Count;
                }
            }
        }

        public void Add(IObservable<T> observable)
        {
            if (observable == null) return;

            var disposable = observable.Subscribe(OnNext, OnError);
            lock (producers)
            {
                producers.Add(new KeyValuePair<IObservable<T>, IDisposable>(observable, disposable));
            }
        }

        public void AddRange(IEnumerable<IObservable<T>> observables)
        {
            if (observables == null) return;

            var list = observables.Select(o => new KeyValuePair<IObservable<T>, IDisposable>(o, o.Subscribe(OnNext, OnError))).ToList();
            lock (producers)
            {
                producers.AddRange(list);
            }
        }

        public bool Remove(IObservable<T> observable)
        {
            lock (producers)
            {
                return InternalRemove(observable);
            }
        }

        public void RemoveRange(IEnumerable<IObservable<T>> observables)
        {
            lock (producers)
            {
                foreach (var observable in observables)
                {
                    InternalRemove(observable);
                }
            }
        }

        private bool InternalRemove(IObservable<T> observable)
        {
            for (var i = producers.Count - 1; i >= 0; i--)
            {
                var producer = producers[i];
                if (producer.Key != observable) continue;

                producer.Value.Dispose();
                producers.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}
