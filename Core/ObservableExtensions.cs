using System;
using System.Collections.Generic;
using Observable.Anonymous;

namespace Observable
{
    public static class ObservableExtensions
    {
        #region Subscribe

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext, Action<Exception> onError = null, Action onCompleted = null)
        {
            return observable == null
                ? AnonymousDisposable.Empty
                : observable.Subscribe(new AnonymousObserver<T>(onNext, onError, onCompleted));
        }

        #endregion // Subscribe

        #region Zip

        public static IObservable<T> Zip<T>(this IEnumerable<IObservable<T>> source)
        {
            if (source == null) return null;

            var zip = new ZipSubject<T>();
            zip.AddRange(source);
            return zip;
        }

        public static IObservable<T> Zip<T>(this IObservable<T> left, params IObservable<T>[] rights)
        {
            if (left == null) return AnonymousObservable<T>.Empty;
            if (rights.Length == 0) return left;

            var zip = new ZipSubject<T>();
            zip.Add(left);
            zip.AddRange(rights);
            return zip;
        }

        #endregion // Zip

        #region Select

        public static IObservable<TSelect> Select<T, TSelect>(this IObservable<T> observable, Func<T, TSelect> selector)
        {
            if (observable == null || selector == null) return AnonymousObservable<TSelect>.Empty;

            return new AnonymousObservable<TSelect>(observer =>
                observable.Subscribe(o =>
                {
                    TSelect value;
                    try
                    {
                        value = selector(o);
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                        return;
                    }
                    observer.OnNext(value);
                }));
        }

        public static IObservable<TResult> SelectToSubject<T, TResult>(this IObservable<T> observable, Func<T, TResult> selector)
        {
            if (observable == null || selector == null) return new AnonymousObservable<TResult>(null);


            var subject = new Subject<TResult>();
            observable.Subscribe(value =>
            {
                TResult result;
                try
                {
                    result = selector(value);
                }
                catch (Exception e)
                {
                    subject.OnError(e);
                    return;
                }
                subject.OnNext(result);
            });
            return subject;
        }

        #endregion // Select

        #region Where

        public static IObservable<T> Where<T>(this IObservable<T> observable, Func<T, bool> predicate)
        {
            if (observable == null || predicate == null) return new AnonymousObservable<T>(null);

            return new AnonymousObservable<T>(observer =>
                observable.Subscribe(o =>
                {
                    bool onNext;
                    try
                    {
                        onNext = predicate(o);
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                        return;
                    }
                    if (onNext)
                        observer.OnNext(o);
                }));
        }

        public static IObservable<T> WhereToSubject<T>(this IObservable<T> observable, Func<T, bool> predicate)
        {
            if (observable == null || predicate == null)
                return new AnonymousObservable<T>(null);

            var subject = new Subject<T>();
            observable.Subscribe(value =>
            {
                bool onNext;
                try
                {
                    onNext = predicate(value);
                }
                catch (Exception e)
                {
                    subject.OnError(e);
                    return;
                }
                if (onNext)
                    subject.OnNext(value);
            });

            return subject;
        }

        #endregion // Where

        #region OfType

        public static IObservable<TO> OfType<TFrom, TO>(this IObservable<TFrom> observable)
        {
            if (observable == null) return new AnonymousObservable<TO>(null);

            return new AnonymousObservable<TO>(observer =>
                observable.Subscribe(o =>
                {
                    TO casted;
                    try
                    {
                        casted = (TO)(object)o;
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                        return;
                    }

                    observer.OnNext(casted);
                }));
        }

        #endregion // OfType
    }
}
