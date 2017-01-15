using System;
using System.Collections.Generic;

namespace Observable.Repository.Collections
{
    public class Pool<T>
    {
        private readonly Func<T> factory;
        private readonly Stack<T> items = new Stack<T>();
 
        public Pool(Func<T> factory, int capacity = 0)
        {
            this.factory = factory;

            for(var i=0;i <capacity; i++)
                items.Push(factory());
        }

        public T Get()
        {
            return items.Count == 0 
                ? factory() 
                : items.Pop();
        }

        public void Free(T item)
        {
            items.Push(item);
        }

        public void Clear()
        {
            items.Clear();
        }
    }
}
