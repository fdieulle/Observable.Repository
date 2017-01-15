using System;

namespace Observable.Anonymous
{
    public class AnonymousObserver<T> : IObserver<T>
    {
        private static readonly Action<T> emptyOnNext = p => { };

        private readonly Action onCompleted;
        private readonly Action<Exception> onError;
        private readonly Action<T> onNext;

        public AnonymousObserver(Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            this.onNext = onNext ?? emptyOnNext;
            this.onError = onError ?? Anonymous.DefaultOnError;
            this.onCompleted = onCompleted ?? Anonymous.DefaultOnAction;
        }

        public void OnCompleted()
        {
            onCompleted();
        }

        public void OnError(Exception error)
        {
            onError(error);
        }

        public void OnNext(T value)
        {
            onNext(value);
        }
    }
}
