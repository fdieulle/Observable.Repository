using System;
using System.Collections.Generic;
using System.Linq;
using Observable.Repository.Core;
using Observable.Repository.Tests.Data;
using Xunit;

namespace Observable.Repository.Tests
{
    public class RepositoryBehaviorsTests : IDisposable
    {
        private readonly IRepositoryContainer _container;
        private readonly Subject<List<ModelLeft>> _addSubject;
        private readonly Subject<List<ModelLeft>> _removeSubject;
        private readonly Subject<List<ModelLeft>> _reloadSubject;

        public RepositoryBehaviorsTests()
        {
            _container = new RepositoryContainer();

            _addSubject = new Subject<List<ModelLeft>>();
            _removeSubject = new Subject<List<ModelLeft>>();
            _reloadSubject = new Subject<List<ModelLeft>>();

            _container.AddProducer(ActionType.Add, _addSubject);
            _container.AddProducer(ActionType.Remove, _removeSubject);
            _container.AddProducer(ActionType.Reload, _reloadSubject);
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        [Fact]
        public void TestRollingBehavior()
        {
            var repository = _container.Build<int, ModelLeft>(p => p.PrimaryKey)
                .AddRollingBehavior(5)
                .Create();

            IEnumerable<KeyValue<int, ModelLeft>> addedItems = new List<KeyValue<int, ModelLeft>>();
            IEnumerable<KeyValue<int, ModelLeft>> updatedItems = new List<KeyValue<int, ModelLeft>>();
            IEnumerable<KeyValue<int, ModelLeft>> replacedItems = new List<KeyValue<int, ModelLeft>>();
            IEnumerable<KeyValue<int, ModelLeft>> removedItems = new List<KeyValue<int, ModelLeft>>();

            repository.Subscribe(e =>
            {
                switch (e.Action)
                {
                    case ActionType.Add:
                        addedItems = e.NewItems;
                        break;
                    case ActionType.Update:
                        updatedItems = e.NewItems;
                        replacedItems = e.OldItems;
                        break;
                    case ActionType.Remove:
                        removedItems = e.OldItems;
                        break;
                    case ActionType.Reload:
                        addedItems = e.NewItems;
                        removedItems = e.OldItems;
                        break;
                }
            });

            var list = CreateData(1, 3);

            _addSubject.OnNext(list);

            AssertContains(addedItems, CreateDataTest(1, 3));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems);
            AssertContains(repository, CreateDataTest(1, 3));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _addSubject.OnNext(CreateData(4, 7));

            AssertContains(addedItems, CreateDataTest(4, 7));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, CreateDataTest(1, 2));
            AssertContains(repository, CreateDataTest(3, 7));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _removeSubject.OnNext(CreateData(5, 6));

            AssertContains(addedItems);
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, CreateDataTest(5, 6));
            AssertContains(repository, T(3), T(4), T(7));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _addSubject.OnNext(CreateData(8, 14));

            AssertContains(addedItems, CreateDataTest(10, 14));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, T(3), T(4), T(7));
            AssertContains(repository, CreateDataTest(10, 14));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _reloadSubject.OnNext(CreateData(1, 6));

            AssertContains(addedItems, CreateDataTest(2, 6));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, CreateDataTest(10, 14));
            AssertContains(repository, CreateDataTest(2, 6));
        }

        [Fact]
        public void TestTimeIntervalBehavior()
        {
            var repository = _container.Build<int, ModelLeft>(p => p.PrimaryKey)
                .AddTimeIntervalBehavior(TimeSpan.FromSeconds(5), p => p.Timestamp)
                .Create();

            IEnumerable<KeyValue<int, ModelLeft>> addedItems = new List<KeyValue<int, ModelLeft>>();
            IEnumerable<KeyValue<int, ModelLeft>> updatedItems = new List<KeyValue<int, ModelLeft>>();
            IEnumerable<KeyValue<int, ModelLeft>> replacedItems = new List<KeyValue<int, ModelLeft>>();
            IEnumerable<KeyValue<int, ModelLeft>> removedItems = new List<KeyValue<int, ModelLeft>>();

            repository.Subscribe(e =>
            {
                switch (e.Action)
                {
                    case ActionType.Add:
                        addedItems = e.NewItems;
                        break;
                    case ActionType.Update:
                        updatedItems = e.NewItems;
                        replacedItems = e.OldItems;
                        break;
                    case ActionType.Remove:
                        removedItems = e.OldItems;
                        break;
                    case ActionType.Reload:
                        addedItems = e.NewItems;
                        removedItems = e.OldItems;
                        break;
                }
            });

            var list = CreateData(1, 3);

            _addSubject.OnNext(list);

            AssertContains(addedItems, CreateDataTest(1, 3));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems);
            AssertContains(repository, CreateDataTest(1, 3));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _addSubject.OnNext(CreateData(4, 7));

            AssertContains(addedItems, CreateDataTest(4, 7));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, CreateDataTest(1, 2));
            AssertContains(repository, CreateDataTest(3, 7));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _removeSubject.OnNext(CreateData(5, 6));

            AssertContains(addedItems);
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, CreateDataTest(5, 6));
            AssertContains(repository, T(3), T(4), T(7));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _addSubject.OnNext(CreateData(8, 14));

            AssertContains(addedItems, CreateDataTest(10, 14));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, T(3), T(4), T(7));
            AssertContains(repository, CreateDataTest(10, 14));

            addedItems = new List<KeyValue<int, ModelLeft>>();
            updatedItems = new List<KeyValue<int, ModelLeft>>();
            replacedItems = new List<KeyValue<int, ModelLeft>>();
            removedItems = new List<KeyValue<int, ModelLeft>>();

            _reloadSubject.OnNext(CreateData(1, 6));

            AssertContains(addedItems, CreateDataTest(2, 6));
            AssertContains(updatedItems);
            AssertContains(replacedItems);
            AssertContains(removedItems, CreateDataTest(10, 14));
            AssertContains(repository, CreateDataTest(2, 6));
        }

        private static void AssertContains(IRepository<int, ModelLeft> repository, params Tuple<int, string, DateTime>[] items)
        {
            Assert.Equal(items.Length, repository.Count);

            var i = 0;
            foreach (var pair in repository)
                AssertItem(pair, items[i++]);
        }

        private static void AssertContains(IEnumerable<KeyValue<int, ModelLeft>> source, params Tuple<int, string, DateTime>[] items)
        {
            var list = source.ToList();
            Assert.Equal(items.Length, list.Count);

            for (var i = 0; i < items.Length; i++)
                AssertItem(list[i], items[i]);
        }

        private static void AssertItem(KeyValue<int, ModelLeft> pair, Tuple<int, string, DateTime> tuple)
        {
            Assert.Equal(tuple.Item1, pair.Key);
            Assert.Equal(tuple.Item1, pair.Value.PrimaryKey);
            Assert.Equal(tuple.Item2, pair.Value.Name);
            Assert.Equal(tuple.Item3, pair.Value.Timestamp);
        }

        private static readonly DateTime startDate = DateTime.Now;

        private static List<ModelLeft> CreateData(int start, int end)
        {
            var list = new List<ModelLeft>();
            for(var i=start; i<=end; i++)
                list.Add(D(i));
            return list;
        }

        private static ModelLeft D(int i)
        {
            return new ModelLeft {PrimaryKey = i, Name = "Test" + i, Timestamp = startDate.AddSeconds(i)};
        }

        private static Tuple<int, string, DateTime>[] CreateDataTest(int start, int end)
        {
            var array = new Tuple<int, string, DateTime>[(end - start) + 1];
            for (var i = start; i <= end; i++)
                array[i - start] = T(i);
            return array;
        }

        private static Tuple<int, string, DateTime> T(int i)
        {
            return new Tuple<int, string, DateTime>(i, "Test" + i, startDate.AddSeconds(i));
        }
    }
}
