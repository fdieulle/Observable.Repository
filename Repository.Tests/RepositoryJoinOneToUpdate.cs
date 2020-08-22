using System.Collections.Generic;
using Observable.Repository.Tests.Data;
using Xunit;

namespace Observable.Repository.Tests
{
    public class RepositoryJoinOneToUpdateTests : RepositoryBaseTests
    {
        private readonly IRepositoryContainer _container;
        private readonly Subject<ModelLeft> _addLeft;
        private readonly Subject<ModelLeft> _removeLeft;
        private readonly Subject<List<ModelLeft>> _reloadLeft;
        private readonly Subject<ModelRight> _addRight;
        private readonly Subject<ModelRight> _removeRight;
        private readonly Subject<List<ModelRight>> _reloadRight;
        private const string FILTERED_NAME = "FILTER";

        public RepositoryJoinOneToUpdateTests()
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

            _container.Build<int, AdapterJoinToUpdate, ModelLeft>(p => p.PrimaryKey)
                .JoinUpdate<ModelRight>(null, p => p.Name != FILTERED_NAME)
                    .DefineUpdate(p => p.Update)
                    .RightLinkKey(p => p.ForeignKey)
                    .LeftLinkKey(p => p.PrimaryKey)
                .Register();
        }

        [Fact]
        public void TestAddRightBeforeLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinToUpdate>();
            var checker = repository.GetChecker(p => p.ModelLeft.PrimaryKey);
            
            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 2, "Right 3"));
            _addRight.OnNext(R(4, 3, FILTERED_NAME));

            checker.CheckNoMoreNotifications();
            checker.Check(); // Means empty repository

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

        [Fact]
        public void TestAddRightAfterLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinToUpdate>();
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

            // Can't distinct old value than new one because we update the reference
            checker.CheckUpdated(V(A(L(1, "Left 1"), R(1, 1, "Right 1"))), V(A(L(1, "Left 1"), R(1, 1, "Right 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")), 
                A(L(2, "Left 2")));

            _addRight.OnNext(R(2, 2, "Right 2"));

            // Can't distinct old value than new one because we update the reference
            checker.CheckUpdated(V(A(L(2, "Left 2"), R(2, 2, "Right 2"))), V(A(L(2, "Left 2"), R(2, 2, "Right 2"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")), 
                A(L(2, "Left 2"), R(2, 2, "Right 2")));

            _addRight.OnNext(R(3, 2, "Right 3"));

            // Can't distinct old value than new one because we update the reference
            checker.CheckUpdated(V(A(L(2, "Left 2"), R(3, 2, "Right 3"))), V(A(L(2, "Left 2"), R(3, 2, "Right 3"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")), 
                A(L(2, "Left 2"), R(3, 2, "Right 3")));

            _addRight.OnNext(R(4, 2, FILTERED_NAME));

            // Can't distinct old value than new one because we update the reference
            checker.CheckUpdated(V(A(L(2, "Left 2"))), V(A(L(2, "Left 2"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1"), R(1, 1, "Right 1")), 
                A(L(2, "Left 2")));
        }

        [Fact]
        public void TestRemoveRight()
        {
            var repository = _container.GetRepository<int, AdapterJoinToUpdate>();
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

            // Can't distinct old value than new one because we update the reference
            checker.CheckUpdated(V(A(L(1, "Left 1"))), V(A(L(1, "Left 1"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1")),
                A(L(2, "Left 2"), R(3, 2, "Right 3")));

            _removeRight.OnNext(R(100, 2, "Right 100"));

            // Normal side effect, Because right foreign key match Left 2 primary key
            checker.CheckUpdated(V(A(L(2, "Left 2"))), V(A(L(2, "Left 2"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(1, "Left 1")),
                A(L(2, "Left 2")));

        }

        [Fact]
        public void TestReloadRight()
        {
            var repository = _container.GetRepository<int, AdapterJoinToUpdate>();
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

            // Can't distinct old value than new one because we update the reference
            checker.CheckUpdated(
                V(
                     A(L(1, "Left 1"), R(100, 1, "Right 100")),
                    A(L(2, "Left 2")),
                    A(L(3, "Left 3"), R(103, 3, "Right 103"))),
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

        [Fact]
        public void TestRemoveLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinToUpdate>();
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

        [Fact]
        public void TestReloadLeft()
        {
            var repository = _container.GetRepository<int, AdapterJoinToUpdate>();
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
                    A(L(3, "Left 3"))),
                V(
                    A(L(2, "Left 200"), R(3, 2, "Right 3")),
                    A(L(4, "Left 400"), R(4, 4, "Right 4"))));
            checker.CheckNoMoreNotifications();
            checker.Check(
                A(L(2, "Left 200"), R(3, 2, "Right 3")),
                A(L(4, "Left 400"), R(4, 4, "Right 4")));
        }

        [Fact]
        public void TestChangeLinkKeyOnLeft()
        {
            var checker = _container.Build<int, AdapterJoinToUpdate, ModelLeft>(p => p.PrimaryKey)
                     .JoinUpdate<ModelRight>()
                         .DefineUpdate(p => p.Update)
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

        [Fact]
        public void TestGetSnapshotFromRightSource()
        {
            _container.Build<int, ModelRight>(p => p.PrimaryKey)
                .Register();

            _addRight.OnNext(R(1, 1, "Right 1"));
            _addRight.OnNext(R(2, 2, "Right 2"));
            _addRight.OnNext(R(3, 3, "Right 3"));
            _addRight.OnNext(R(4, 2, "Right 4"));
            _addRight.OnNext(R(5, 3, FILTERED_NAME));

            var checker = _container.GetRepository<int, ModelRight>().GetChecker(p => p.PrimaryKey);
            checker.Check(
                R(1, 1, "Right 1"),
                R(2, 2, "Right 2"),
                R(3, 3, "Right 3"),
                R(4, 2, "Right 4"),
                R(5, 3, FILTERED_NAME));
            Assert.Equal(5, _container.GetRepository<int, ModelRight>().Count);

            // Build repository and get snapshot from TRight source
            _container.Build<int, AdapterJoinToUpdate, ModelLeft>("2", p => p.PrimaryKey)
                .JoinUpdate<ModelRight>(null, p => p.Name != FILTERED_NAME)
                    .DefineUpdate(p => p.Update)
                    .RightLinkKey(p => p.ForeignKey)
                    .LeftLinkKey(p => p.PrimaryKey)
                .Register();

            _addLeft.OnNext(L(1, "Left 1"));
            _addLeft.OnNext(L(2, "Left 2"));
            _addLeft.OnNext(L(3, "Left 3"));

            _container.GetRepository<int, AdapterJoinToUpdate>("2")
                .GetChecker(p => p.ModelLeft.PrimaryKey)
                .Check(
                    A(L(1, "Left 1"), R(1, 1, "Right 1")),
                    A(L(2, "Left 2"), R(4, 2, "Right 4")),
                    A(L(3, "Left 3")));
            
            //// Build repository without get snapshot from TRight source
            //container.Build<ModelLeft, AdapterJoinToUpdate>()
            //     .DefineKey(p => p.Id)
            //     .JoinToUpdate<ModelRight>(false, p => p.Name != FILTERED_NAME)
            //         .DefineUpdate(p => p.Update)
            //         .RightForeignKey(p => p.Model1Id)
            //         .LeftKey(p => p.Id)
            //     .Configure()
            //    .Register("3");

            //addLeft.OnNext(new ModelLeft { Id = 1, Name = "Left 1" });
            //addLeft.OnNext(new ModelLeft { Id = 2, Name = "Left 2" });
            //addLeft.OnNext(new ModelLeft { Id = 3, Name = "Left 3" });

            //AssertContains(container.GetRepository<int, AdapterJoinToUpdate>("3"),
            //    new Adapter(new Left(1, "Left 1"), null),
            //    new Adapter(new Left(2, "Left 2"), null),
            //    new Adapter(new Left(3, "Left 3"), null));

            //addRight.OnNext(new ModelRight { Id = 1, Model1Id = 1, Name = "Right 1 updated" });

            //AssertContains(container.GetRepository<int, AdapterJoinToUpdate>("3"),
            //    new Adapter(new Left(1, "Left 1"), new Right(1, 1, "Right 1 updated")),
            //    new Adapter(new Left(2, "Left 2"), null),
            //    new Adapter(new Left(3, "Left 3"), null));
        }

        private static AdapterJoinToUpdate A(ModelLeft left, ModelRight right = null)
        {
            var adapter = new AdapterJoinToUpdate(left);
            adapter.Update(right);
            return adapter;
        }
    }
}
