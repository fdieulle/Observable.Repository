using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace Observable.Tests
{
    public class SubjectTests : IDisposable
    {
        private readonly CountdownEvent _countdownEvent;

        public SubjectTests()
        {
            _countdownEvent = new CountdownEvent(4);
        }

        public void Dispose()
        {
            _countdownEvent.Dispose();
        }

        [Fact]
        public void NominalTest()
        {
            var subject = new Subject<string>();

            var counter = 0;
            var subscribe = subject.Subscribe(
                p =>
                {
                    counter++;
                    Assert.Equal("Test", p);
                });

            subject.OnNext("Test");
            subject.OnNext("Test");

            Assert.Equal(2, counter);

            subscribe.Dispose();

            subject.OnNext("Must be unsubsribe");

            Assert.Equal(2, counter);

            subscribe = subject.Subscribe(
                p =>
                {
                    counter++;
                    Assert.Equal("Test", p);
                });

            subject.OnNext("Test");
            Assert.Equal(3, counter);

            subject.OnCompleted();

            subject.OnNext("Test");
            Assert.Equal(3, counter);

            subscribe.Dispose();
        }

        [Fact]
        public void TestSubjectWithSelectOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.Select(p => new Tuple<string, int>(p, 1))
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.Equal("Test", p.Item1);
                                        Assert.Equal(1, p.Item2);
                                    });

            subject.OnNext("Test");
            subject.OnNext("Test");
            subject.OnNext("Test");

            Assert.Equal(3, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.Equal(3, publicationCount);

            var selectedSubject = subject.Select(p => new Tuple<string, int>(p, 1));

            Assert.IsAssignableFrom<IObservable<Tuple<string, int>>>(selectedSubject);
            Assert.IsNotType<IObserver<Tuple<string, int>>>(selectedSubject);
        }

        [Fact]
        public void TestSubjectWithSelectToSubjectOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.SelectToSubject(p => new Tuple<string, int>(p, 1))
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.Equal("Test", p.Item1);
                                        Assert.Equal(1, p.Item2);
                                    });

            subject.OnNext("Test");
            subject.OnNext("Test");
            subject.OnNext("Test");

            Assert.Equal(3, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.Equal(3, publicationCount);

            var selectedSubject = subject.SelectToSubject(p => new Tuple<string, int>(p, 1));

            Assert.IsAssignableFrom<IObservable<Tuple<string, int>>>(selectedSubject);
            Assert.IsAssignableFrom<IObserver<Tuple<string, int>>>(selectedSubject);
        }

        [Fact]
        public void TestSubjectWithWhereOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.Where(p => p == "Test")
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.Equal("Test", p);
                                    });

            subject.OnNext("Test");
            subject.OnNext("TEst 2");
            subject.OnNext("Test");

            Assert.Equal(2, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.Equal(2, publicationCount);

            var filteredSubject = subject.Where(p => p == "Test");

            Assert.IsAssignableFrom<IObservable<string>>(filteredSubject);
            Assert.IsNotType<IObserver<string>>(filteredSubject);
        }

        [Fact]
        public void TestSubjectWithWhereToSubjectOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.WhereToSubject(p => p == "Test")
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.Equal("Test", p);
                                    });

            subject.OnNext("Test");
            subject.OnNext("TEst 2");
            subject.OnNext("Test");

            Assert.Equal(2, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.Equal(2, publicationCount);

            var filteredSubject = subject.WhereToSubject(p => p == "Test");

            Assert.IsAssignableFrom<IObservable<string>>(filteredSubject);
            Assert.IsAssignableFrom<IObserver<string>>(filteredSubject);
        }

        [Fact]
        public void TestSubjectWithCombineOperation()
        {
            var left = new Subject<string>();
            var right = new Subject<string>();

            var combine = left.Zip(right);

            var counter = 0;
            var value = string.Empty;
            var dispose = combine.Subscribe(
                p =>
                {
// ReSharper disable AccessToModifiedClosure
                    Assert.Equal(value, p);
// ReSharper restore AccessToModifiedClosure
                    counter++;
                });

            value = "test from left";
            left.OnNext(value);

            value = "test from right";
            right.OnNext(value);

            value = "test from left 2";
            left.OnNext(value);
            value = "test from left 3";
            left.OnNext(value);

            Assert.Equal(4, counter);

            dispose.Dispose();

            value = "No published";
            left.OnNext(value);
            right.OnNext(value);

            Assert.Equal(4, counter);
        }

        [Fact]
        public void TestSubjectWithCombineToSubjectOperation()
        {
            var left = new Subject<string>();
            var right = new Subject<string>();

            var combine = left.Zip(right);

            var counter = 0;
            var value = string.Empty;
            var dispose = combine.Subscribe(
                p =>
                {
// ReSharper disable AccessToModifiedClosure
                    Assert.Equal(value, p);
                    counter++;
// ReSharper restore AccessToModifiedClosure
                });

            value = "test from left";
            left.OnNext(value);

            value = "test from right";
            right.OnNext(value);

            value = "test from left 2";
            left.OnNext(value);
            value = "test from left 3";
            left.OnNext(value);

            Assert.Equal(4, counter);

            dispose.Dispose();

            value = "No published";
            left.OnNext(value);
            right.OnNext(value);

            Assert.Equal(4, counter);

            Assert.IsAssignableFrom<IObservable<string>>(combine);
            Assert.IsAssignableFrom<IObserver<string>>(combine);

            counter = 0;
            combine.Subscribe(
                p =>
                {
// ReSharper disable AccessToModifiedClosure
                    Assert.Equal(value, p);
// ReSharper restore AccessToModifiedClosure
                    counter++;
                });

            value = "test from left";
            left.OnNext(value);

            value = "test from right";
            right.OnNext(value);

            value = "test from left 2";
            left.OnNext(value);
            value = "test from left 3";
            left.OnNext(value);

            Assert.Equal(4, counter);

// ReSharper disable SuspiciousTypeConversion.Global
            ((IObserver<string>)combine).OnCompleted();
// ReSharper restore SuspiciousTypeConversion.Global

            value = "No published";
            left.OnNext(value);
            right.OnNext(value);

            Assert.Equal(4, counter);
        }


        const int COUNT = 10000;
        [Fact]
        public void TestConcurrentAccess()
        {
            var counterRef = 0;

            var subject = new Subject<string>();

            var subscribe = subject.Subscribe(
                p =>
                {
                    Interlocked.Increment(ref counterRef);

                    Assert.Equal("Test", p);
                });

            ThreadPool.QueueUserWorkItem(Receiver, subject);
            ThreadPool.QueueUserWorkItem(Publisher, subject);
            ThreadPool.QueueUserWorkItem(Publisher, subject);
            ThreadPool.QueueUserWorkItem(SubscriberUnsubscriber, subject);
            ThreadPool.QueueUserWorkItem(SubscriberUnsubscriber, subject);
            ThreadPool.QueueUserWorkItem(SubscriberUnsubscriber, subject);
            ThreadPool.QueueUserWorkItem(Receiver, subject);
            ThreadPool.QueueUserWorkItem(Publisher, subject);
            ThreadPool.QueueUserWorkItem(Publisher, subject);

            _countdownEvent.Wait();

            Assert.Equal(COUNT * 4, counterRef);
            subscribe.Dispose();
        }

        private void Publisher(object state)
        {
            try
            {
                var subject = state as IObserver<string>;
                if(subject == null)
                    Assert.True(false, "Subject is null");

                for (var i = 0; i < COUNT; i++)
                    subject.OnNext("Test");
            }
            catch (Exception e)
            {
                Assert.False(true, e.ToString());
            }
            finally
            {
                if (_countdownEvent != null)
                    _countdownEvent.Signal();
            }
        }
        private void Receiver(object state)
        {
            try
            {
                var subject = state as IObservable<string>;

                subject.Subscribe(p => Assert.Equal("Test", p));

                _countdownEvent.Wait();
            }
            catch (Exception e)
            {
                Assert.False(true, e.ToString());
            }
        }
        private void SubscriberUnsubscriber(object state)
        {
            try
            {
                var subject = state as IObservable<string>;

                while (_countdownEvent.CurrentCount != _countdownEvent.InitialCount)
                {
                    var disposable = subject.Subscribe(p => { });
                    Thread.Sleep(1);
                    disposable.Dispose();
                }
            }
            catch (Exception e)
            {
                Assert.False(true, e.ToString());
            }
        }


        [Fact]
        public void TestPerformances()
        {
            var sw = Stopwatch.StartNew();

            var subject = new Subject<string>();
            Publisher(subject);

            sw.Stop();
            Assert.True(true, "" + sw.Elapsed);
        }
    }
}
