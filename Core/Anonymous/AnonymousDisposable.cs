using System;

namespace Observable.Anonymous
{
    public class AnonymousDisposable : IDisposable
    {
        public static readonly AnonymousDisposable Empty = new AnonymousDisposable();
        private Action _onDispose;

        public AnonymousDisposable(Action onDispose = null)
        {
            this._onDispose = onDispose;
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
            this._data = data;
            this._onDispose = onDispose;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_onDispose != null)
                _onDispose(_data);

            _onDispose = null;
            _data = default(T);
        }

        #endregion
    }
}
