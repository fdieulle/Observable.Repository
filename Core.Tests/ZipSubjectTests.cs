using System;
using Xunit;

namespace Observable.Tests
{
    public class ZipSubjectTests
    {
        [Fact]
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

            Assert.Equal(1, notificationCount);
            Assert.Equal("Master", lastNotification);

            producer1.OnNext("Producer1");

            Assert.Equal(2, notificationCount);
            Assert.Equal("Producer1", lastNotification);

            producer2.OnNext("Producer2");

            Assert.Equal(3, notificationCount);
            Assert.Equal("Producer2", lastNotification);

            var exceptionMaster = new Exception("Master");
            subject.OnError(exceptionMaster);

            Assert.Equal(1, exceptionCount);
            Assert.Equal(exceptionMaster, lastException);

            var exceptionProducer1 = new Exception("Producer1");
            producer1.OnError(exceptionProducer1);

            Assert.Equal(2, exceptionCount);
            Assert.Equal(exceptionProducer1, lastException);

            var exceptionProducer2 = new Exception("Producer2");
            producer2.OnError(exceptionProducer2);

            Assert.Equal(3, exceptionCount);
            Assert.Equal(exceptionProducer2, lastException);

            producer1.OnCompleted();

            producer1.OnNext("Producer1");

            Assert.Equal(0, completedCount);
            Assert.Equal(3, notificationCount);
            Assert.Equal("Producer2", lastNotification);

            subject.OnCompleted();

            subject.OnNext("Master");
            subject.OnNext("Producer21");

            Assert.Equal(1, completedCount);
            Assert.Equal(3, notificationCount);
            Assert.Equal("Producer2", lastNotification);
        }

        [Fact]
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

            Assert.Equal(0, notificationCount);
            Assert.Equal(null, lastNotification);

            producer2.OnNext("Producer2");

            Assert.Equal(1, notificationCount);
            Assert.Equal("Producer2", lastNotification);
        }
    }
}
