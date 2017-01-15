using System.Collections.Generic;

namespace Observable.Repository.Tests.Tools
{
    public class Produce<T>
    {
        public string Name { get; set; }
        public List<T> Items { get; set; }
        public int OperationsCount { get; set; }
    }
}
