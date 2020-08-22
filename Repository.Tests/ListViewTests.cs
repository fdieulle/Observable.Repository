using System;
using System.Collections.Generic;
using Observable.Repository.Tests.Data;
using Xunit;

namespace Observable.Repository.Tests
{
    public class ListViewTests
    {
        private readonly Subject<ModelLeft> _addProducer;
        private readonly Subject<ModelLeft> _removeProducer;
        private readonly Subject<List<ModelLeft>> _reloadProducer;
        private readonly IRepositoryContainer _container;

        public ListViewTests()
        {
            _addProducer = new Subject<ModelLeft>();
            _removeProducer = new Subject<ModelLeft>();
            _reloadProducer = new Subject<List<ModelLeft>>();

            _container = new RepositoryContainer();

            _container.AddProducer(ActionType.Add, _addProducer);
            _container.AddProducer(ActionType.Remove, _removeProducer);
            _container.AddProducer(ActionType.Reload, _reloadProducer);

            _container.Build<int, ModelLeft>(p => p.PrimaryKey)
                .Register();
        }

        [Fact]
        public void SubscribeAndDispose()
        {
            var repository = _container.GetRepository<int, ModelLeft>();

            var list = new List<ModelLeft> { CreateLeft(1), CreateLeft(2) };

            // 1. Subscribe list on an empty repository
            var subscribe = repository.Subscribe(list);
            // The list should be cleared
            Assert.Empty(list);

            // Add 4 items in the repository
            _addProducer.OnNext(CreateLeft(1));
            _addProducer.OnNext(CreateLeft(2));
            _addProducer.OnNext(CreateLeft(3));
            _addProducer.OnNext(CreateLeft(4));

            // Assure that the list contains all elements
            AreEqual(list, CreateLeft(1), CreateLeft(2), CreateLeft(3), CreateLeft(4));

            // 2. Dispose the suscription
            subscribe.Dispose();
            // The list should be cleared
            Assert.Empty(list);

            // 3. Subscribe list on a filled repository
            subscribe = repository.Subscribe(list);
            // By default the list should be syncronized with the repository
            AreEqual(list, CreateLeft(1), CreateLeft(2), CreateLeft(3), CreateLeft(4));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.Empty(list);

            // 4. Subscribe without get the snapshot
            subscribe = repository.Subscribe(list, synchronize: false);
            Assert.Empty(list);

            // Update an item in the repository
            _addProducer.OnNext(CreateLeft(2, "Update"));
            // Only the new updated item should added in the list
            AreEqual(list, CreateLeft(2, "Update"));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.Empty(list);

            // 5. Subscribe by filtering repository items
            subscribe = repository.Subscribe(list, p => p.PrimaryKey != 3);

            AreEqual(list, CreateLeft(1), CreateLeft(2, "Update"), CreateLeft(4));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.Empty(list);

            // 6. Subscribe by Selected a part of ModelLeft
            var ids = new List<int>();
            var subscribe2 = repository.Subscribe(ids, p => p.PrimaryKey);
            AreEqual(ids, 1, 2, 3, 4);

            // Dispose the suscription
            subscribe2.Dispose();
            Assert.Empty(ids);

            // 7. Subscribe and dispatch list management
            var step = 0;
            Action<Action> dispatcher = p =>
            {
                step = 1;
                p();
                step = 2;
            };
            subscribe = repository.Subscribe(list, synchronize: true, viewDispatcher: dispatcher);

            Assert.Equal(2, step);
            AreEqual(list, CreateLeft(1), CreateLeft(2, "Update"), CreateLeft(3), CreateLeft(4));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.Empty(list);
        }

        [Fact]
        public void TestMangeItems()
        {
            var repository = _container.GetRepository<int, ModelLeft>();

            var list = new List<ModelLeft>();

            var subscribe = repository.Subscribe(list, p => p.PrimaryKey != 3);

            _addProducer.OnNext(CreateLeft(1));
            _addProducer.OnNext(CreateLeft(2));
            _addProducer.OnNext(CreateLeft(3));
            _addProducer.OnNext(CreateLeft(4));

            AreEqual(list, CreateLeft(1), CreateLeft(2), CreateLeft(4));

            _addProducer.OnNext(CreateLeft(1, "Update"));
            _addProducer.OnNext(CreateLeft(4, "Update"));

            AreEqual(list, CreateLeft(1, "Update"), CreateLeft(2), CreateLeft(4, "Update"));

            _removeProducer.OnNext(CreateLeft(1));

            AreEqual(list, CreateLeft(2), CreateLeft(4, "Update"));

            _reloadProducer.OnNext(new List<ModelLeft> { CreateLeft(3), CreateLeft(4), CreateLeft(4, "Update"), CreateLeft(5) });

            AreEqual(list, CreateLeft(4, "Update"), CreateLeft(5));

            subscribe.Dispose();
        }

        private static ModelLeft CreateLeft(int id)
        {
            return CreateLeft(id, "Name " + id);
        }

        private static ModelLeft CreateLeft(int id, string name)
        {
            return new ModelLeft { PrimaryKey = id, Idstr = "" + id, Name = name };
        }

        private static void AreEqual<T>(IList<T> list, params T[] array)
        {
            Assert.Equal(array.Length, list.Count);
            var count = array.Length;
            for (var i = 0; i < count; i++)
            {
                Assert.Equal(array[i], list[i]);
            }
        }
    }
}
