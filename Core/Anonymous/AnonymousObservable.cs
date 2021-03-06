﻿using System;

namespace Observable.Anonymous
{
    public class AnonymousObservable<T> : IObservable<T>
    {
        public static readonly AnonymousObservable<T> Empty =  new AnonymousObservable<T>(null);

        private readonly Func<IObserver<T>, IDisposable> _subscription;

        public AnonymousObservable(Func<IObserver<T>, IDisposable> subscription)
        {
            _subscription = subscription;
        }

        public IDisposable Subscribe(IObserver<T> observer) 
            => _subscription == null 
                ? AnonymousDisposable.Empty 
                : _subscription(observer);
    }
}
