using System;

namespace Observable.Anonymous
{
    public static class Anonymous
    {
        public static readonly Action DefaultOnAction = () => { };
        public static readonly Action<Exception> DefaultOnError = e => { };
    }
}
