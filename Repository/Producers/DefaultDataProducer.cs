using System;

namespace Observable.Repository.Producers
{
    public class DefaultDataProducer : AbstractDataProducer
    {
        private readonly Action<Action> _dispatcher;

        public Action<Action> Dispatcher { get { return _dispatcher; } }

        public DefaultDataProducer(Action<Action> dispatcher)
        {
            this._dispatcher = dispatcher;
        }

        #region Overrides of AbstractDataProducer

        protected override Producer<T> CreateProducer<T>()
        {
            return new Producer<T>(_dispatcher);
        }

        #endregion
    }
}
