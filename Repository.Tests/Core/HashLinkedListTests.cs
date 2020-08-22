using Observable.Repository.Collections;
using Observable.Repository.Core;
using Xunit;

namespace Observable.Repository.Tests.Core
{
    public class HashLinkedListTests
    {
        private readonly Pool<LinkedNode<int, string>> _pool = new Pool<LinkedNode<int,string>>(() => new LinkedNode<int, string>());

        [Fact]
        public void AddRemoveAndClearTest()
        {
            var list = new HashLinkedList<int, string>(_pool)
            {
                {1, "Test1"},
                {2, "Test2"},
                {3, "Test3"},
                {4, "Test4"},
                {5, "Test5"}
            };

            Assert.Equal(5, list.Count);

            var idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx, pair._key);
                Assert.Equal("Test"+idx, pair._value);
            }

            Assert.Equal(5, idx);

            list.Remove(1);
            Assert.Equal(4, list.Count);

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx + 1, pair._key);
                Assert.Equal("Test" + (idx + 1), pair._value);
            }
            Assert.Equal(4, idx);

            list.Remove(5);
            Assert.Equal(3, list.Count);

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx + 1, pair._key);
                Assert.Equal("Test" + (idx + 1), pair._value);
            }
            Assert.Equal(3, idx);

            idx = 0;
            list.Clear((k,v) => idx++);
            Assert.Equal(3, idx);

            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void AddAndUpdateTest()
        {
            var values = new[]
            {
                "Test1",
                "Test2",
                "Test3",
                "Test4",
                "Test5"
            };

            var list = new HashLinkedList<int, string>(_pool)
            {
                {1, values[0]},
                {2, values[1]},
                {3, values[2]},
                {4, values[3]},
                {5, values[4]}
            };

            Assert.Equal(5, list.Count);

            var idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx, pair._key);
                Assert.Equal(values[idx - 1], pair._value);
            }

            Assert.Equal(5, idx);

            values[0] = "Update1";
            list[1] = values[0];

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx, pair._key);
                Assert.Equal(values[idx - 1], pair._value);
            }

            Assert.Equal(5, idx);

            values[4] = "Update5";
            list[5] = values[4];

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx, pair._key);
                Assert.Equal(values[idx - 1], pair._value);
            }

            Assert.Equal(5, idx);

            values[2] = "Update3";
            list[3] = values[2];

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx, pair._key);
                Assert.Equal(values[idx - 1], pair._value);
            }

            Assert.Equal(5, idx);
        }

        [Fact]
        public void FlushTest()
        {
            var list = new HashLinkedList<int, string>(_pool)
            {
                {1, "Test1"},
                {2, "Test2"},
                {3, "Test3"},
                {4, "Test4"},
                {5, "Test5"}
            };

            var items = list.Flush();
            Assert.Equal(5, items.Length);
            Assert.Equal(0, list.Count);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.Equal(i + 1, items[i].Key);
                Assert.Equal("Test" + (i + 1), items[i].Value);
            }
        }

        [Fact]
        public void FlushValuesTest()
        {
            var list = new HashLinkedList<int, string>(_pool)
            {
                {1, "Test1"},
                {2, "Test2"},
                {3, "Test3"},
                {4, "Test4"},
                {5, "Test5"}
            };

            var items = list.FlushValues();
            Assert.Equal(5, items.Length);
            Assert.Equal(0, list.Count);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.Equal("Test" + (i + 1), items[i]);
            }
        }

        [Fact]
        public void MakeCopyTests()
        {
            var list = new HashLinkedList<int, string>(_pool)
            {
                {1, "Test1"},
                {2, "Test2"},
                {3, "Test3"},
                {4, "Test4"},
                {5, "Test5"}
            };

            var copy = list.MakeCopy();
            Assert.Equal(5, copy.Length);
            Assert.Equal(5, list.Count);

            var idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.Equal(idx, pair._key);
                Assert.Equal("Test" + idx, pair._value);
                Assert.Equal(idx, copy[idx - 1].Key);
                Assert.Equal("Test"+idx, copy[idx - 1].Value);
            }

            Assert.Equal(5, idx);
        }

        [Fact]
        public void TryGetValueTest()
        {
            var list = new HashLinkedList<int, string>(_pool)
            {
                {1, "Test1"},
                {2, "Test2"},
                {3, "Test3"},
                {4, "Test4"},
                {5, "Test5"}
            };

            Assert.False(list.TryGetValue(0, out var value));
            Assert.Null(value);

            Assert.True(list.TryGetValue(1, out value));
            Assert.Equal("Test1", value);
            Assert.True(list.TryGetValue(2, out value));
            Assert.Equal("Test2", value);
            Assert.True(list.TryGetValue(3, out value));
            Assert.Equal("Test3", value);
            Assert.True(list.TryGetValue(4, out value));
            Assert.Equal("Test4", value);
            Assert.True(list.TryGetValue(5, out value));
            Assert.Equal("Test5", value);

            Assert.False(list.TryGetValue(6, out value));
            Assert.Null(value);
        }

        [Fact]
        public void ContainsKeyTest()
        {
            var list = new HashLinkedList<int, string>(_pool)
            {
                {1, "Test1"},
                {2, "Test2"},
                {3, "Test3"},
                {4, "Test4"},
                {5, "Test5"}
            };

            Assert.False(list.ContainsKey(0));
            Assert.True(list.ContainsKey(1));
            Assert.True(list.ContainsKey(2));
            Assert.True(list.ContainsKey(3));
            Assert.True(list.ContainsKey(4));
            Assert.True(list.ContainsKey(5));
            Assert.False(list.ContainsKey(6));
        }
    }
}
