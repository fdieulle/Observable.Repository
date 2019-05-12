using System.Collections.Generic;
using NUnit.Framework;
using Observable.Repository.Tests.Data;

namespace Observable.Repository.Tests
{
    [TestFixture]
    public class RepositoryJoinOneToBuildTests : RepositoryBaseTests
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

            _container.Build<int, AdapterJoin, ModelLeft>(p => p.PrimaryKey)
                .Join<ModelRight>(null, p => p.Name != FILTERED_NAME)
                    .RightLinkKey(p => p.ForeignKey)
                    .LeftLinkKey(p => p.PrimaryKey)
                .DefineCtor((p1, p2) => new AdapterJoin(p1, p2))
                .Register();
        }

        [Test]
        public void TestAddRightBeforeLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();
            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));
            _addRight.OnNext(R(4, 3, FILTERED_NAME));

            checker.CheckNoMoreNotifications();
            checker.Check();

            _addLeft.OnNext(L(1, "Left 1"));

            checker.CheckAdded(null, V(A(L(1, "Left 1"), R(1, 1, "Right 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(A(L(1, "Left 1"), R(1, 1, "Right 1")));

            _addLeft.OnNext(L(2, "Left 2"));

            checker.CheckAdded(null, V(A(L(2, "Left 2"), R(3, 2, "Right 3"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")));

            _addLeft.OnNext(L(3, "Left 3"));

            checker.CheckAdded(null, V(A(L(3, "Left 3"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")),
                A(L(3, "Left 3")));
        }

        [Test]
        public void TestAddRightAfterLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();
            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);

            _addLeft.OnNext(L(1, "Left 1"));

            checker.CheckAdded(null, V(A(L(1, "Left 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(A(L(1, "Left 1")));

            _addLeft.OnNext(L(2, "Left 2"));

            checker.CheckAdded(null, V(A(L(2, "Left 2"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1")),
                A(L(2, "Left 2")));

            _addRight.OnNext(R(1, 1, FILTERED_NAME));

            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1")),
                A(L(2, "Left 2")));

            _addRight.OnNext(R(1, 1, "Right 1"));

            checker.CheckUpdated(V(A(L(1, "Left 1"))), V(A(L(1, "Left 1"), R(1, 1, "Right 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2")));

            _addRight.OnNext(R(2, 2, "Right 2"));

            checker.CheckUpdated(V(A(L(2, "Left 2"))), V(A(L(2, "Left 2"), R(2, 2, "Right 2"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(2, 2, "Right 2")));

            _addRight.OnNext(R(3, 2, "Right 3"));

            checker.CheckUpdated(V(A(L(2, "Left 2"), R(2, 2, "Right 2"))), V(A(L(2, "Left 2"), R(3, 2, "Right 3"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")));

            _addRight.OnNext(R(4, 2, FILTERED_NAME));

            checker.CheckUpdated(V(A(L(2, "Left 2"), R(3, 2, "Right 3"))), V(A(L(2, "Left 2"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2")));
        }

        [Test]
        public void TestUpdateLeftThenUpdateRight()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();
            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);

            _addLeft.OnNext(L(1, "Left 1"));

            checker.CheckAdded(null, V(A(L(1, "Left 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(A(L(1, "Left 1")));

            _addRight.OnNext(R(1, 1, "Right 1"));

            checker.CheckUpdated(V(A(L(1, "Left 1"))), V(A(L(1, "Left 1"), R(1, 1, "Right 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(A(L(1, "Left 1"), R(1, 1, "Right 1")));

            _addLeft.OnNext(L(1, "Left 1 Updated"));

            checker.CheckUpdated(V(A(L(1, "Left 1"), R(1, 1, "Right 1"))), V(A(L(1, "Left 1 Updated"), R(1, 1, "Right 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(A(L(1, "Left 1 Updated"), R(1, 1, "Right 1")));

            _addRight.OnNext(R(1, 1, "Right 1 Updated"));

            checker.CheckUpdated(V(A(L(1, "Left 1 Updated"), R(1, 1, "Right 1"))), V(A(L(1, "Left 1 Updated"), R(1, 1, "Right 1 Updated"))));
            checker.CheckNoMoreNotifications();
            checker.Check(A(L(1, "Left 1 Updated"), R(1, 1, "Right 1 Updated")));
        }

        [Test]
        public void TestRemoveRight()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();
            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));

            _addLeft.OnNext(L(1, "Left 1"));
            _addLeft.OnNext(L(2, "Left 2"));

            checker.ClearNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")));


            _removeRight.OnNext(R(1, 1, "Right 1"));

            checker.CheckUpdated(V(A(L(1, "Left 1"), R(1, 1, "Right 1"))), V(A(L(1, "Left 1"))));
            checker.ClearNotifications();
            checker.Check(
                A(L(1, "Left 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")));

            _removeRight.OnNext(R(100, 2, "Right 100"));

            checker.CheckUpdated(V(A(L(2, "Left 2"), R(3, 2, "Right 3"))), V(A(L(2, "Left 2"))));
            checker.ClearNotifications();
            checker.Check(
                A(L(1, "Left 1")),
                A(L(2, "Left 2")));
        }

        [Test]
        public void TestReloadRight()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();

            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));

            _addLeft.OnNext(L(1, "Left 1"));
            _addLeft.OnNext(L(2, "Left 2"));
            _addLeft.OnNext(L(3, "Left 3"));

            checker.ClearNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")),
                A(L(3, "Left 3")));

            _reloadRight.OnNext(R(100, 1, "Right 100"), R(103, 3, "Right 103"));

            checker.CheckUpdated(
                V(
                    A(L(1, "Left 1"), R(1, 1, "Right 1")),
                    A(L(2, "Left 2"), R(3, 2, "Right 3")),
                    A(L(3, "Left 3"))),
                V(
                    A(L(1, "Left 1"), R(100, 1, "Right 100")),
                    A(L(2, "Left 2")),
                    A(L(3, "Left 3"), R(103, 3, "Right 103"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(100, 1, "Right 100")),
                A(L(2, "Left 2")),
                A(L(3, "Left 3"), R(103, 3, "Right 103")));
        }

        [Test]
        public void TestRemoveLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();
            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));

            _addLeft.OnNext(L(1, "Left 1"));
            _addLeft.OnNext(L(2, "Left 2"));
            _addLeft.OnNext(L(3, "Left 3"));

            checker.ClearNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")),
                A(L(3, "Left 3")));

            _removeLeft.OnNext(L(2, "Left 200"));

            checker.CheckRemoved(V(A(L(2, "Left 2"), R(3, 2, "Right 3"))), null);
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(3, "Left 3")));
        }

        [Test]
        public void TestReloadLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoin>();
            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));
            _addRight.OnNext(R(4, 4, "Right 4"));

            _addLeft.OnNext(L(1, "Left 1"));
            _addLeft.OnNext(L(2, "Left 2"));
            _addLeft.OnNext(L(3, "Left 3"));

            checker.ClearNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")),
                A(L(3, "Left 3")));

            _reloadLeft.OnNext(L(2, "Left 200"), L(4, "Left 400"));

            checker.CheckReloaded(
                V(
                    A(L(1, "Left 1"), R(1, 1, "Right 1")),
                    A(L(2, "Left 2"), R(3, 2, "Right 3")),
                    A(L(3, "Left 3"))
                ),
                V(
                    A(L(2, "Left 200"), R(3, 2, "Right 3")),
                    A(L(4, "Left 400"), R(4, 4, "Right 4"))));
        }

        [Test]
        public void TestChangeLinkKeyOnLeft()
        {
            var checker = _container.Build<int, AdapterJoin, ModelLeft>(p => p.PrimaryKey)
                     .Join<ModelRight>()
                         .RightLinkKey(p => p.ForeignKey)
                         .LeftLinkKey(p => p.ForeignKey)
                     .Create()
                     .GetChecker(p => p.ModelLeft.PrimaryKey);

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));
            _addRight.OnNext(R(4, 4, "Right 4"));

            _addLeft.OnNext(L(1, "Left 1", 1));
            _addLeft.OnNext(L(2, "Left 2", 2));
            _addLeft.OnNext(L(3, "Left 3", 4));

            checker.ClearNotifications();
            checker.Check(
                A(L(1, "Left 1", 1), R(1, 1, "Right 1")),
                A(L(2, "Left 2", 2), R(3, 2, "Right 3")),
                A(L(3, "Left 3", 4), R(4, 4, "Right 4")));

            _addLeft.OnNext(L(3, "Left 3", 2));

            checker.CheckNotification(ActionType.Update,
                V(A(L(3, "Left 3", 4), R(4, 4, "Right 4"))),
                V(A(L(3, "Left 3", 2), R(3, 2, "Right 3"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
               A(L(1, "Left 1", 1), R(1, 1, "Right 1")),
               A(L(2, "Left 2", 2), R(3, 2, "Right 3")),
               A(L(3, "Left 3", 2), R(3, 2, "Right 3")));

            _addRight.OnNext(R(4, 4, "Right 4 Updated"));
            checker.CheckNoMoreNotifications();
            checker.Check(
               A(L(1, "Left 1", 1), R(1, 1, "Right 1")),
               A(L(2, "Left 2", 2), R(3, 2, "Right 3")),
               A(L(3, "Left 3", 2), R(3, 2, "Right 3")));

            _addLeft.OnNext(L(3, "Left 3", 4));

            checker.CheckNotification(ActionType.Update,
                V(A(L(3, "Left 3", 2), R(3, 2, "Right 3"))),
                V(A(L(3, "Left 3", 4), R(4, 4, "Right 4 Updated"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1", 1), R(1, 1, "Right 1")),
                A(L(2, "Left 2", 2), R(3, 2, "Right 3")),
                A(L(3, "Left 3", 4), R(4, 4, "Right 4 Updated")));
        }

        [Test]
        public void TestGetSnapshotFromRightSource()
        {
            _container.Build<int, ModelRight>(p => p.PrimaryKey)
                .Register();

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));
            _addRight.OnNext(R(4, 2, "Right 4"));
            _addRight.OnNext(R(5, 3, FILTERED_NAME));

            Assert.AreEqual(5, _container.GetRepository<int, ModelRight>().Count);

            // Build repository and get snapshot from TRight source
            _container.Build<int, AdapterJoin, ModelLeft>("2", p => p.PrimaryKey)
                 .Join<ModelRight>(null, p => p.Name != FILTERED_NAME)
                     .RightLinkKey(p => p.ForeignKey)
                     .LeftLinkKey(p => p.PrimaryKey)
                 .DefineCtor((p1, p2) => new AdapterJoin(p1, p2))
                 .Register();

            _addLeft.OnNext(L(1, "Left 1"));
            _addLeft.OnNext(L(2, "Left 2"));
            _addLeft.OnNext(L(3, "Left 3"));

            _container.GetRepository<int, AdapterJoin>("2").GetChecker(p => p.ModelLeft.PrimaryKey).Check(
                    A(L(1, "Left 1"), R(1, 1, "Right 1")),
                    A(L(2, "Left 2"), R(4, 2, "Right 4")),
                    A(L(3, "Left 3")));
            
            //// Build repository without get snapshot from TRight source
            //container.Build<int, AdapterJoin, ModelLeft>("3", p => p.Id)
            //     .Join<ModelRight>(null, p => p.Name != FILTERED_NAME, false)
            //         .RightForeignKey(p => p.Model1Id)
            //         .LeftKey(p => p.Id)
            //     .DefineCtor((p1, p2) => new AdapterJoin(p1, p2))
            //    .Register();

            //addLeft.OnNext(new ModelLeft { Id = 1, Name = "Left 1" });
            //addLeft.OnNext(new ModelLeft { Id = 2, Name = "Left 2" });
            //addLeft.OnNext(new ModelLeft { Id = 3, Name = "Left 3" });

            //AssertContains(container.GetRepository<int, AdapterJoin>("3"),
            //    new Adapter(new Left(1, "Left 1"), null),
            //    new Adapter(new Left(2, "Left 2"), null),
            //    new Adapter(new Left(3, "Left 3"), null));

            //addRight.OnNext(new ModelRight { Id = 1, Model1Id = 1, Name = "Right 1 updated" });

            //AssertContains(container.GetRepository<int, AdapterJoin>("3"),
            //    new Adapter(new Left(1, "Left 1"), new Right(1, 1, "Right 1 updated")),
            //    new Adapter(new Left(2, "Left 2"), null),
            //    new Adapter(new Left(3, "Left 3"), null));
        }

        private static AdapterJoin A(ModelLeft left, ModelRight right = null)
        {
            return new AdapterJoin(left, right);
        }
    }
}
