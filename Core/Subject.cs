using System;
using Observable.Anonymous;

namespace Observable
{
    public class Subject<T> : IObservable<T>, IObserver<T>
    {
        private IObserver<T>[] _observers = new IObserver<T>[0];

        #region Implementation of IObservable<out T>

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (_observers)
            {
                var array = _observers;
                var length = array.Length;
                var copy = new IObserver<T>[length + 1];
                Array.Copy(array, copy, length);
                copy[length] = observer;
                _observers = copy;
            }

            return new AnonymousDisposable<IObserver<T>>(observer, Unsubscribe);
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            lock (_observers)
            {
                var array = _observers;
                var length = array.Length;
                var copy = new IObserver<T>[Math.Max(0, length - 1)];
                var found = false;
                for (int i = 0, j = 0; i < length; i++)
                {
                    if (array[i] == observer)
                    {
                        found = true;
                        continue;
                    }
                    if (j < length - 1)
                        copy[j++] = array[i];
                }

                if (found)
                {
                    _observers = copy;
                    return;
                }

                _observers = array;
            }
        }

        #endregion

        #region Implementation of IObserver<in T>

        public virtual void OnCompleted()
        {
            try
            {
                var array = _observers;
                var length = array.Length;
                for (var i = 0; i < length; i++)
                    array[i].OnCompleted();
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                lock (_observers)
                {
                    _observers = new IObserver<T>[0];
                }
            }
        }

        public virtual void OnError(Exception error)
        {
            var array = _observers;
            var length = array.Length;
            for (var i = 0; i < length; i++)
                array[i].OnError(error);
        }

        public virtual void OnNext(T value)
        {
            var array = _observers;
            var length = array.Length;
            for (var i = 0; i < length; i++)
            {
                try
                {
                    array[i].OnNext(value);
                }
                catch (Exception e)
                {
                    array[i].OnError(e);
                }
            }

        }

        #endregion
    }
}
