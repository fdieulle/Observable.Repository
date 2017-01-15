using System;
using NUnit.Framework;

namespace Observable.Tests
{
    [TestFixture]
    public class ZipSubjectTests
    {
        [Test]
        public void AddProducer()
        {
            var subject = new ZipSubject<string>();

            var notificationCount = 0;
            string lastNotification = null;
            var exceptionCount = 0;
            Exception lastException = null;
            var completedCount = 0;
            subject.Subscribe(p =>
            {
                notificationCount++;
                lastNotification = p;
            }, e =>
            {
                exceptionCount++;
                lastException = e;
            }, () =>
            {
                completedCount++;
            });

            var producer1 = new Subject<string>();
            var producer2 = new Subject<string>();

            subject.Add(producer1);
            subject.Add(producer2);

            subject.OnNext("Master");

            Assert.AreEqual(1, notificationCount);
            Assert.AreEqual("Master", lastNotification);

            producer1.OnNext("Producer1");

            Assert.AreEqual(2, notificationCount);
            Assert.AreEqual("Producer1", lastNotification);

            producer2.OnNext("Producer2");

            Assert.AreEqual(3, notificationCount);
            Assert.AreEqual("Producer2", lastNotification);

            var exceptionMaster = new Exception("Master");
            subject.OnError(exceptionMaster);

            Assert.AreEqual(1, exceptionCount);
            Assert.AreEqual(exceptionMaster, lastException);

            var exceptionProducer1 = new Exception("Producer1");
            producer1.OnError(exceptionProducer1);

            Assert.AreEqual(2, exceptionCount);
            Assert.AreEqual(exceptionProducer1, lastException);

            var exceptionProducer2 = new Exception("Producer2");
            producer2.OnError(exceptionProducer2);

            Assert.AreEqual(3, exceptionCount);
            Assert.AreEqual(exceptionProducer2, lastException);

            producer1.OnCompleted();

            producer1.OnNext("Producer1");

            Assert.AreEqual(0, completedCount);
            Assert.AreEqual(3, notificationCount);
            Assert.AreEqual("Producer2", lastNotification);

            subject.OnCompleted();

            subject.OnNext("Master");
            subject.OnNext("Producer21");

            Assert.AreEqual(1, completedCount);
            Assert.AreEqual(3, notificationCount);
            Assert.AreEqual("Producer2", lastNotification);
        }

        [Test]
        public void RemoveProducer()
        {
            var subject = new ZipSubject<string>();

            var notificationCount = 0;
            string lastNotification = null;
            subject.Subscribe(p =>
            {
                notificationCount++;
                lastNotification = p;
            });

            var producer1 = new Subject<string>();
            var producer2 = new Subject<string>();

            subject.Add(producer1);
            subject.Add(producer2);

            subject.Remove(producer1);

            producer1.OnNext("Producer1");

            Assert.AreEqual(0, notificationCount);
            Assert.AreEqual(null, lastNotification);

            producer2.OnNext("Producer2");

            Assert.AreEqual(1, notificationCount);
            Assert.AreEqual("Producer2", lastNotification);
        }
    }
}
