using System;

namespace Observable.Anonymous
{
    public class AnonymousObserver<T> : IObserver<T>
    {
        private static readonly Action<T> emptyOnNext = p => { };

        private readonly Action _onCompleted;
        private readonly Action<Exception> _onError;
        private readonly Action<T> _onNext;

        public AnonymousObserver(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            this._onNext = onNext ?? emptyOnNext;
            this._onError = onError ?? Anonymous.DefaultOnError;
            this._onCompleted = onCompleted ?? Anonymous.DefaultOnAction;
        }

        public void OnCompleted()
        {
            _onCompleted();
        }

        public void OnError(Exception error)
        {
            _onError(error);
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }
    }
}
