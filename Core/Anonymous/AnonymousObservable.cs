using System;

namespace Observable.Anonymous
{
    public class AnonymousObservable<T> : IObservable<T>
    {
        public static readonly AnonymousObservable<T> Empty =  new AnonymousObservable<T>(null);

        private readonly Func<IObserver<T>, IDisposable> subscription;

        public AnonymousObservable(Func<IObserver<T>, IDisposable> subscription)
        {
            this.subscription = subscription;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return subscription == null 
                ? AnonymousDisposable.Empty 
                : subscription(observer);
        }
    }
}
