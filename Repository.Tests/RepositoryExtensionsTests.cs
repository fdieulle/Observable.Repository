using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Observable.Repository.Tests.Data;

namespace Observable.Repository.Tests
{
    [TestFixture]
    public class RepositoryExtensionsTests
    {
        [Test]
        public void RepositoryContainerBuildTests()
        {
            var mockContainer = new Mock<IRepositoryContainer>();
            var container = mockContainer.Object;

            const string name = "Name";
            var getKey = new Func<ModelLeft, int>(p => p.PrimaryKey);
            const string leftSource = "LeftSourceName";
            var onUpdateItSelf = new Action<ModelLeft, ModelLeft>((p1, p2) => {});
            var onUpdate = new Action<AdapterJoin, ModelLeft>((p1, p2) => { });
            var filter = new Func<ModelLeft, bool>(p => true);
            var dispatcher = new Action<Action>(p => p());

            container.Build(name, getKey, onUpdate, leftSource, filter, true, dispatcher);
            mockContainer.Verify(m => m.Build(
                It.Is<string>(i => i == name),
                It.Is<Func<ModelLeft, int>>(i => i == getKey),
                It.Is<Action<AdapterJoin, ModelLeft>>(i => i == onUpdate),
                It.Is<string>(i => i == leftSource),
                It.Is<Func<ModelLeft, bool>>(i => i == filter),
                It.Is<bool>(i => i),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            container.Build(getKey, onUpdate, leftSource, filter, true, dispatcher);
            mockContainer.Verify(m => m.Build(
                It.Is<string>(i => i == null),
                It.Is<Func<ModelLeft, int>>(i => i == getKey),
                It.Is<Action<AdapterJoin, ModelLeft>>(i => i == onUpdate),
                It.Is<string>(i => i == leftSource),
                It.Is<Func<ModelLeft, bool>>(i => i == filter),
                It.Is<bool>(i => i),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            container.Build(name, getKey, onUpdateItSelf, leftSource, filter, true, dispatcher);
            mockContainer.Verify(m => m.Build(
                It.Is<string>(i => i == name),
                It.Is<Func<ModelLeft, int>>(i => i == getKey),
                It.Is<Action<ModelLeft, ModelLeft>>(i => i == onUpdateItSelf),
                It.Is<string>(i => i == leftSource),
                It.Is<Func<ModelLeft, bool>>(i => i == filter),
                It.Is<bool>(i => i),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            container.Build<int, ModelLeft, ModelLeft>(getKey, onUpdateItSelf, leftSource, filter, true, dispatcher);
            mockContainer.Verify(m => m.Build(
                It.Is<string>(i => i == null),
                It.Is<Func<ModelLeft, int>>(i => i == getKey),
                It.Is<Action<ModelLeft, ModelLeft>>(i => i == onUpdateItSelf),
                It.Is<string>(i => i == leftSource),
                It.Is<Func<ModelLeft, bool>>(i => i == filter),
                It.Is<bool>(i => i),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());
        }

        [Test]
        public void RepositorySubscribeViewTests()
        {
            var mockRepository = new Mock<IRepository<int, ModelLeft>>();

            var instance = mockRepository.Object;
            var view = new List<ModelLeft>();
            var viewSelector = new List<Tuple<ModelLeft>>();
            var selector = new Func<ModelLeft, Tuple<ModelLeft>>(p => new Tuple<ModelLeft>(p));
            var filter = new Predicate<ModelLeft>(p => true);
            var dispatcher = new Action<Action>(p => p());

            instance.Subscribe(view);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == null),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());

            instance.Subscribe(viewSelector, selector);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<Tuple<ModelLeft>>>(i => i == viewSelector),
                It.Is<Func<ModelLeft, Tuple<ModelLeft>>>(i => i == selector),
                It.Is<Predicate<ModelLeft>>(i => i == null),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());

            instance.Subscribe(view, filter);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == filter),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());

            instance.Subscribe(view, synchronize: false);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == null),
                It.Is<bool>(i => i == false),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());
            
            instance.Subscribe(view, viewDispatcher: dispatcher);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == null),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            instance.Subscribe(viewSelector, selector, filter);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<Tuple<ModelLeft>>>(i => i == viewSelector),
                It.Is<Func<ModelLeft, Tuple<ModelLeft>>>(i => i == selector),
                It.Is<Predicate<ModelLeft>>(i => i == filter),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());

            instance.Subscribe(viewSelector, selector, synchronize: false);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<Tuple<ModelLeft>>>(i => i == viewSelector),
                It.Is<Func<ModelLeft, Tuple<ModelLeft>>>(i => i == selector),
                It.Is<Predicate<ModelLeft>>(i => i == null),
                It.Is<bool>(i => i == false),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());

            instance.Subscribe(viewSelector, selector, viewDispatcher: dispatcher);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<Tuple<ModelLeft>>>(i => i == viewSelector),
                It.Is<Func<ModelLeft, Tuple<ModelLeft>>>(i => i == selector),
                It.Is<Predicate<ModelLeft>>(i => i == null),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            instance.Subscribe(viewSelector, selector, filter, false);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<Tuple<ModelLeft>>>(i => i == viewSelector),
                It.Is<Func<ModelLeft, Tuple<ModelLeft>>>(i => i == selector),
                It.Is<Predicate<ModelLeft>>(i => i == filter),
                It.Is<bool>(i => i == false),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());

            instance.Subscribe(viewSelector, selector, filter, viewDispatcher: dispatcher);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<Tuple<ModelLeft>>>(i => i == viewSelector),
                It.Is<Func<ModelLeft, Tuple<ModelLeft>>>(i => i == selector),
                It.Is<Predicate<ModelLeft>>(i => i == filter),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            instance.Subscribe(view, filter, false);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == filter),
                It.Is<bool>(i => i == false),
                It.Is<Action<Action>>(i => i == null)),
                Times.Once());

            instance.Subscribe(view, filter, viewDispatcher: dispatcher);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == filter),
                It.Is<bool>(i => i == true),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            instance.Subscribe(view, filter, false, dispatcher);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == filter),
                It.Is<bool>(i => i == false),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());

            instance.Subscribe(view, synchronize: false, viewDispatcher: dispatcher);
            mockRepository.Verify(m => m.Subscribe(
                It.Is<IList<ModelLeft>>(i => i == view),
                It.Is<Func<ModelLeft, ModelLeft>>(i => i != null),
                It.Is<Predicate<ModelLeft>>(i => i == null),
                It.Is<bool>(i => i == false),
                It.Is<Action<Action>>(i => i == dispatcher)),
                Times.Once());
        }
    }
}
