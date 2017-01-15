using System;
using System.Collections.Generic;

namespace Observable.Repository.Tests.Tools
{
    public static class Producer
    {
        public static Produce<T> Produce<T>(string name, int count, params Func<int, T>[] createItems)
        {
            return Produce(name, count, 0, createItems);
        }

        public static Produce<T> Produce<T>(string name, int count, int startIndex, params Func<int, T>[] createItems)
        {
            var produce = new Produce<T> {Name = name};
            var length = createItems.Length;
            produce.OperationsCount = length;

            var items = new List<T>();

            var halfCount = count / 2;
            for (var i = 0; i < halfCount; i++)
            {
                for (var j = 0; j < length; j++)
                    items.Add(createItems[j](startIndex + i));
            }

            for (var j = 0; j < length; j++)
            {
                for (var i = halfCount; i < count; i++)
                    items.Add(createItems[j](startIndex + i));
            }

            produce.Items = items;

            return produce;
        }
    }
}
