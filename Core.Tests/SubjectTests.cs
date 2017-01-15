using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Observable.Tests
{
    [TestFixture]
    public class SubjectTests
    {
        private CountdownEvent countdownEvent;

        [SetUp]
        public void SetUp()
        {
            countdownEvent = new CountdownEvent(4);
        }

        [TearDown]
        public void TearDown()
        {
            countdownEvent.Dispose();
        }

        [Test]
        public void NominalTest()
        {
            var subject = new Subject<string>();

            var counter = 0;
            var subscribe = subject.Subscribe(
                p =>
                {
                    counter++;
                    Assert.AreEqual("Test", p);
                });

            subject.OnNext("Test");
            subject.OnNext("Test");

            Assert.AreEqual(2, counter);

            subscribe.Dispose();

            subject.OnNext("Must be unsubsribe");

            Assert.AreEqual(2, counter);

            subscribe = subject.Subscribe(
                p =>
                {
                    counter++;
                    Assert.AreEqual("Test", p);
                });

            subject.OnNext("Test");
            Assert.AreEqual(3, counter);

            subject.OnCompleted();

            subject.OnNext("Test");
            Assert.AreEqual(3, counter);

            subscribe.Dispose();
        }

        [Test]
        public void TestSubjectWithSelectOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.Select(p => new Tuple<string, int>(p, 1))
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.AreEqual("Test", p.Item1);
                                        Assert.AreEqual(1, p.Item2);
                                    });

            subject.OnNext("Test");
            subject.OnNext("Test");
            subject.OnNext("Test");

            Assert.AreEqual(3, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.AreEqual(3, publicationCount);

            var selectedSubject = subject.Select(p => new Tuple<string, int>(p, 1));

            Assert.IsInstanceOf<IObservable<Tuple<string, int>>>(selectedSubject);
            Assert.IsNotInstanceOf<IObserver<Tuple<string, int>>>(selectedSubject);
        }

        [Test]
        public void TestSubjectWithSelectToSubjectOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.SelectToSubject(p => new Tuple<string, int>(p, 1))
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.AreEqual("Test", p.Item1);
                                        Assert.AreEqual(1, p.Item2);
                                    });

            subject.OnNext("Test");
            subject.OnNext("Test");
            subject.OnNext("Test");

            Assert.AreEqual(3, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.AreEqual(3, publicationCount);

            var selectedSubject = subject.SelectToSubject(p => new Tuple<string, int>(p, 1));

            Assert.IsInstanceOf<IObservable<Tuple<string, int>>>(selectedSubject);
            Assert.IsInstanceOf<IObserver<Tuple<string, int>>>(selectedSubject);
        }

        [Test]
        public void TestSubjectWithWhereOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.Where(p => p == "Test")
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.AreEqual("Test", p);
                                    });

            subject.OnNext("Test");
            subject.OnNext("TEst 2");
            subject.OnNext("Test");

            Assert.AreEqual(2, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.AreEqual(2, publicationCount);

            var filteredSubject = subject.Where(p => p == "Test");

            Assert.IsInstanceOf<IObservable<string>>(filteredSubject);
            Assert.IsNotInstanceOf<IObserver<string>>(filteredSubject);
        }

        [Test]
        public void TestSubjectWithWhereToSubjectOperation()
        {
            var subject = new Subject<string>();

            var publicationCount = 0;
            var subscribe = subject.WhereToSubject(p => p == "Test")
                                   .Subscribe(
                                    p =>
                                    {
                                        publicationCount++;
                                        Assert.AreEqual("Test", p);
                                    });

            subject.OnNext("Test");
            subject.OnNext("TEst 2");
            subject.OnNext("Test");

            Assert.AreEqual(2, publicationCount);

            subscribe.Dispose();

            subject.OnNext("Test");

            Assert.AreEqual(2, publicationCount);

            var filteredSubject = subject.WhereToSubject(p => p == "Test");

            Assert.IsInstanceOf<IObservable<string>>(filteredSubject);
            Assert.IsInstanceOf<IObserver<string>>(filteredSubject);
        }

        [Test]
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
                    Assert.AreEqual(value, p);
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

            Assert.AreEqual(4, counter);

            dispose.Dispose();

            value = "No published";
            left.OnNext(value);
            right.OnNext(value);

            Assert.AreEqual(4, counter);
        }

        [Test]
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
                    Assert.AreEqual(value, p);
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

            Assert.AreEqual(4, counter);

            dispose.Dispose();

            value = "No published";
            left.OnNext(value);
            right.OnNext(value);

            Assert.AreEqual(4, counter);

            Assert.IsInstanceOf<IObservable<string>>(combine);
            Assert.IsInstanceOf<IObserver<string>>(combine);

            counter = 0;
            combine.Subscribe(
                p =>
                {
// ReSharper disable AccessToModifiedClosure
                    Assert.AreEqual(value, p);
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

            Assert.AreEqual(4, counter);

// ReSharper disable SuspiciousTypeConversion.Global
            ((IObserver<string>)combine).OnCompleted();
// ReSharper restore SuspiciousTypeConversion.Global

            value = "No published";
            left.OnNext(value);
            right.OnNext(value);

            Assert.AreEqual(4, counter);
        }


        const int COUNT = 10000;
        [Test]
        public void TestConcurrentAccess()
        {
            var counterRef = 0;

            var subject = new Subject<string>();

            var subscribe = subject.Subscribe(
                p =>
                {
                    Interlocked.Increment(ref counterRef);

                    Assert.AreEqual("Test", p);
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

            countdownEvent.Wait();

            Assert.AreEqual(COUNT * 4, counterRef);
            subscribe.Dispose();
        }

        private void Publisher(object state)
        {
            try
            {
                var subject = state as IObserver<string>;
                if(subject == null)
                    Assert.IsTrue(false, "Subject is null");

                for (var i = 0; i < COUNT; i++)
                    subject.OnNext("Test");
            }
            catch (Exception e)
            {
                Assert.IsFalse(true, e.ToString());
            }
            finally
            {
                if (countdownEvent != null)
                    countdownEvent.Signal();
            }
        }
        private void Receiver(object state)
        {
            try
            {
                var subject = state as IObservable<string>;

                subject.Subscribe(p => Assert.AreEqual("Test", p));

                countdownEvent.Wait();
            }
            catch (Exception e)
            {
                Assert.IsFalse(true, e.ToString());
            }
        }
        private void SubscriberUnsubscriber(object state)
        {
            try
            {
                var subject = state as IObservable<string>;

                while (countdownEvent.CurrentCount != countdownEvent.InitialCount)
                {
                    var disposable = subject.Subscribe(p => { });
                    Thread.Sleep(1);
                    disposable.Dispose();
                }
            }
            catch (Exception e)
            {
                Assert.IsFalse(true, e.ToString());
            }
        }


        [Test]
        public void TestPerformances()
        {
            var sw = Stopwatch.StartNew();

            var subject = new Subject<string>();
            Publisher(subject);

            sw.Stop();
            Assert.IsTrue(true, "" + sw.Elapsed);
        }
    }
}
