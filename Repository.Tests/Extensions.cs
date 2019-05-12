using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Observable.Repository.Core;

namespace Observable.Repository.Tests
{
    public static class Extensions
    {
        public static RepositoryChecker<TKey, TValue> GetChecker<TKey, TValue>(this IRepository<TKey, TValue> repository, Func<TValue, TKey> getKey = null)
        {
            return new RepositoryChecker<TKey, TValue>(repository, getKey);
        }

        public static void OnNext<T>(this Subject<List<T>> subject, params T[] array)
        {
            subject.OnNext(array.ToList());
        }

        public static Subject<List<T>> AddProducer<T>(this IRepositoryContainer container, ActionType action)
        {
            var subject = new Subject<List<T>>();
            container.AddProducer(action, subject);
            return subject;
        }

        public class RepositoryChecker<TKey, TValue> : IDisposable
        {
            private readonly IRepository<TKey, TValue> _repository;
            private readonly Func<TValue, TKey> _getKey;
            private readonly Queue<RepositoryNotification<KeyValue<TKey, TValue>>> _notifications = new Queue<RepositoryNotification<KeyValue<TKey, TValue>>>();
            private readonly IDisposable _suscription;

            public RepositoryChecker(IRepository<TKey, TValue> repository, Func<TValue, TKey> getKey)
            {
                this._repository = repository;
                this._getKey = getKey;
                _suscription = repository.Subscribe(OnRepositoryNotified);
            }

            private void OnRepositoryNotified(RepositoryNotification<KeyValue<TKey, TValue>> e)
            {
                _notifications.Enqueue(e);
            }

            public void CheckAdded(TValue[] oldValues, TValue[] newValues)
            {
                CheckNotification(ActionType.Add, oldValues, newValues);
            }

            public void CheckUpdated(TValue[] oldValues, TValue[] newValues)
            {
                CheckNotification(ActionType.Update, oldValues, newValues);
            }

            public void CheckRemoved(TValue[] oldValues, TValue[] newValues)
            {
                CheckNotification(ActionType.Remove, oldValues, newValues);
            }

            public void CheckReloaded(TValue[] oldValues, TValue[] newValues)
            {
                CheckNotification(ActionType.Reload, oldValues, newValues);
            }

            public void CheckNotification(ActionType action, TValue[] oldValues, TValue[] newValues)
            {
                Assert.Greater(_notifications.Count, 0, "No notifications raised !");
                var e = _notifications.Dequeue();
                Assert.AreEqual(action, e.Action);
                Check(oldValues ?? new TValue[0], e.OldItems);
                Check(newValues ?? new TValue[0], e.NewItems);
            }

            private void Check(TValue[] x, IEnumerable<KeyValue<TKey, TValue>> y)
            {
                var ay = y.ToArray();
                Assert.AreEqual(x.Length, ay.Length);
                for (var i = 0; i < x.Length; i++)
                {
                    Assert.AreEqual(x[i], ay[i].Value);
                    if(_getKey != null)
                        Assert.AreEqual(_getKey(x[i]), ay[i].Key);
                }
            }

            public void Check(params TValue[] values)
            {
                Check(values, _repository);
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                _suscription.Dispose();
            }

            #endregion

            public void CheckNoMoreNotifications()
            {
                Assert.AreEqual(0, _notifications.Count);
            }

            public void ClearNotifications()
            {
                _notifications.Clear();
            }
        }
    }
}
