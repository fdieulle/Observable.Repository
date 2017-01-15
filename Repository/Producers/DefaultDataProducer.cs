using System;

namespace Observable.Repository.Producers
{
    public class DefaultDataProducer : AbstractDataProducer
    {
        private readonly Action<Action> dispatcher;

        public Action<Action> Dispatcher { get { return dispatcher; } }

        public DefaultDataProducer(Action<Action> dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        #region Overrides of AbstractDataProducer

        protected override Producer<T> CreateProducer<T>()
        {
            return new Producer<T>(dispatcher);
        }

        #endregion
    }
}
