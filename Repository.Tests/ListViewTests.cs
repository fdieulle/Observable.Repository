﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Observable.Repository.Tests.Data;

namespace Observable.Repository.Tests
{
    [TestFixture]
    public class ListViewTests
    {
        private Subject<ModelLeft> addProducer;
        private Subject<ModelLeft> removeProducer;
        private Subject<List<ModelLeft>> reloadProducer;
        private IRepositoryContainer container;

        [SetUp]
        public void SetUp()
        {
            addProducer = new Subject<ModelLeft>();
            removeProducer = new Subject<ModelLeft>();
            reloadProducer = new Subject<List<ModelLeft>>();

            container = new RepositoryContainer();

            container.AddProducer(ActionType.Add, addProducer);
            container.AddProducer(ActionType.Remove, removeProducer);
            container.AddProducer(ActionType.Reload, reloadProducer);

            container.Build<int, ModelLeft>(p => p.PrimaryKey)
                .Register();
        }

        [Test]
        public void SubscribeAndDispose()
        {
            var repository = container.GetRepository<int, ModelLeft>();

            var list = new List<ModelLeft> { CreateLeft(1), CreateLeft(2) };

            // 1. Subscribe list on an empty repository
            var subscribe = repository.Subscribe(list);
            // The list should be cleared
            Assert.AreEqual(0, list.Count);

            // Add 4 items in the repository
            addProducer.OnNext(CreateLeft(1));
            addProducer.OnNext(CreateLeft(2));
            addProducer.OnNext(CreateLeft(3));
            addProducer.OnNext(CreateLeft(4));

            // Assure that the list contains all elements
            AreEqual(list, CreateLeft(1), CreateLeft(2), CreateLeft(3), CreateLeft(4));

            // 2. Dispose the suscription
            subscribe.Dispose();
            // The list should be cleared
            Assert.AreEqual(0, list.Count);

            // 3. Subscribe list on a filled repository
            subscribe = repository.Subscribe(list);
            // By default the list should be syncronized with the repository
            AreEqual(list, CreateLeft(1), CreateLeft(2), CreateLeft(3), CreateLeft(4));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.AreEqual(0, list.Count);

            // 4. Subscribe without get the snapshot
            subscribe = repository.Subscribe(list, synchronize: false);
            Assert.AreEqual(0, list.Count);

            // Update an item in the repository
            addProducer.OnNext(CreateLeft(2, "Update"));
            // Only the new updated item should added in the list
            AreEqual(list, CreateLeft(2, "Update"));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.AreEqual(0, list.Count);

            // 5. Subscribe by filtering repository items
            subscribe = repository.Subscribe(list, p => p.PrimaryKey != 3);

            AreEqual(list, CreateLeft(1), CreateLeft(2, "Update"), CreateLeft(4));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.AreEqual(0, list.Count);

            // 6. Subscribe by Selected a part of ModelLeft
            var ids = new List<int>();
            var subscribe2 = repository.Subscribe(ids, p => p.PrimaryKey);
            AreEqual(ids, 1, 2, 3, 4);

            // Dispose the suscription
            subscribe2.Dispose();
            Assert.AreEqual(0, ids.Count);

            // 7. Subscribe and dispatch list management
            var step = 0;
            Action<Action> dispatcher = p =>
            {
                step = 1;
                p();
                step = 2;
            };
            subscribe = repository.Subscribe(list, synchronize: true, viewDispatcher: dispatcher);

            Assert.AreEqual(2, step);
            AreEqual(list, CreateLeft(1), CreateLeft(2, "Update"), CreateLeft(3), CreateLeft(4));

            // Dispose the suscription
            subscribe.Dispose();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void TestMangeItems()
        {
            var repository = container.GetRepository<int, ModelLeft>();

            var list = new List<ModelLeft>();

            var subscribe = repository.Subscribe(list, p => p.PrimaryKey != 3);

            addProducer.OnNext(CreateLeft(1));
            addProducer.OnNext(CreateLeft(2));
            addProducer.OnNext(CreateLeft(3));
            addProducer.OnNext(CreateLeft(4));

            AreEqual(list, CreateLeft(1), CreateLeft(2), CreateLeft(4));

            addProducer.OnNext(CreateLeft(1, "Update"));
            addProducer.OnNext(CreateLeft(4, "Update"));

            AreEqual(list, CreateLeft(1, "Update"), CreateLeft(2), CreateLeft(4, "Update"));

            removeProducer.OnNext(CreateLeft(1));

            AreEqual(list, CreateLeft(2), CreateLeft(4, "Update"));

            reloadProducer.OnNext(new List<ModelLeft> { CreateLeft(3), CreateLeft(4), CreateLeft(4, "Update"), CreateLeft(5) });

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
            Assert.AreEqual(array.Length, list.Count);
            var count = array.Length;
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(array[i], list[i]);
            }
        }
    }
}
