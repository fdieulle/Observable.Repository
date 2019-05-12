using System;
using System.Collections.Generic;

namespace Observable.Repository.Producers
{
    /// <summary>
    /// The <see cref="Producer{T}"/> manage all producers on the same data type.
    /// It's the original source for all repositories.
    /// </summary>
    /// <typeparam name="T">Type of data to produce</typeparam>
    public class Producer<T> : ZipSubject<RepositoryNotification<T>>, IDisposable
    {
        private readonly Action<Action> _dispatch;
        private readonly List<ProducerItem> _producers = new List<ProducerItem>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dispatch">Dispatcher to produce items.</param>
        public Producer(Action<Action> dispatch)
        {
            _dispatch = dispatch;
        }

        /// <summary>
        /// On producer completed. 
        /// </summary>
        public override void OnCompleted()
        {
            if(_dispatch != null)
                _dispatch(base.OnCompleted);
            else base.OnCompleted();
        }

        /// <summary>
        /// On producer in error.
        /// </summary>
        /// <param name="error">Exception raised.</param>
        public override void OnError(Exception error)
        {
            if (_dispatch != null)
                _dispatch(() => base.OnError(error));
            else base.OnError(error);
        }

        /// <summary>
        /// On producer publish.
        /// </summary>
        /// <param name="value">Item to publish.</param>
        public override void OnNext(RepositoryNotification<T> value)
        {
            if (_dispatch != null)
                _dispatch(() => base.OnNext(value));
            else base.OnNext(value);
        }

        /// <summary>
        /// Add an atomic producer source.
        /// </summary>
        /// <param name="action">Action type of the producer</param>
        /// <param name="producer"><see cref="IObservable{T}"/> producer instance.</param>
        public void Add(ActionType action, IObservable<T> producer)
        {
            if (producer == null) return;

            var selector = producer.Select(
                    p => action == ActionType.Remove
                        ? new RepositoryNotification<T>(action, new List<T> { p }, null)
                        : new RepositoryNotification<T>(action, null, new List<T> { p }));

            Add(producer, selector);
        }

        /// <summary>
        /// Add a producer source.
        /// </summary>
        /// <param name="action">Action type of the producer</param>
        /// <param name="producer">
        ///   <see>
        ///     <cref>IObservable{IEnumerable{T}}</cref>
        ///   </see> 
        /// producer instance.</param>
        public void Add(ActionType action, IObservable<IEnumerable<T>> producer)
        {
            if (producer == null) return;

            var selector = producer.Select(
                p => action == ActionType.Remove
                         ? new RepositoryNotification<T>(action, p, null)
                         : new RepositoryNotification<T>(action, null, p));

            Add(producer, selector);
        }

        private void Add(object source, IObservable<RepositoryNotification<T>> selector)
        {
            Add(selector);
            lock (_producers)
            {
                _producers.Add(new ProducerItem { Source = source, Selector = selector });
            }
        }

        /// <summary>
        /// Remove an atomic producer.
        /// </summary>
        /// <param name="producer"><see cref="IObservable{T}"/> producer instance.</param>
        public void Remove(IObservable<T> producer) 
            => InternalRemove(producer);

        /// <summary>
        /// Remove a producer.
        /// </summary>
        /// <param name="producer">
        ///   <see>
        ///     <cref>IObservable{IEnumerable{T}}</cref>
        ///   </see>
        /// producer instance.</param>
        public void Remove(IObservable<IEnumerable<T>> producer) 
            => InternalRemove(producer);

        private void InternalRemove(object observable)
        {
            for (var i = _producers.Count - 1; i >= 0; i--)
            {
                var producer = _producers[i];
                if (producer.Source != observable) continue;
                Remove(producer.Selector);
                _producers.RemoveAt(i);
                break;
            }
        }

        private class ProducerItem
        {
            public object Source { get; set; }
            public IObservable<RepositoryNotification<T>> Selector { get; set; }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            lock (_producers)
            {
                foreach (var producer in _producers)
                    Remove(producer.Selector);
                _producers.Clear();
            }
            OnCompleted();
        }

        #endregion
    }
}
