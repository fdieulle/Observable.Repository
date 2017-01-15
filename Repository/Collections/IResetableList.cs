using System.Collections;
using System.Collections.Generic;

namespace Observable.Repository.Collections
{
    public interface IResetableList : IList
    {
        void Reset(IEnumerable source);
    }

    public interface IResetableList<T> : IList<T>
    {
        void Reset(IEnumerable<T> source);
    }
}
