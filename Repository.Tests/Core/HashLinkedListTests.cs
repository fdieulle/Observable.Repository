using NUnit.Framework;
using Observable.Repository.Collections;
using Observable.Repository.Core;

namespace Observable.Repository.Tests.Core
{
    [TestFixture]
    public class HashLinkedListTests
    {
        private readonly Pool<LinkedNode<int, string>> _pool = new Pool<LinkedNode<int,string>>(() => new LinkedNode<int, string>());

        [Test]
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

            Assert.AreEqual(5, list.Count);

            var idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx, pair._key);
                Assert.AreEqual("Test"+idx, pair._value);
            }

            Assert.AreEqual(5, idx);

            list.Remove(1);
            Assert.AreEqual(4, list.Count);

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx + 1, pair._key);
                Assert.AreEqual("Test" + (idx + 1), pair._value);
            }
            Assert.AreEqual(4, idx);

            list.Remove(5);
            Assert.AreEqual(3, list.Count);

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx + 1, pair._key);
                Assert.AreEqual("Test" + (idx + 1), pair._value);
            }
            Assert.AreEqual(3, idx);

            idx = 0;
            list.Clear((k,v) => idx++);
            Assert.AreEqual(3, idx);

            Assert.AreEqual(0, list.Count);
        }

        [Test]
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

            Assert.AreEqual(5, list.Count);

            var idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx, pair._key);
                Assert.AreEqual(values[idx - 1], pair._value);
            }

            Assert.AreEqual(5, idx);

            values[0] = "Update1";
            list[1] = values[0];

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx, pair._key);
                Assert.AreEqual(values[idx - 1], pair._value);
            }

            Assert.AreEqual(5, idx);

            values[4] = "Update5";
            list[5] = values[4];

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx, pair._key);
                Assert.AreEqual(values[idx - 1], pair._value);
            }

            Assert.AreEqual(5, idx);

            values[2] = "Update3";
            list[3] = values[2];

            idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx, pair._key);
                Assert.AreEqual(values[idx - 1], pair._value);
            }

            Assert.AreEqual(5, idx);
        }

        [Test]
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
            Assert.AreEqual(5, items.Length);
            Assert.AreEqual(0, list.Count);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(i + 1, items[i].Key);
                Assert.AreEqual("Test" + (i + 1), items[i].Value);
            }
        }

        [Test]
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
            Assert.AreEqual(5, items.Length);
            Assert.AreEqual(0, list.Count);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.AreEqual("Test" + (i + 1), items[i]);
            }
        }

        [Test]
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
            Assert.AreEqual(5, copy.Length);
            Assert.AreEqual(5, list.Count);

            var idx = 0;
            foreach (var pair in list)
            {
                ++idx;
                Assert.AreEqual(idx, pair._key);
                Assert.AreEqual("Test" + idx, pair._value);
                Assert.AreEqual(idx, copy[idx - 1].Key);
                Assert.AreEqual("Test"+idx, copy[idx - 1].Value);
            }

            Assert.AreEqual(5, idx);
        }

        [Test]
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

            Assert.IsFalse(list.TryGetValue(0, out var value));
            Assert.AreEqual(null, value);

            Assert.IsTrue(list.TryGetValue(1, out value));
            Assert.AreEqual("Test1", value);
            Assert.IsTrue(list.TryGetValue(2, out value));
            Assert.AreEqual("Test2", value);
            Assert.IsTrue(list.TryGetValue(3, out value));
            Assert.AreEqual("Test3", value);
            Assert.IsTrue(list.TryGetValue(4, out value));
            Assert.AreEqual("Test4", value);
            Assert.IsTrue(list.TryGetValue(5, out value));
            Assert.AreEqual("Test5", value);

            Assert.IsFalse(list.TryGetValue(6, out value));
            Assert.AreEqual(null, value);
        }

        [Test]
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

            Assert.IsFalse(list.ContainsKey(0));
            Assert.IsTrue(list.ContainsKey(1));
            Assert.IsTrue(list.ContainsKey(2));
            Assert.IsTrue(list.ContainsKey(3));
            Assert.IsTrue(list.ContainsKey(4));
            Assert.IsTrue(list.ContainsKey(5));
            Assert.IsFalse(list.ContainsKey(6));
        }
    }
}
