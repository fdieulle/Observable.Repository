using System;

namespace Observable.Repository.Tests.Data
{
    public class T1
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }
    }

    public class T2
    {
        public long Id { get; set; }

        public int T1Id { get; set; }

        public string Name { get; set; }
    }

    public class T3
    {
        public DateTime Date { get; set; }

        public int T1Id { get; set; }

        public string Name { get; set; }
    }

    public class T4
    {
        public int Id { get; set; }

        public int T1Id { get; set; }

        public string Name { get; set; }
    }

    public class T5
    {
        public int Id { get; set; }

        public int T1Id { get; set; }

        public string Name { get; set; }
    }
}
