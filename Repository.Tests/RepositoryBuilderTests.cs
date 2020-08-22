using System;
using Observable.Repository.Configuration;
using Observable.Repository.Tests.Data;
using Xunit;

namespace Observable.Repository.Tests
{
    public class RepositoryBuilderTests
    {
        private IRepositoryContainer _container;

        public RepositoryBuilderTests()
        {
            _container = new RepositoryContainer();
        }

        [Fact]
        public void TestExtensionsMethods()
        {
            

        }

        [Fact]
        public void TestBuildThenCreate()
        {
            var repository1 = _container.Build<int, T1>(p => p.Id).Create();
            Test(null, repository1);

            var repository2 = _container.Build<int, T1>("R2", p => p.Id).Create();
            Test("R2", repository2);

            var filter = new Func<T1, bool>(p => !string.IsNullOrEmpty(p.Name));
            var repository3 = _container.Build("R3", p => p.Id, leftSourceName: "Source1", filter: filter).Create();
            Test("R3", repository3, "Source1", filter);

            var repository4 = _container.Build<int, Tuple<T1>, T1>("R4", p => p.Id, leftSourceName: "Source2", filter: filter).Create();
            Test("R4", repository4, "Source2", filter);
        }

        [Fact]
        public void TestBuildThenRegister()
        {
            _container.Build<int, T1>(p => p.Id).Register();
            var repository1 = _container.GetRepository<int, T1>();
            Test(null, repository1);

            _container.Build<int, T1>("R2", p => p.Id).Register();
            var repository2 = _container.GetRepository<int, T1>("R2");
            Test("R2", repository2);

            var filter = new Func<T1, bool>(p => !string.IsNullOrEmpty(p.Name));
            _container.Build("R3", p => p.Id, leftSourceName: "Source1", filter: filter).Register();
            var repository3 = _container.GetRepository<int, T1>("R3");
            Test("R3", repository3, "Source1", filter);

            _container.Build<int, Tuple<T1>, T1>("R4", p => p.Id, leftSourceName: "Source2", filter: filter).Register();
            var repository4 = _container.GetRepository<int, Tuple<T1>>("R4");
            Test("R4", repository4, "Source2", filter);
        }

        [Fact]
        public void TestBuildAndAddBehavior()
        {
            var repository0 = _container.Build<int, T1>(p => p.Id)
                .Create();
            Test(repository0, StorageBehavior.None, default(int), default(TimeSpan), null);

            var repository1 = _container.Build<int, T1>(p => p.Id)
                .AddRollingBehavior(1000)
                .Create();
            Test(repository1, StorageBehavior.Rolling, 1000, default(TimeSpan), null);

            var getTimestamp = new Func<T1, DateTime>(p => p.Timestamp);

            var repository2 = _container.Build<int, T1>(p => p.Id)
                .AddTimeIntervalBehavior(TimeSpan.FromHours(2), getTimestamp)
                .Create();
            Test(repository2, StorageBehavior.TimeInterval, default(int), TimeSpan.FromHours(2), getTimestamp);

            var repository3 = _container.Build<int, T1>(p => p.Id)
                .AddRollingAndTimeIntervalBehavior(100, TimeSpan.FromHours(2), getTimestamp)
                .Create();
            Test(repository3, StorageBehavior.RollingAndTimeInterval, 100, TimeSpan.FromHours(2), getTimestamp);
        }

        [Fact]
        public void TestBuildAndDefineCtor()
        {
            var repository1 = _container.Build<int, T1>(p => p.Id)
                .DefineCtor(p => p)
                .Create();
            Test<int, T1, T1>(repository1, typeof(T1));

            repository1 = _container.Build<int, T1>(p => p.Id)
                //.DefineCtor(p => p)
                .Create();
            Test<int, T1, T1>(repository1, typeof(T1));

            var repository2 = _container.Build<int, Tuple<T1>, T1>(p => p.Id)
                .DefineCtor(p => new Tuple<T1>(p))
                .Create();
            Test<int, Tuple<T1>, T1>(repository2, typeof(T1));
            repository2 = _container.Build<int, Tuple<T1>, T1>(p => p.Id)
                //.DefineCtor(p => new Tuple<T1>(p))
                .Create();
            Test<int, Tuple<T1>, T1>(repository2, typeof(T1));

            var repository3 = _container.Build<int, Tuple<T1>, T1>(p => p.Id)
                .DefineCtor(p => new Tuple<T1>(p))
                .AddRollingBehavior(1000)
                .Create();

            Test<int, Tuple<T1>, T1>(repository3, typeof(T1));
            Test<int, Tuple<T1>, T1>(repository3, StorageBehavior.Rolling, 1000, default(TimeSpan), null);
        }

        public static void Test<TKey, TValue, TLeft>(IRepository<TKey, TValue> repository, params Type[] ctorArguments)
        {
            Test<TKey, TValue, TLeft>(null, repository);

            Assert.NotNull(repository.Configuration.Ctor);
            Assert.Equal(ctorArguments.Length, repository.Configuration.CtorArguments.Count);
            for (var i = 0; i < ctorArguments.Length; i++ )
                Assert.Equal(ctorArguments[i], repository.Configuration.CtorArguments[i]);
        }

        private static void Test<TKey, TValue>(IRepository<TKey, TValue> repository, StorageBehavior behavior,
                                                      int rollingCount, TimeSpan timspan,
                                                      Func<TValue, DateTime> getTimestamp)
        {
            Test<TKey, TValue, TValue>(repository, behavior, rollingCount, timspan, getTimestamp);
        }

        private static void Test<TKey, TValue, TLeft>(IRepository<TKey, TValue> repository, StorageBehavior behavior, int rollingCount, TimeSpan timspan, Func<TValue, DateTime> getTimestamp)
        {
            Test<TKey, TValue, TLeft>(null, repository);

            Assert.Equal(behavior, repository.Configuration.Behavior);
            Assert.Equal(rollingCount, repository.Configuration.RollingCount);
            Assert.Equal(timspan, repository.Configuration.TimeInterval);
            Assert.Equal(getTimestamp, repository.Configuration.GetTimestamp);
        }

        private static void Test<TKey, TValue>(string name, IRepository<TKey, TValue> repository)
        {
            Test<TKey, TValue, TValue>(name, repository);
        }

        private static void Test<TKey, TValue, TLeft>(string name, IRepository<TKey, TValue> repository, string leftSourceName = null, Func<TLeft, bool> leftFilter = null)
        {
            Assert.NotNull(repository);
            name = name ?? string.Empty;
            Assert.Equal(name, repository.Name);

            var configuration = repository.Configuration;
            Assert.NotNull(configuration);
            Assert.Equal(name, configuration.Name);

            Assert.Equal(typeof(TKey), configuration.KeyType);
            Assert.Equal(typeof(TValue), configuration.ValueType);
            Assert.Equal(typeof (TLeft), configuration.LeftType);
            
            Assert.NotNull(configuration.Key);
            Assert.Equal(typeof(TKey), configuration.Key.KeyType);
            Assert.Equal(typeof(TLeft), configuration.Key.FromType);
            Assert.NotNull(configuration.Key.GetKey);
            Assert.IsAssignableFrom<Func<TLeft, TKey>>(configuration.Key.GetKey);

            Assert.NotNull(((RepositoryConfiguration<TKey, TValue, TLeft>)configuration).GetKey);
            Assert.IsAssignableFrom<Func<TLeft, TKey>>(((RepositoryConfiguration<TKey, TValue, TLeft>)configuration).GetKey);

            Assert.Equal(leftSourceName, configuration.LeftSourceName);
            Assert.Equal(leftFilter, configuration.LeftFilter);
        }
    }
}
