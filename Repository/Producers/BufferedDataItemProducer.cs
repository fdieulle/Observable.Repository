using System;
using System.Collections.Generic;

namespace Observable.Repository.Producers
{
    public class BufferedDataItemProducer<T> : IBufferedDataProducer
    {
        private readonly Producer<T> producer;
        private readonly List<Tuple<object, IDisposable>> producers = new List<Tuple<object, IDisposable>>();
        private Queue<RepositoryNotification<T>> workingQueue = new Queue<RepositoryNotification<T>>();
        private Queue<RepositoryNotification<T>> pendingQueue = new Queue<RepositoryNotification<T>>();
        private readonly object mutex = new object();
 
        public BufferedDataItemProducer(Producer<T> producer)
        {
            this.producer = producer;
        } 

        public void Add(ActionType action, IObservable<T> observable)
        {
            IDisposable suscription = null;

            switch (action)
            {
                case ActionType.Add:
                    suscription = observable.Subscribe(OnItemAdded);
                    break;
                case ActionType.Update:
                    suscription = observable.Subscribe(OnItemUpdated);
                    break;
                case ActionType.Remove:
                    suscription = observable.Subscribe(OnItemRemoved);
                    break;
                case ActionType.Reload:
                    suscription = observable.Subscribe(OnItemReloaded);
                    break;
            }

            producers.Add(new Tuple<object, IDisposable>(observable, suscription));
        }

        public void Add(ActionType action, IObservable<List<T>> observable)
        {
            IDisposable suscription = null;

            switch (action)
            {
                case ActionType.Add:
                    suscription = observable.Subscribe(OnItemAdded);
                    break;
                case ActionType.Update:
                    suscription = observable.Subscribe(OnItemUpdated);
                    break;
                case ActionType.Remove:
                    suscription = observable.Subscribe(OnItemRemoved);
                    break;
                case ActionType.Reload:
                    suscription = observable.Subscribe(OnItemReloaded);
                    break;
            }

            producers.Add(new Tuple<object, IDisposable>(observable, suscription));
        }

        public void Add(IObservable<RepositoryNotification<T>> observable)
        {
            var suscription = observable.Subscribe(OnItemNotification);
            producers.Add(new Tuple<object, IDisposable>(observable, suscription));
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
            for (var i = producers.Count - 1; i >= 0; i--)
            {
                if (producers[i].Item1 == observable)
                {
                    var suscription = producers[i].Item2;
                    if (suscription != null)
                        suscription.Dispose();

                    producers.RemoveAt(i);
                    break;
                }
            }
        }

        #region Buffer events

        private void OnItemAdded(T item)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Add, null, new [] { item }));
            }
        }

        private void OnItemUpdated(T item)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Update, null, new[] { item }));
            }
        }

        private void OnItemRemoved(T item)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Remove, new[] { item }, null));
            }
        }

        private void OnItemReloaded(T item)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Reload, null, new[] { item }));
            }
        }

        private void OnItemAdded(List<T> items)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Add, null, items));
            }
        }

        private void OnItemUpdated(List<T> items)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Update, null, items));
            }
        }

        private void OnItemRemoved(List<T> items)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Remove, items, null));
            }
        }

        private void OnItemReloaded(List<T> items)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(new RepositoryNotification<T>(ActionType.Reload, null, items));
            }
        }

        private void OnItemNotification(RepositoryNotification<T> notification)
        {
            lock (mutex)
            {
                var queue = workingQueue;
                queue.Enqueue(notification);
            }
        }

        #endregion

        public void Flush()
        {
            Queue<RepositoryNotification<T>> queue;
            lock (mutex)
            {
                queue = workingQueue;
                workingQueue = pendingQueue;
            }

            while (queue.Count > 0)
            {
                producer.OnNext(queue.Dequeue());
            }

            pendingQueue = queue;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            var count = producers.Count;
            for (var i = 0; i < count; i++)
            {
                var suscription = producers[i].Item2;
                if (suscription != null)
                    suscription.Dispose();
            }
            producers.Clear();
        }

        #endregion
    }
}
