using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using Observable.Repository.Tests.Data;

namespace Observable.Repository.Tests
{
    [TestFixture]
    public class RepositoryJoinManyTests
    {
        private IRepositoryContainer _container;
        private Subject<ModelLeft> _addLeft;
        private Subject<ModelLeft> _removeLeft;
        private Subject<List<ModelLeft>> _reloadLeft;
        private Subject<ModelRight> _addRight;
        private Subject<ModelRight> _removeRight;
        private Subject<List<ModelRight>> _reloadRight;
        private const string FILTERED_NAME = "FILTER";

        [SetUp]
        public void SetUp()
        {
            _addLeft = new Subject<ModelLeft>();
            _removeLeft = new Subject<ModelLeft>();
            _reloadLeft = new Subject<List<ModelLeft>>();
            _addRight = new Subject<ModelRight>();
            _removeRight = new Subject<ModelRight>();
            _reloadRight = new Subject<List<ModelRight>>();

            _container = new RepositoryContainer();

            _container.AddProducer(ActionType.Add, _addLeft);
            _container.AddProducer(ActionType.Remove, _removeLeft);
            _container.AddProducer(ActionType.Reload, _reloadLeft);
            _container.AddProducer(ActionType.Add, _addRight);
            _container.AddProducer(ActionType.Remove, _removeRight);
            _container.AddProducer(ActionType.Reload, _reloadRight);

            _container.Build<int, AdapterJoinMany, ModelLeft>(p => p.PrimaryKey)
                .JoinMany<ModelRight>(null, p => p.Name != FILTERED_NAME)
                    .DefineList(p => p.ModelRights)
                    .RightPrimaryKey(p => p.PrimaryKey)
                    .RightLinkKey(p => p.ForeignKey)
                    .LeftLinkKey(p => p.PrimaryKey)
                .Register();
        }

        [Test]
        public void TestAddRightBeforeLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinMany>();

            IEnumerable<AdapterJoinMany> addedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> updatedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> replacedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> removedItems = new List<AdapterJoinMany>();
            ActionType? action = null;

            repository.Subscribe(e =>
            {
                action = e.Action;
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

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 4" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 5, ForeignKey = 3, Name = FILTERED_NAME });

            Assert.IsNull(action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository);

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });

            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft.PrimaryKey == 1 && p.ModelRights.Count == 1 && p.ModelRights[0].PrimaryKey == 1));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"), new[] { new Tuple<int, int, string>(1, 1, "Right 1") }));

            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });

            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft.PrimaryKey == 2 && p.ModelRights.Count == 3 && p.ModelRights[0].Name == "Right 2" && p.ModelRights[1].Name == "Right 3" && p.ModelRights[2].Name == "Right 4"));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(3, 2, "Right 3"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }));

            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 3, Name = "Left 3" });

            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft.PrimaryKey == 3 && p.ModelRights.Count == 0));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                        new[]
                        {
                            new Tuple<int, int, string>(2, 2, "Right 2"), 
                            new Tuple<int, int, string>(3, 2, "Right 3"), 
                            new Tuple<int, int, string>(4, 2, "Right 4")
                        }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"), new Tuple<int, int, string>[0]));
        }

        [Test]
        public void TestAddRightAfterLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinMany>();

            IEnumerable<AdapterJoinMany> addedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> updatedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> replacedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> removedItems = new List<AdapterJoinMany>();
            ActionType? action = null;

            repository.Subscribe(e =>
            {
                action = e.Action;
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

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });

            Assert.AreEqual(ActionType.Add, action);
            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft.PrimaryKey == 1 && p.ModelRights.Count == 0));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository, new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"), new Tuple<int, int, string>[0]));

            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });

            Assert.AreEqual(ActionType.Add, action);
            Assert.AreEqual(1, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft.PrimaryKey == 2 && p.ModelRights.Count == 0));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"), new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"), new Tuple<int, int, string>[0]));

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = FILTERED_NAME });
            Assert.AreEqual(null, action);
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"), new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"), new Tuple<int, int, string>[0]));

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new Tuple<int, int, string>[0]));

            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[] { new Tuple<int, int, string>(2, 2, "Right 2") }));

            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"),
                        new Tuple<int, int, string>(3, 2, "Right 3")
                    }));

            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = FILTERED_NAME });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"),
                        new Tuple<int, int, string>(3, 2, "Right 3")
                    }));
        }

        [Test]
        public void RightModelChangeHisForeignKey()
        {
            var repository = _container.GetRepository<int, AdapterJoinMany>();

            IEnumerable<AdapterJoinMany> addedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> updatedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> replacedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> removedItems = new List<AdapterJoinMany>();
            ActionType? action = null;

            repository.Subscribe(e =>
            {
                action = e.Action;
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

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 4" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 5, ForeignKey = 3, Name = FILTERED_NAME });

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 3, Name = "Left 3" });

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                        new[]
                        {
                            new Tuple<int, int, string>(2, 2, "Right 2"), 
                            new Tuple<int, int, string>(3, 2, "Right 3"), 
                            new Tuple<int, int, string>(4, 2, "Right 4")
                        }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"), new Tuple<int, int, string>[0]));


            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 3, Name = "Right 3" });

            Assert.IsNull(action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());

            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                        new[]
                        {
                            new Tuple<int, int, string>(2, 2, "Right 2"), 
                            new Tuple<int, int, string>(4, 2, "Right 4")
                        }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                        new[] { new Tuple<int, int, string>(3, 3, "Right 3") }));

            // Assure that the right indices are available

            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Update Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Update Right 4" });

            Assert.IsNull(action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());

            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                        new[]
                        {
                            new Tuple<int, int, string>(2, 2, "Update Right 2"), 
                            new Tuple<int, int, string>(4, 2, "Update Right 4")
                        }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                        new[] { new Tuple<int, int, string>(3, 3, "Right 3") }));
        }

        [Test]
        public void TestRemoveRight()
        {
            var repository = _container.GetRepository<int, AdapterJoinMany>();

            IEnumerable<AdapterJoinMany> addedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> updatedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> replacedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> removedItems = new List<AdapterJoinMany>();
            ActionType? action = null;

            repository.Subscribe(e =>
            {
                action = e.Action;
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

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 4" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 5, ForeignKey = 3, Name = FILTERED_NAME });

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 3, Name = "Left 3" });

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                        new[]
                        {
                            new Tuple<int, int, string>(2, 2, "Right 2"), 
                            new Tuple<int, int, string>(3, 2, "Right 3"), 
                            new Tuple<int, int, string>(4, 2, "Right 4")
                        }),
                    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"), new Tuple<int, int, string>[0]));

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            _removeRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(3, 2, "Right 3"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));

            _removeRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 100" });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));

            _removeRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 100" });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));

            _removeRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 100" });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));
        }

        [Test]
        public void TestReloadRight()
        {
            var repository = _container.GetRepository<int, AdapterJoinMany>();

            IEnumerable<AdapterJoinMany> addedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> updatedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> replacedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> removedItems = new List<AdapterJoinMany>();
            ActionType? action = null;

            repository.Subscribe(e =>
            {
                action = e.Action;
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

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 4" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 5, ForeignKey = 3, Name = FILTERED_NAME });

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 3, Name = "Left 3" });

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(3, 2, "Right 3"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            _reloadRight.OnNext(new List<ModelRight>
            {
                new ModelRight { PrimaryKey = 100, ForeignKey = 1, Name = "Right 100" },
                new ModelRight { PrimaryKey = 103, ForeignKey = 3, Name = "Right 103" },
                new ModelRight { PrimaryKey = 104, ForeignKey = 3, Name = "Right 104" },
                new ModelRight { PrimaryKey = 105, ForeignKey = 3, Name = "Right 105" },
                new ModelRight { PrimaryKey = 106, ForeignKey = 3, Name = "Right 106" },
            });

            Assert.AreEqual(null, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(0, removedItems.Count());
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(100, 1, "Right 100") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new Tuple<int, int, string>[0]),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new[]
                    {
                        new Tuple<int, int, string>(103, 3, "Right 103"), 
                        new Tuple<int, int, string>(104, 3, "Right 104"), 
                        new Tuple<int, int, string>(105, 3, "Right 105"), 
                        new Tuple<int, int, string>(106, 3, "Right 106") 
                    }));
        }

        [Test]
        public void TestRemoveLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinMany>();

            IEnumerable<AdapterJoinMany> addedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> updatedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> replacedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> removedItems = new List<AdapterJoinMany>();
            ActionType? action = null;

            repository.Subscribe(e =>
            {
                action = e.Action;
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

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 4" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 5, ForeignKey = 3, Name = FILTERED_NAME });

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 3, Name = "Left 3" });

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(3, 2, "Right 3"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            _removeLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 200" });

            Assert.AreEqual(ActionType.Remove, action);
            Assert.AreEqual(0, addedItems.Count());
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(1, removedItems.Count());
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft.PrimaryKey == 2 && p.ModelLeft.Name == "Left 2" && p.ModelRights.Count == 0));
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));
        }

        [Test]
        public void TestReloadLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinMany>();

            IEnumerable<AdapterJoinMany> addedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> updatedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> replacedItems = new List<AdapterJoinMany>();
            IEnumerable<AdapterJoinMany> removedItems = new List<AdapterJoinMany>();
            ActionType? action = null;

            repository.Subscribe(e =>
            {
                action = e.Action;
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

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 4" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 5, ForeignKey = 3, Name = FILTERED_NAME });

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 3, Name = "Left 3" });

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(3, 2, "Right 3"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));

            action = null;
            addedItems = new List<AdapterJoinMany>();
            updatedItems = new List<AdapterJoinMany>();
            replacedItems = new List<AdapterJoinMany>();
            removedItems = new List<AdapterJoinMany>();

            var list = new List<ModelLeft>
            {
                new ModelLeft { PrimaryKey = 2, Name = "Left 200" },
                new ModelLeft { PrimaryKey = 4, Name = "Left 400" }
            };
            _reloadLeft.OnNext(list);

            Assert.AreEqual(ActionType.Reload, action);
            Assert.AreEqual(2, addedItems.Count());
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft.PrimaryKey == 2 && p.ModelLeft.Name == "Left 200" && p.ModelRights.Count == 3));
            Assert.IsTrue(addedItems.Any(p => p.ModelLeft.PrimaryKey == 4 && p.ModelLeft.Name == "Left 400" && p.ModelRights.Count == 0));
            Assert.AreEqual(0, updatedItems.Count());
            Assert.AreEqual(0, replacedItems.Count());
            Assert.AreEqual(3, removedItems.Count());
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft.PrimaryKey == 1 && p.ModelLeft.Name == "Left 1" && p.ModelRights.Count == 0));
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft.PrimaryKey == 2 && p.ModelLeft.Name == "Left 2" && p.ModelRights.Count == 0));
            Assert.IsTrue(removedItems.Any(p => p.ModelLeft.PrimaryKey == 3 && p.ModelLeft.Name == "Left 3" && p.ModelRights.Count == 0));
            AssertContains(repository,
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 200"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(3, 2, "Right 3"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(4, "Left 400"),
                    new Tuple<int, int, string>[0]));
        }

        [Test]
        public void TestGetSnapshotFromRightSource()
        {
            _container.Build<int, ModelRight>(p => p.PrimaryKey)
                .Register();

            _addRight.OnNext(new ModelRight { PrimaryKey = 1, ForeignKey = 1, Name = "Right 1" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 2, ForeignKey = 2, Name = "Right 2" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 3, ForeignKey = 2, Name = "Right 3" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 4, ForeignKey = 2, Name = "Right 4" });
            _addRight.OnNext(new ModelRight { PrimaryKey = 5, ForeignKey = 3, Name = FILTERED_NAME });

            Assert.AreEqual(5, _container.GetRepository<int, ModelRight>().Count);

            // Build repository and get snapshot from TRight source
            _container.Build<int, AdapterJoinMany, ModelLeft>("2", p => p.PrimaryKey)
                .JoinMany<ModelRight>(null, p => p.Name != FILTERED_NAME)
                    .DefineList(p => p.ModelRights)
                    .RightPrimaryKey(p => p.PrimaryKey)
                    .RightLinkKey(p => p.ForeignKey)
                    .LeftLinkKey(p => p.PrimaryKey)
                .Register();

            _addLeft.OnNext(new ModelLeft { PrimaryKey = 1, Name = "Left 1" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 2, Name = "Left 2" });
            _addLeft.OnNext(new ModelLeft { PrimaryKey = 3, Name = "Left 3" });

            AssertContains(_container.GetRepository<int, AdapterJoinMany>("2"),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
                    new[] { new Tuple<int, int, string>(1, 1, "Right 1") }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
                    new[]
                    {
                        new Tuple<int, int, string>(2, 2, "Right 2"), 
                        new Tuple<int, int, string>(3, 2, "Right 3"), 
                        new Tuple<int, int, string>(4, 2, "Right 4")
                    }),
                new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
                    new Tuple<int, int, string>[0]));

            //// Build repository without get snapshot from TRight source
            //container.Build<int, AdapterJoinMany, ModelLeft>("3", p => p.Id)
            //    .JoinMany<ModelRight>(null, p => p.Name != FILTERED_NAME)
            //        .DefineList(p => p.ModelRights)
            //        .RightPrimaryKey(p => p.Id)
            //        .RightLinkKey(p => p.Model1Id)
            //        .LeftLinkKey(p => p.Id)
            //    .Register();

            //addLeft.OnNext(new ModelLeft { Id = 1, Name = "Left 1" });
            //addLeft.OnNext(new ModelLeft { Id = 2, Name = "Left 2" });
            //addLeft.OnNext(new ModelLeft { Id = 3, Name = "Left 3" });

            //AssertContains(container.GetRepository<int, AdapterJoinMany>("3"),
            //    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
            //        new Tuple<int, int, string>[0]),
            //    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
            //        new Tuple<int, int, string>[0]),
            //    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
            //        new Tuple<int, int, string>[0]));

            //addRight.OnNext(new ModelRight { Id = 1, Model1Id = 1, Name = "Right 1 updated" });

            //AssertContains(container.GetRepository<int, AdapterJoinMany>("3"),
            //    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(1, "Left 1"),
            //        new[] { new Tuple<int, int, string>(1, 1, "Right 1 updated") }),
            //    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(2, "Left 2"),
            //        new Tuple<int, int, string>[0]),
            //    new Tuple<Tuple<int, string>, Tuple<int, int, string>[]>(new Tuple<int, string>(3, "Left 3"),
            //        new Tuple<int, int, string>[0]));
        }

        private static void AssertContains(IRepository<int, AdapterJoinMany> repository, params Tuple<Tuple<int, string>, Tuple<int, int, string>[]>[] adapters)
        {
            Assert.AreEqual(adapters.Length, repository.Count);
            foreach (var adapter in adapters)
            {
                var item = repository[adapter.Item1.Item1];
                Assert.AreEqual(adapter.Item1.Item1, item.ModelLeft.PrimaryKey);
                Assert.AreEqual(adapter.Item1.Item2, item.ModelLeft.Name);

                Assert.AreEqual(adapter.Item2.Length, item.ModelRights.Count);

                var count = adapter.Item2.Length;
                for (var i = 0; i < count; i++)
                {
                    Assert.AreEqual(adapter.Item2[i].Item1, item.ModelRights[i].PrimaryKey);
                    Assert.AreEqual(adapter.Item2[i].Item2, item.ModelRights[i].ForeignKey);
                    Assert.AreEqual(adapter.Item2[i].Item3, item.ModelRights[i].Name);
                }
            }
        }
    }
}
