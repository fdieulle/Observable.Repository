using System;
using Observable.Anonymous;

namespace Observable
{
    public class Observable<T> : IDisposable
    {
        private Action<T>[] _observers = new Action<T>[0];
        private readonly object _mutex = new object();

        public IDisposable Subscribe(Action<T> observer)
        {
            if (observer == null) return AnonymousDisposable.Empty;
            
            lock (_mutex)
            {
                var length = _observers.Length;
                var array = new Action<T>[length + 1];
                for (var i = 0; i < length; i++)
                    array[i] = _observers[i];
                array[length] = observer;
                _observers = array;
            }

            return new AnonymousDisposable<Action<T>>(observer, Unsubscribe);
        }

        private void Unsubscribe(Action<T> observer)
        {
            lock (_mutex)
            {
                var length = _observers.Length;
                var array = new Action<T>[length - 1];
                for (int i = 0, j = 0; i < length; i++)
                {
                    if(_observers[i] == observer) continue;
                    array[j++] = _observers[i];
                }
                _observers = array;
            }
        }

        public void Send(T value)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            var array = _observers;
            var count = array.Length;
            for (var i = 0; i < count; i++)
                array[i](value);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            lock (_mutex)
            {
                _observers = new Action<T>[0];
            }
        }

        #endregion
    }
}
