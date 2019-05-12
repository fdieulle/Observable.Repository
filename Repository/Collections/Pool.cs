using System;
using System.Collections.Generic;

namespace Observable.Repository.Collections
{
    public class Pool<T>
    {
        private readonly Func<T> _factory;
        private readonly Stack<T> _items = new Stack<T>();
 
        public Pool(Func<T> factory, int capacity = 0)
        {
            this._factory = factory;

            for(var i=0;i <capacity; i++)
                _items.Push(factory());
        }

        public T Get()
        {
            return _items.Count == 0 
                ? _factory() 
                : _items.Pop();
        }

        public void Free(T item)
        {
            _items.Push(item);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
