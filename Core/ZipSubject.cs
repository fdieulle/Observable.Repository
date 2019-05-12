using System;
using System.Collections.Generic;
using System.Linq;

namespace Observable
{
    public class ZipSubject<T> : Subject<T>
    {
        private readonly List<KeyValuePair<IObservable<T>, IDisposable>> _producers = new List<KeyValuePair<IObservable<T>, IDisposable>>();

        public int Count
        {
            get
            {
                lock (_producers)
                {
                    return _producers.Count;
                }
            }
        }

        public void Add(IObservable<T> observable)
        {
            if (observable == null) return;

            var disposable = observable.Subscribe(OnNext, OnError);
            lock (_producers)
            {
                _producers.Add(new KeyValuePair<IObservable<T>, IDisposable>(observable, disposable));
            }
        }

        public void AddRange(IEnumerable<IObservable<T>> observables)
        {
            if (observables == null) return;

            var list = observables.Select(o => new KeyValuePair<IObservable<T>, IDisposable>(o, o.Subscribe(OnNext, OnError))).ToList();
            lock (_producers)
            {
                _producers.AddRange(list);
            }
        }

        public bool Remove(IObservable<T> observable)
        {
            lock (_producers)
            {
                return InternalRemove(observable);
            }
        }

        public void RemoveRange(IEnumerable<IObservable<T>> observables)
        {
            lock (_producers)
            {
                foreach (var observable in observables)
                {
                    InternalRemove(observable);
                }
            }
        }

        private bool InternalRemove(IObservable<T> observable)
        {
            for (var i = _producers.Count - 1; i >= 0; i--)
            {
                var producer = _producers[i];
                if (producer.Key != observable) continue;

                producer.Value.Dispose();
                _producers.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}
