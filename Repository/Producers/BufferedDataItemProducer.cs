using System;
using System.Collections.Generic;

namespace Observable.Repository.Producers
{
    public class BufferedDataItemProducer<T> : IBufferedDataProducer
    {
        private readonly Producer<T> _producer;
        private readonly List<Tuple<object, IDisposable>> _producers = new List<Tuple<object, IDisposable>>();
        private Queue<RepositoryNotification<T>> _workingQueue = new Queue<RepositoryNotification<T>>();
        private Queue<RepositoryNotification<T>> _pendingQueue = new Queue<RepositoryNotification<T>>();
        private readonly object _mutex = new object();
 
        public BufferedDataItemProducer(Producer<T> producer)
        {
            _producer = producer;
        } 

        public void Add(ActionType action, IObservable<T> observable)
        {
            IDisposable subscription = null;

            switch (action)
            {
                case ActionType.Add:
                    subscription = observable.Subscribe(OnItemAdded);
                    break;
                case ActionType.Update:
                    subscription = observable.Subscribe(OnItemUpdated);
                    break;
                case ActionType.Remove:
                    subscription = observable.Subscribe(OnItemRemoved);
                    break;
                case ActionType.Reload:
                    subscription = observable.Subscribe(OnItemReloaded);
                    break;
            }

            _producers.Add(new Tuple<object, IDisposable>(observable, subscription));
        }

        public void Add(ActionType action, IObservable<List<T>> observable)
        {
            IDisposable subscription = null;

            switch (action)
            {
                case ActionType.Add:
                    subscription = observable.Subscribe(OnItemAdded);
                    break;
                case ActionType.Update:
                    subscription = observable.Subscribe(OnItemUpdated);
                    break;
                case ActionType.Remove:
                    subscription = observable.Subscribe(OnItemRemoved);
                    break;
                case ActionType.Reload:
                    subscription = observable.Subscribe(OnItemReloaded);
                    break;
            }

            _producers.Add(new Tuple<object, IDisposable>(observable, subscription));
        }

        public void Add(IObservable<RepositoryNotification<T>> observable)
        {
            var subscription = observable.Subscribe(OnItemNotification);
            _producers.Add(new Tuple<object, IDisposable>(observable, subscription));
        }

        public void Remove(IObservable<T> observable)
        {
            InternalRemove(observable);
        }

        public void Remove(IObservable<List<T>> observable)
        {
            InternalRemove(observable);
        }

        public void Remove(IObservable<RepositoryNotification<T>> observable)
        {
            InternalRemove(observable);
        }

        private void InternalRemove(object observable)
        {
            for (var i = _producers.Count - 1; i >= 0; i--)
            {
                if (_producers[i].Item1 == observable)
                {
                    _producers[i].Item2?.Dispose();

                    _producers.RemoveAt(i);
                    break;
                }
            }
        }

        #region Buffer events

        private void OnItemAdded(T item)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Add, null, new [] { item }));
            }
        }

        private void OnItemUpdated(T item)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Update, null, new[] { item }));
            }
        }

        private void OnItemRemoved(T item)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Remove, new[] { item }, null));
            }
        }

        private void OnItemReloaded(T item)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Reload, null, new[] { item }));
            }
        }

        private void OnItemAdded(List<T> items)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Add, null, items));
            }
        }

        private void OnItemUpdated(List<T> items)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Update, null, items));
            }
        }

        private void OnItemRemoved(List<T> items)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Remove, items, null));
            }
        }

        private void OnItemReloaded(List<T> items)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Reload, null, items));
            }
        }

        private void OnItemNotification(RepositoryNotification<T> notification)
        {
            lock (_mutex)
            {
                var queue = _workingQueue;
                queue.Enqueue(notification);
            }
        }

        #endregion

        public void Flush()
        {
            Queue<RepositoryNotification<T>> queue;
            lock (_mutex)
            {
                queue = _workingQueue;
                _workingQueue = _pendingQueue;
            }

            while (queue.Count > 0)
            {
                _producer.OnNext(queue.Dequeue());
            }

            _pendingQueue = queue;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            var count = _producers.Count;
            for (var i = 0; i < count; i++)
                _producers[i].Item2?.Dispose();
            _producers.Clear();
        }

        #endregion
    }
}
