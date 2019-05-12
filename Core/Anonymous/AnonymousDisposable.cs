using System;

namespace Observable.Anonymous
{
    public class AnonymousDisposable : IDisposable
    {
        public static readonly AnonymousDisposable Empty = new AnonymousDisposable();
        private Action _onDispose;

        public AnonymousDisposable(Action onDispose = null)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_onDispose == null) return;
            _onDispose();
            _onDispose = null;
        }
    }

    public class AnonymousDisposable<T> : IDisposable
    {
        private T _data;
        private Action<T> _onDispose;

        public AnonymousDisposable(T data = default(T), Action<T> onDispose = null)
        {
            _data = data;
            _onDispose = onDispose;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _onDispose?.Invoke(_data);
            _onDispose = null;
            _data = default(T);
        }

        #endregion
    }
}
