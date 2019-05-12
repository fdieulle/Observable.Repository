using System;

namespace Observable.Repository.Producers
{
    public class DefaultDataProducer : AbstractDataProducer
    {
        public Action<Action> Dispatcher { get; }

        public DefaultDataProducer(Action<Action> dispatcher)
        {
            Dispatcher = dispatcher;
        }

        #region Overrides of AbstractDataProducer

        protected override Producer<T> CreateProducer<T>() => new Producer<T>(Dispatcher);

        #endregion
    }
}
