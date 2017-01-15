using System.Diagnostics;

namespace Observable.Repository.Tests.Tools
{
    public class MonitorProduce
    {
        public string Name { get; set; }

        public Stopwatch Stopwatch { get; set; }

        public int OperationCount { get; set; }
    }
}
