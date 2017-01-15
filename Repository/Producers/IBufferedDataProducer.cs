using System;

namespace Observable.Repository.Producers
{
    public interface IBufferedDataProducer : IDisposable
    {
        void Flush();
    }
}
