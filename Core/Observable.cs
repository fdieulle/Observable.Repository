using System;
using Observable.Anonymous;

namespace Observable
{
    public class Observable<T> : IDisposable
    {
        private Action<T>[] observers = new Action<T>[0];
        private readonly object mutex = new object();

        public IDisposable Subscribe(Action<T> observer)
        {
            if (observer == null) return AnonymousDisposable.Empty;
            
            lock (mutex)
            {
                var length = observers.Length;
                var array = new Action<T>[length + 1];
                for (var i = 0; i < length; i++)
                    array[i] = observers[i];
                array[length] = observer;
                observers = array;
            }

            return new AnonymousDisposable<Action<T>>(observer, Unsubscribe);
        }

        private void Unsubscribe(Action<T> observer)
        {
            lock (mutex)
            {
                var length = observers.Length;
                var array = new Action<T>[length - 1];
                for (int i = 0, j = 0; i < length; i++)
                {
                    if(observers[i] == observer) continue;
                    array[j++] = observers[i];
                }
                observers = array;
            }
        }

        public void Send(T value)
        {
            var array = observers;
            var count = array.Length;
            for (var i = 0; i < count; i++)
                array[i](value);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            lock (mutex)
            {
                observers = new Action<T>[0];
            }
        }

        #endregion
    }
}
