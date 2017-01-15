using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Observable.Tools;

namespace Observable.Repository.Tests
{
    [TestFixture]
    public class PerformancesTest
    {
        [Test]
        public void DispatchTest()
        {
            var normal = new NoDispatcherTest();
            var dispatch = new DispatcherTest(p => { });
            var noDispatch = new DispatcherTest(null);
            var alwaysDispatch = new AlwaysDispatchedTest(null);

            // Jitter
            for (var i = 0; i < 1000; i++)
            {
                normal.Method1();
                normal.Method2(i);
                dispatch.Method1();
                dispatch.Method2(i);
                noDispatch.Method1();
                noDispatch.Method2(i);
                alwaysDispatch.Method1();
                alwaysDispatch.Method2(i);
            }

            TestDispatch("Normal           ", normal);
            TestDispatch("Dispatch         ", dispatch);
            TestDispatch("No Dispatch      ", noDispatch);
            TestDispatch("Always Dispatched", alwaysDispatch);
        }

        private static void TestDispatch(string name, IDispatcherTest dispatcher)
        {
            const int count = 1000000;

            Console.WriteLine(name);

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                dispatcher.Method1();
            }
            sw.Stop();

            Console.WriteLine("Method1 : {0} µs, {1}", sw.Elapsed.TotalMilliseconds / count * 1e3, dispatcher.Counter);

            sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                dispatcher.Method2(i);
            }
            sw.Stop();

            Console.WriteLine("Method2 : {0} µs, {1}", sw.Elapsed.TotalMilliseconds / count * 1e3, dispatcher.Counter);
        }

        private interface IDispatcherTest
        {
            int Counter { get; }

            void Method1();
            void Method2(int parameter);
        }

        private class NoDispatcherTest : IDispatcherTest
        {
            private int counter;

            public int Counter { get { return counter; } }

            public void Method1()
            {
                counter += 1;
            }

            public void Method2(int parameter)
            {
                counter += parameter;
            }
        }

        private class DispatcherTest : IDispatcherTest
        {
            private readonly Action<Action> dispatch;
            private int counter;

            public int Counter { get { return counter; } }

            public DispatcherTest(Action<Action> dispatch)
            {
                this.dispatch = dispatch;
            }

            public void Method1()
            {
                if (dispatch != null)
                    dispatch(InternalMethod1);
                else InternalMethod1();
            }

            private void InternalMethod1()
            {
                counter += 1;
            }

            public void Method2(int parameter)
            {
                if (dispatch != null)
                    dispatch(() => InternalMethod2(parameter));
                else InternalMethod2(parameter);
            }

            private void InternalMethod2(int parameter)
            {
                counter += parameter;
            }
        }

        private class AlwaysDispatchedTest : IDispatcherTest
        {
            private readonly Action<Action> dispatch;
            private int counter;

            public int Counter { get { return counter; } }

            public AlwaysDispatchedTest(Action<Action> dispatch)
            {
                this.dispatch = dispatch ?? (p => p());
            }

            public void Method1()
            {
                dispatch(() =>
                {
                    counter += 1;
                });
            }

            public void Method2(int parameter)
            {
                dispatch(() =>
                {
                    counter += parameter;
                });
            }
        }

        [Ignore("Take too many times")]
        [TestCase(10000, false)]
        [TestCase(10, true)]
        public void DictionaryTest(int times, bool checkMemory)
        {
            const int nbKeys = 100;
            //const int times = 10;
            const int jitterTimes = 1000;
            //const bool checkMemory = true;

            var names = new string[nbKeys];
            for (var i = 0; i < nbKeys; i++)
                names[i] = "Name " + i;
            var types = new [] { typeof (object), typeof (int), typeof (double), typeof (string), typeof (long), typeof (DateTime), typeof (float) };

            var f1 = new Func<int, string, int, Type, KeyClass>((i, n, j, t) => new KeyClass(n, t));
            var f2 = new Func<int, string, int, Type, KeyClassWithHasCodeReadOnly>((i, n, j, t) => new KeyClassWithHasCodeReadOnly(n, t));
            var f3 = new Func<int, string, int, Type, KeyStruct>((i, n, j, t) => new KeyStruct(n, t));
            var f4 = new Func<int, string, int, Type, KeyStructWithHasCodeReadOnly>((i, n, j, t) => new KeyStructWithHasCodeReadOnly(n, t));
            var f5 = new Func<int, string, int, Type, KeyStructWithIEquatable>((i, n, j, t) => new KeyStructWithIEquatable(n, t));
            var f6 = new Func<int, string, int, Type, KeyStructWithIEquatable>((i, n, j, t) => new KeyStructWithIEquatable(n, t));
            var f7 = new Func<int, string, int, Type, KeyClassNative>((i, n, j, t) => new KeyClassNative(i, j));
            var f8 = new Func<int, string, int, Type, KeyStructNative>((i, n, j, t) => new KeyStructNative(i, j));
            var f9 = new Func<int, string, int, Type, KeyStructNative>((i, n, j, t) => new KeyStructNative(i, j));

            var d1 = new Dictionary<KeyClass, object>();
            var d2 = new Dictionary<KeyClassWithHasCodeReadOnly, object>();
            var d3 = new Dictionary<KeyStruct, object>();
            var d4 = new Dictionary<KeyStructWithHasCodeReadOnly, object>();
            var d5 = new Dictionary<KeyStructWithIEquatable, object>();
            var d6 = new Dictionary<KeyStructWithIEquatable, object>(KeyStructWithIEquatable.Comparer);
            var d7 = new Dictionary<KeyClassNative, object>();
            var d8 = new Dictionary<KeyStructNative, object>();
            var d9 = new Dictionary<KeyStructNative, object>(KeyStructNative.Comparer);

            Console.WriteLine("Running TryGet then add ...");

            var m1 = Benchmark.Measure(RunTryGet, d1, f1, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m2 = Benchmark.Measure(RunTryGet, d2, f2, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m3 = Benchmark.Measure(RunTryGet, d3, f3, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m4 = Benchmark.Measure(RunTryGet, d4, f4, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m5 = Benchmark.Measure(RunTryGet, d5, f5, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m6 = Benchmark.Measure(RunTryGet, d6, f6, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m7 = Benchmark.Measure(RunTryGet, d7, f7, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m8 = Benchmark.Measure(RunTryGet, d8, f8, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());
            var m9 = Benchmark.Measure(RunTryGet, d9, f9, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: (p1, p2, p3, p4) => p1.Clear());

            Console.WriteLine("Class:             {0}", m1);
            Console.WriteLine("Class With Hash:   {0}", m2);
            Console.WriteLine("Struct:            {0}", m3);
            Console.WriteLine("Struct With Hash : {0}", m4);
            Console.WriteLine("Struct IEquatable: {0}", m5);
            Console.WriteLine("Struct Comparer:   {0}", m6);
            Console.WriteLine("Class Native:      {0}", m7);
            Console.WriteLine("Struct Native:     {0}", m8);
            Console.WriteLine("Struct Native + c: {0}", m9);

            Console.WriteLine("===");
            Console.WriteLine("Running TryGet ...");

            m1 = Benchmark.Measure(RunTryGet, d1, f1, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m2 = Benchmark.Measure(RunTryGet, d2, f2, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m3 = Benchmark.Measure(RunTryGet, d3, f3, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m4 = Benchmark.Measure(RunTryGet, d4, f4, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m5 = Benchmark.Measure(RunTryGet, d5, f5, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m6 = Benchmark.Measure(RunTryGet, d6, f6, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m7 = Benchmark.Measure(RunTryGet, d7, f7, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m8 = Benchmark.Measure(RunTryGet, d8, f8, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m9 = Benchmark.Measure(RunTryGet, d9, f9, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());

            Console.WriteLine("Class:             {0}", m1);
            Console.WriteLine("Class With Hash:   {0}", m2);
            Console.WriteLine("Struct:            {0}", m3);
            Console.WriteLine("Struct With Hash : {0}", m4);
            Console.WriteLine("Struct IEquatable: {0}", m5);
            Console.WriteLine("Struct Comparer:   {0}", m6);
            Console.WriteLine("Class Native:      {0}", m7);
            Console.WriteLine("Struct Native:     {0}", m8);
            Console.WriteLine("Struct Native + c: {0}", m9);

            Console.WriteLine("===");
            Console.WriteLine("Running ContainsKey ...");

            m1 = Benchmark.Measure(RunContainsKey, d1, f1, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m2 = Benchmark.Measure(RunContainsKey, d2, f2, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m3 = Benchmark.Measure(RunContainsKey, d3, f3, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m4 = Benchmark.Measure(RunContainsKey, d4, f4, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m5 = Benchmark.Measure(RunContainsKey, d5, f5, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m6 = Benchmark.Measure(RunContainsKey, d6, f6, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m7 = Benchmark.Measure(RunContainsKey, d7, f7, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m8 = Benchmark.Measure(RunContainsKey, d8, f8, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());
            m9 = Benchmark.Measure(RunContainsKey, d9, f9, names, types, times, jitterTimes: jitterTimes, checkMemory: checkMemory, begin: Add, end: (p1, p2, p3, p4) => p1.Clear());

            Console.WriteLine("Class:             {0}", m1);
            Console.WriteLine("Class With Hash:   {0}", m2);
            Console.WriteLine("Struct:            {0}", m3);
            Console.WriteLine("Struct With Hash : {0}", m4);
            Console.WriteLine("Struct IEquatable: {0}", m5);
            Console.WriteLine("Struct Comparer:   {0}", m6);
            Console.WriteLine("Class Native:      {0}", m7);
            Console.WriteLine("Struct Native:     {0}", m8);
            Console.WriteLine("Struct Native + c: {0}", m9);
        }

        private static void Add<TKey>(Dictionary<TKey, object> dico, Func<int, string, int, Type, TKey> createKey, string[] names, Type[] types)
        {
            var nc = names.Length;
            var tc = types.Length;
            for (var i = 0; i < nc; i++)
            {
                for (var j = 0; j < tc; j++)
                {
                    var key = createKey(i, names[i], j, types[j]);
                    dico[key] = null;
                }
            }
        }

        private static void RunTryGet<TKey>(Dictionary<TKey, object> dico, Func<int, string, int, Type, TKey> createKey, string[] names, Type[] types)
        {
            var nc = names.Length;
            var tc = types.Length;
            for (var i = 0; i < nc; i++)
            {
                for (var j = 0; j < tc; j++)
                {
                    var key = createKey(i, names[i], j, types[j]);
                    object obj;
                    if (!dico.TryGetValue(key, out obj))
                        dico.Add(key, null);
                }
            }
        }

        private static void RunContainsKey<TKey>(Dictionary<TKey, object> dico, Func<int, string, int, Type, TKey> createKey, string[] names, Type[] types)
        {
            var flag = false;

            var nc = names.Length;
            var tc = types.Length;
            for (var i = 0; i < nc; i++)
            {
                for (var j = 0; j < tc; j++)
                {
                    var key = createKey(i, names[i], j, types[j]);
                    flag |= dico.ContainsKey(key);
                }
            }

            if(!flag)
                dico.Clear();
        }

        private class KeyClass
        {
            private readonly string name;
            private readonly Type type;

            public KeyClass(string name, Type type)
            {
                this.name = name;
                this.type = type;
            }

            #region Equality members

            private bool Equals(KeyClass other)
            {
                return string.Equals(name, other.name) && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((KeyClass)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((name != null ? name.GetHashCode() : 0) * 397) ^ (type != null ? type.GetHashCode() : 0);
                }
            }

            public static bool operator ==(KeyClass left, KeyClass right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(KeyClass left, KeyClass right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        private class KeyClassWithHasCodeReadOnly
        {
            private readonly string name;
            private readonly Type type;
            private readonly int hashode;

            public KeyClassWithHasCodeReadOnly(string name, Type type)
            {
                this.name = name;
                this.type = type;

                unchecked
                {
                    hashode = ((name != null ? name.GetHashCode() : 0) * 397) ^ (type != null ? type.GetHashCode() : 0);
                }
            }

            #region Equality members

            private bool Equals(KeyClassWithHasCodeReadOnly other)
            {
                return string.Equals(name, other.name) && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((KeyClassWithHasCodeReadOnly)obj);
            }

            public override int GetHashCode()
            {
                return hashode;
            }

            public static bool operator ==(KeyClassWithHasCodeReadOnly left, KeyClassWithHasCodeReadOnly right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(KeyClassWithHasCodeReadOnly left, KeyClassWithHasCodeReadOnly right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        private struct KeyStruct
        {
            private readonly string name;
            private readonly Type type;

            public KeyStruct(string name, Type type)
            {
                this.name = name;
                this.type = type;
            }

            #region Equality members

            private bool Equals(KeyStruct other)
            {
                return string.Equals(name, other.name) && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is KeyStruct && Equals((KeyStruct)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((name != null ? name.GetHashCode() : 0) * 397) ^ (type != null ? type.GetHashCode() : 0);
                }
            }

            public static bool operator ==(KeyStruct left, KeyStruct right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(KeyStruct left, KeyStruct right)
            {
                return !left.Equals(right);
            }

            #endregion
        }

        private struct KeyStructWithHasCodeReadOnly
        {
            private readonly string name;
            private readonly Type type;
            private readonly int hashode;

            public KeyStructWithHasCodeReadOnly(string name, Type type)
            {
                this.name = name;
                this.type = type;

                unchecked
                {
                    hashode = ((name != null ? name.GetHashCode() : 0) * 397) ^ (type != null ? type.GetHashCode() : 0);
                }
            }

            #region Equality members

            private bool Equals(KeyStructWithHasCodeReadOnly other)
            {
                return string.Equals(name, other.name) && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is KeyStructWithHasCodeReadOnly && Equals((KeyStructWithHasCodeReadOnly)obj);
            }

            public override int GetHashCode()
            {
                return hashode;
            }

            public static bool operator ==(KeyStructWithHasCodeReadOnly left, KeyStructWithHasCodeReadOnly right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(KeyStructWithHasCodeReadOnly left, KeyStructWithHasCodeReadOnly right)
            {
                return !left.Equals(right);
            }
            #endregion

            #region Comparer

            private sealed class NameTYPEEqualityComparer : IEqualityComparer<KeyStructWithHasCodeReadOnly>
            {
                public bool Equals(KeyStructWithHasCodeReadOnly x, KeyStructWithHasCodeReadOnly y)
                {
                    return string.Equals(x.name, y.name) && x.type == y.type;
                }

                public int GetHashCode(KeyStructWithHasCodeReadOnly obj)
                {
                    return obj.hashode;
                }
            }

            private static readonly IEqualityComparer<KeyStructWithHasCodeReadOnly> comparerInstance = new NameTYPEEqualityComparer();

            public static IEqualityComparer<KeyStructWithHasCodeReadOnly> Comparer
            {
                get { return comparerInstance; }
            }

            #endregion
        }

        private struct KeyStructWithIEquatable : IEquatable<KeyStructWithIEquatable>
        {
            private readonly string name;
            private readonly Type type;
            private readonly int hashode;

            public KeyStructWithIEquatable(string name, Type type)
            {
                this.name = name;
                this.type = type;

                unchecked
                {
                    hashode = ((name != null ? name.GetHashCode() : 0) * 397) ^ (type != null ? type.GetHashCode() : 0);
                }
            }

            #region Equality members

            public bool Equals(KeyStructWithIEquatable other)
            {
                return string.Equals(name, other.name) && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is KeyStructWithIEquatable && Equals((KeyStructWithIEquatable)obj);
            }

            public override int GetHashCode()
            {
                return hashode;
            }

            public static bool operator ==(KeyStructWithIEquatable left, KeyStructWithIEquatable right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(KeyStructWithIEquatable left, KeyStructWithIEquatable right)
            {
                return !left.Equals(right);
            }
            #endregion

            #region Comparer

            private sealed class NameTYPEEqualityComparer : IEqualityComparer<KeyStructWithIEquatable>
            {
                public bool Equals(KeyStructWithIEquatable x, KeyStructWithIEquatable y)
                {
                    return string.Equals(x.name, y.name) && x.type == y.type;
                }

                public int GetHashCode(KeyStructWithIEquatable obj)
                {
                    return obj.hashode;
                }
            }

            private static readonly IEqualityComparer<KeyStructWithIEquatable> comparerInstance = new NameTYPEEqualityComparer();

            public static IEqualityComparer<KeyStructWithIEquatable> Comparer
            {
                get { return comparerInstance; }
            }

            #endregion
        }

        private class KeyClassNative
        {
            private readonly int name;
            private readonly int type;
            private readonly int hashode;

            public KeyClassNative(int name, int type)
            {
                this.name = name;
                this.type = type;

                unchecked
                {
                    hashode = (name.GetHashCode() * 397) ^ (type.GetHashCode());
                }
            }

            #region Equality members

            private bool Equals(KeyClassNative other)
            {
                return name == other.name && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((KeyClassNative)obj);
            }

            public override int GetHashCode()
            {
                return hashode;
            }

            public static bool operator ==(KeyClassNative left, KeyClassNative right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(KeyClassNative left, KeyClassNative right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        private struct KeyStructNative : IEquatable<KeyStructNative>
        {
            private readonly int name;
            private readonly int type;
            private readonly int hashode;

            public KeyStructNative(int name, int type)
            {
                this.name = name;
                this.type = type;

                unchecked
                {
                    hashode = (name.GetHashCode() * 397) ^ (type.GetHashCode());
                }
            }

            #region Equality members

            public bool Equals(KeyStructNative other)
            {
                return string.Equals(name, other.name) && type == other.type;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is KeyStructNative && Equals((KeyStructNative)obj);
            }

            public override int GetHashCode()
            {
                return hashode;
            }

            public static bool operator ==(KeyStructNative left, KeyStructNative right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(KeyStructNative left, KeyStructNative right)
            {
                return !left.Equals(right);
            }
            #endregion

            #region Comparer

            private sealed class NameTYPEEqualityComparer : IEqualityComparer<KeyStructNative>
            {
                public bool Equals(KeyStructNative x, KeyStructNative y)
                {
                    return string.Equals(x.name, y.name) && x.type == y.type;
                }

                public int GetHashCode(KeyStructNative obj)
                {
                    return obj.hashode;
                }
            }

            private static readonly IEqualityComparer<KeyStructNative> comparerInstance = new NameTYPEEqualityComparer();

            public static IEqualityComparer<KeyStructNative> Comparer
            {
                get { return comparerInstance; }
            }

            #endregion
        }

    }
}
