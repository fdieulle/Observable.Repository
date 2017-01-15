using System;

namespace Observable.Anonymous
{
    public class AnonymousDisposable : IDisposable
    {
        public static readonly AnonymousDisposable Empty = new AnonymousDisposable();
        private Action onDispose;

        public AnonymousDisposable(Action onDispose = null)
        {
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            if (onDispose == null) return;
            onDispose();
            onDispose = null;
        }
    }

    public class AnonymousDisposable<T> : IDisposable
    {
        private T data;
        private Action<T> onDispose;

        public AnonymousDisposable(T data = default(T), Action<T> onDispose = null)
        {
            this.data = data;
            this.onDispose = onDispose;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (onDispose != null)
                onDispose(data);

            onDispose = null;
            data = default(T);
        }

        #endregion
    }
}
