using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Observable.Repository.Tests.Data;
using Observable.Repository.Tests.Tools;

namespace Observable.Repository.Tests
{
    [TestFixture]
    public class RepositoryTests
    {
        private IRepositoryContainer _container;
        private const string FILTER_NAME = "Exclude";

        private Subject<List<ModelLeft>> _addSubject;
        private Subject<List<ModelLeft>> _removeSubject;
        private Subject<List<ModelLeft>> _reloadSubject;

        [SetUp]
        public void SetUp()
        {
            _container = new RepositoryContainer();

            _addSubject = new Subject<List<ModelLeft>>();
            _removeSubject = new Subject<List<ModelLeft>>();
            _reloadSubject = new Subject<List<ModelLeft>>();

            _container.AddProducer(ActionType.Add, _addSubject);
            _container.AddProducer(ActionType.Remove, _removeSubject);
            _container.AddProducer(ActionType.Reload, _reloadSubject);

            _container.Build<int, AdapterJoin, ModelLeft>(p => p.PrimaryKey, filter: p => p.Name != FILTER_NAME)
                     .DefineCtor(p => new AdapterJoin(p, null))
                     .Register();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void TestAddOrUpdate()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();

            IEnumerable<AdapterJoin> addedItems = new List<AdapterJoin>();
            IEnumerable<AdapterJoin> updatedItems = new List<AdapterJoin>();
            IEnumerable<AdapterJoin> replacedItems = new List<AdapterJoin>();
            IEnumerable<AdapterJoin> removedItems = new List<AdapterJoin>();

            repository.Subscribe(e =>
            {
                switch (e.Action)
                {
                    case ActionType.Add:
                        addedItems = e.NewItems.Select(p => p.Value);
                        break;
                    case ActionType.Update:
                        updatedItems = e.NewItems.Select(p => p.Value);
                        replacedItems = e.OldItems.Select(p => p.Value);
                        break;
                    case ActionType.Remove:
                        removedItems = e.OldItems.Select(p => p.Value);
                        break;
                    case ActionType.Reload:
                        addedItems = e.NewItems.Select(p => p.Value);
                        removedItems = e.OldItems.Select(p => p.Value);
                        break;
                }
            });

            // 1. Add
            var list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 1, Name = "Test1"},
                new ModelLeft {PrimaryKey = 2, Name = "Test2"},
                new ModelLeft {PrimaryKey = 3, Name = "Test3"},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(3, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft == list[0]));
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft == list[1]));
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft == list[2]));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(1, "Test1"), new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(3, "Test3"));

            // 2. Update
            var last = list.ToList();
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 1, Name = "Update Test1"},
                new ModelLeft {PrimaryKey = 2, Name = "Update Test2"},
                new ModelLeft {PrimaryKey = 3, Name = "Update Test3"},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(3, updatedItems.Count());
            Assert.IsTrue(updatedItems.Any(p => p.ModelLeft == list[0]));
            Assert.IsTrue(updatedItems.Any(p => p.ModelLeft == list[1]));
            Assert.IsTrue(updatedItems.Any(p => p.ModelLeft == list[2]));
            Assert.AreEqual(3, replacedItems.Count());
            Assert.IsTrue(replacedItems.Any(p => p.ModelLeft == last[0]));
            Assert.IsTrue(replacedItems.Any(p => p.ModelLeft == last[1]));
            Assert.IsTrue(replacedItems.Any(p => p.ModelLeft == last[2]));
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(1, "Update Test1"), new KeyValuePair<int, string>(2, "Update Test2"), new KeyValuePair<int, string>(3, "Update Test3"));

            // 3. Update should be removed
            last = list.ToList();
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 1, Name = FILTER_NAME},
                new ModelLeft {PrimaryKey = 2, Name = "Test2"},
                new ModelLeft {PrimaryKey = 3, Name = FILTER_NAME},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(1, updatedItems.Count());
            Assert.IsTrue(updatedItems.Any(p => p.ModelLeft == list[1]));
            Assert.AreEqual(1, replacedItems.Count());
            Assert.IsTrue(replacedItems.Any(p => p.ModelLeft == last[1]));
            Assert.AreEqual(2, removedItems.Count());
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft == last[0]));
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft == last[2]));
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"));

            // 4. Add but it's filtered
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 4, Name = FILTER_NAME},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            Assert.AreEqual(1, repository.Count);
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"));

            // 5. Add and update the added
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 4, Name = "Add"},
                new ModelLeft {PrimaryKey = 4, Name = "Update"},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft == list[1]));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Update"));

            // 6. Add and update the added but it's filtered
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 5, Name = "Add"},
                new ModelLeft {PrimaryKey = 5, Name = FILTER_NAME},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Update"));

            // 7. Update -> Filter
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 4, Name = "Update again"},
                new ModelLeft {PrimaryKey = 4, Name = FILTER_NAME},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(1, removedItems.Count());
            // See the 5.
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft.PrimaryKey == 4 && p.ModelLeft.Name == "Update"));
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"));

            // 8. Add -> update -> Filter
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 4, Name = "Add"},
                new ModelLeft {PrimaryKey = 4, Name = "Update again"},
                new ModelLeft {PrimaryKey = 4, Name = FILTER_NAME},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"));

            // 9. Add -> Filter -> Add
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 4, Name = "Add"},
                new ModelLeft {PrimaryKey = 4, Name = FILTER_NAME},
                new ModelLeft {PrimaryKey = 4, Name = "Add again"},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft == list[2]));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"));

            // 10. Add -> Filter -> Add -> Update
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 5, Name = "Add"},
                new ModelLeft {PrimaryKey = 5, Name = FILTER_NAME},
                new ModelLeft {PrimaryKey = 5, Name = "Add again"},
                new ModelLeft {PrimaryKey = 5, Name = "Update"},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft == list[3]));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"), new KeyValuePair<int, string>(5, "Update"));

            // 10. Update -> Filter
            last = list.ToList();
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 5, Name = "Update again"},
                new ModelLeft {PrimaryKey = 5, Name = FILTER_NAME},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(1, removedItems.Count());
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft == last[3]));
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"));

            // 11. Remove -> Add
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 6, Name = "Add" },
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft == list[0]));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"), new KeyValuePair<int, string>(6, "Add"));

            last = list.ToList();
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 6, Name = FILTER_NAME },
                new ModelLeft {PrimaryKey = 6, Name = "Add should be update"},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(1, updatedItems.Count());
            Assert.IsTrue(updatedItems.Any(p => p.ModelLeft == list[1]));
            Assert.AreEqual(1, replacedItems.Count());
            Assert.IsTrue(replacedItems.Any(p => p.ModelLeft == last[0]));
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"), new KeyValuePair<int, string>(6, "Add should be update"));

            // 12. Filter -> Add -> Update
            last = list.ToList();
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 6, Name = FILTER_NAME },
                new ModelLeft {PrimaryKey = 6, Name = "Add should be update"},
                new ModelLeft {PrimaryKey = 6, Name = "Update should be update"},
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(1, updatedItems.Count());
            Assert.IsTrue(updatedItems.Any(p => p.ModelLeft == list[2]));
            Assert.AreEqual(1, replacedItems.Count());
            Assert.IsTrue(replacedItems.Any(p => p.ModelLeft == last[1]));
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"), new KeyValuePair<int, string>(6, "Update should be update"));

            // 13. Filter -> Add -> Update -> Filter
            last = list.ToList();
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 6, Name = FILTER_NAME },
                new ModelLeft {PrimaryKey = 6, Name = "Add should be update"},
                new ModelLeft {PrimaryKey = 6, Name = "Update should be update"},
                new ModelLeft {PrimaryKey = 6, Name = FILTER_NAME },
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(1, removedItems.Count());
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft == last[2]));
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"));

            // 14. Filter an unexisted item
            list = new List<ModelLeft>
            {
                new ModelLeft {PrimaryKey = 7, Name = FILTER_NAME },
            };

            addedItems = new List<AdapterJoin>();
            updatedItems = new List<AdapterJoin>();
            replacedItems = new List<AdapterJoin>();
            removedItems = new List<AdapterJoin>();

            _addSubject.OnNext(list);

            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new KeyValuePair<int, string>(2, "Test2"), new KeyValuePair<int, string>(4, "Add again"));
        }

        private static void AssertContains(IRepository<int, AdapterJoin> repository, params KeyValuePair<int, string>[] idNames)
        {
            Assert.AreEqual(idNames.Length, repository.Count);
            foreach (var pair in idNames)
                AssertContains(repository, pair);
        }

        private static void AssertContains(IRepository<int, AdapterJoin> repository, KeyValuePair<int, string> left)
        {
            var adapter = repository[left.Key];
            
            Assert.AreEqual(left.Key, adapter.ModelLeft.PrimaryKey);
            Assert.AreEqual(left.Value, adapter.ModelLeft.Name);
        }

        [Test]
        [Ignore("Performances: Take too many times")]
        public void ReloadPerformancesTest()
        {
            var itemProduceCount = new[] { 1, 10, 100, 1000, 10000, 100000, 1000000};
            var length = itemProduceCount.Length;

            var repository = _container.GetRepository<int, AdapterJoin>();
            
            var producer = new Subject<List<ModelLeft>>();
            _container.AddProducer(ActionType.Reload, producer);

            // Jitter
            producer.OnNext(CreateScenario("Reload", 1).Items);
            producer.OnNext(CreateScenario("Reload", 10).Items);
            producer.OnNext(CreateScenario("Reload", 100).Items);

            var times = new TimeSpan[length];

            const int iterationCount = 100;

            for (var j = 0; j < length; j++)
            {
                var count = itemProduceCount[j];
                var produce = CreateScenario("Reload", count);

                var elapsed = TimeSpan.Zero;
                for (var i = 0; i < iterationCount; i++)
                {                   
                    var sw = Stopwatch.StartNew();
                    producer.OnNext(produce.Items);
                    elapsed += sw.Elapsed;
                    sw.Stop();

                    times[j] += elapsed;

                    Assert.AreEqual(count, repository.Count);
                }

                Console.WriteLine("Reload for {0} items, Elapsed: {1} ms", count, elapsed.TotalMilliseconds / iterationCount);

                GC.Collect();
            }
        }

        private static Produce<ModelLeft> CreateScenario(string name, int count)
        {
            var list = new List<ModelLeft>(count);

            for(var i=0; i<count;  i++)
                list.Add(new ModelLeft{ PrimaryKey = i, Idstr = ""+i, Name = "Test" + i});

            return new Produce<ModelLeft>
                {
                    Items = list,
                    Name = name,
                    OperationsCount = count,
                };
        }
            
        [Test]
        [Ignore("Performances: Take too many times")]
        public void PerformanceTest()
        {
            const int iterationCount = 10;
            var itemsProduceCount = new[] { 1, 10, 100, 1000, 10000, 100000/*, 1000000*/};

            // Jitter
            PerformancesTest("Target", 5, 5, (r, p) => _addSubject.OnNext(p));

            var results1 = new List<string>();
            var results2 = new List<string>();
            foreach (var t in itemsProduceCount)
            {
                var l = PerformancesTest("Target", iterationCount, t, (r, p) => _addSubject.OnNext(p));

                var result = new[]
                    {
                        new {Name = "Target", TotalElapsed = l[0], FullPlayElapsed = l[1]}
                    };

                var bestTotalElapsed = result.Min(p => p.TotalElapsed);
                var bestFullPlayElapsed = result.Min(p => p.FullPlayElapsed);

                var bte = result.First(p => p.TotalElapsed == bestTotalElapsed);
                results1.Add(
                    string.Format("Best total elapsed for {0} items : {1}, elpased={2}ms, Others : {3}",
                                  t,
                                  bte.Name,
                                  bte.TotalElapsed,
                                  string.Join(",", result.Where(p => p != bte).Select(p => string.Format("{0} : {1}ms", p.Name, p.TotalElapsed)))));

                var bfpe = result.First(p => p.FullPlayElapsed == bestFullPlayElapsed);
                results2.Add(
                    string.Format("Best full play elapsed for {0} items : {1}, elpased={2}ms, Others : {3}",
                                  t,
                                  bfpe.Name,
                                  bfpe.TotalElapsed,
                                  string.Join(",", result.Where(p => p != bte).Select(p => string.Format("{0} : {1}ms", p.Name, p.FullPlayElapsed)))));
            }

            foreach (var result in results1)
                Console.WriteLine(result);

            foreach (var result in results2)
                Console.WriteLine(result);
        }

        private List<double> PerformancesTest(string title, int iterationCount, int itemsProduceCount, Action<IRepository<int, AdapterJoin>, List<ModelLeft>> doWork)
        {
            var repository = _container.GetRepository<int, AdapterJoin>();

            MonitorProduce[] monitor = null;
            var sw = new Stopwatch();
            for (var i = 0; i < iterationCount; i++)
            {
                var play = ProduceModelLefts(itemsProduceCount);
                var length = play.Count;

                if (monitor == null)
                {
                    monitor = new MonitorProduce[length];
                    for (var j = 0; j < length; j++)
                        monitor[j] = new MonitorProduce { Stopwatch = new Stopwatch(), Name = play[j].Name, OperationCount = play[j].OperationsCount };
                }

                for (var j = 0; j < length; j++)
                {
                    monitor[j].Stopwatch.Start();
                    doWork(repository, play[j].Items);
                    monitor[j].Stopwatch.Stop();
                }

                var fullPlay = new List<ModelLeft>();
                foreach (var produce in play)
                    fullPlay.AddRange(produce.Items);

                sw.Start();
                doWork(repository, fullPlay);
                sw.Stop();

                _reloadSubject.OnNext(null);

                play.Clear();

                GC.Collect();
            }

            if (monitor == null) return null;

            var totalElapsed = TimeSpan.Zero;
            foreach (var t in monitor)
            {
                var elapsed = t.Stopwatch.Elapsed;
                totalElapsed += elapsed;
                Console.WriteLine("{0} {1} : {2} ms", title, t.Name, elapsed.TotalMilliseconds / (iterationCount * t.OperationCount));
            }

            Console.WriteLine("{0} Total elapsed : {1} ms", title, totalElapsed.TotalMilliseconds / iterationCount);
            Console.WriteLine("{0} Full play total elapsed : {1} ms", title, sw.Elapsed.TotalMilliseconds / iterationCount);

            return new List<double>
            {
                totalElapsed.TotalMilliseconds / iterationCount,
                sw.Elapsed.TotalMilliseconds / iterationCount,
            };
        }

        private static List<Produce<ModelLeft>> ProduceModelLefts(int count)
        {
            var result = new List<Produce<ModelLeft>>();

            var addLambda = new Func<int, ModelLeft>(i => new ModelLeft { PrimaryKey = i, Name = "Add " + i, Idstr = Guid.NewGuid().ToString() });
            var updateLambda = new Func<int, ModelLeft>(i => new ModelLeft { PrimaryKey = i, Name = "Update " + i, Idstr = Guid.NewGuid().ToString() });
            var filterLambda = new Func<int, ModelLeft>(i => new ModelLeft { PrimaryKey = i, Name = FILTER_NAME, Idstr = Guid.NewGuid().ToString() });

            result.Add(Producer.Produce("Add", count, addLambda));
            result.Add(Producer.Produce("Update", count, updateLambda));
            result.Add(Producer.Produce("Filter", count, filterLambda));
            result.Add(Producer.Produce("Add -> Filter", count, addLambda, filterLambda));
            result.Add(Producer.Produce("Add -> Filter -> Add", count, addLambda, filterLambda, addLambda));
            result.Add(Producer.Produce("Add -> Update", count, count, addLambda, updateLambda));
            result.Add(Producer.Produce("Update -> Filter", count, updateLambda, filterLambda));
            result.Add(Producer.Produce("Update -> Filter", count, updateLambda, filterLambda));
            result.Add(Producer.Produce("Add -> Filter -> Add -> Update", count, addLambda, filterLambda, addLambda, updateLambda));
            result.Add(Producer.Produce("Filter -> Add", count, filterLambda, addLambda));
            result.Add(Producer.Produce("Filter -> Add", count, filterLambda, addLambda));

            return result;
        }
    }
}
