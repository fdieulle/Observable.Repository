namespace Observable.Repository.Core
{
    public sealed class Mutex
    {
        public readonly object _input = new object();
        public readonly object _output = new object();
    }
}
