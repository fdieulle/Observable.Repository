namespace Observable.Repository.Core
{
    public sealed class Mutex
    {
        public readonly object input = new object();
        public readonly object output = new object();
    }
}
