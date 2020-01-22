using System;
using System.Collections;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable HeapView.BoxingAllocation

namespace InfPoints.Tests.Editor.NativeCollections
{
    public class NativeSparseListTests
    {
        [Test]
        public void Create()
        {
            var list = default(NativeSparseList<int>);
            Assert.That(list.IsCreated, Is.False);
            list = new NativeSparseList<int>(10, Allocator.Persistent);
            Assert.That(list.IsCreated, Is.True);
            list.Dispose();
        }

        [Test]
        public void Dispose()
        {
            var list = new NativeSparseList<int>(1, Allocator.Persistent);
            list.Dispose();
            Assert.That(list.IsCreated, Is.False);
            Assert.That(delegate { list.Dispose(); }, Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.That(() => list.AddValue(1, 2), Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void AddValueGivesCorrectValue()
        {
            const int initialCapacity = 2;
            using (var list = new NativeSparseList<int>(initialCapacity, Allocator.Persistent))
            {
                const int sparseIndex = 999;
                const int value = 5;

                list.AddValue(value, sparseIndex);
                Assert.That(list[sparseIndex], Is.EqualTo(value));
                Assert.That(list.ContainsIndex(sparseIndex), Is.True);
                Assert.That(list.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void AddUsingIndexingOperatorGivesCorrectValue()
        {
            var list = new NativeSparseList<int>(1, Allocator.Persistent);

            list[100] = 200;
            Assert.That(list[100], Is.EqualTo(200));
            Assert.That(list.Length, Is.EqualTo(1));

            list[100] = 300;
            Assert.That(list[100], Is.EqualTo(300));
            Assert.That(list.Length, Is.EqualTo(1));

            list.Dispose();
        }

        [Test]
        public void AddMultipleGivesCorrectValues()
        {
            const int listLength = 3;
            using (var list = new NativeSparseList<int>(listLength, Allocator.Persistent))
            {
                list.AddValue(1, 100);
                list.AddValue(2, 200);
                list.AddValue(3, 300);

                Assert.That(list[100], Is.EqualTo(1));
                Assert.That(list[200], Is.EqualTo(2));
                Assert.That(list[300], Is.EqualTo(3));
                Assert.That(list.Length, Is.EqualTo(3));
            }
        }


        [Test]
        public void HandlesNonExistentSparseIndexes()
        {
            const int listLength = 2;
            using (var list = new NativeSparseList<int>(listLength, Allocator.Persistent))
            {
                const int sparseIndex = 999;

                Assert.That(() =>
                {
                    var v = list[sparseIndex];
                }, Throws.Exception.TypeOf<IndexOutOfRangeException>());
                Assert.That(list.ContainsIndex(sparseIndex), Is.False);
                Assert.That(list.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void AddFails()
        {
            const int listLength = 2;
            using (var list = new NativeSparseList<int>(listLength, Allocator.Persistent))
            {
                list.AddValue(1, 1);
                Assert.That(list.Length, Is.EqualTo(1));
                list.AddValue(2, 2);
                Assert.That(list.Length, Is.EqualTo(2));

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.That(() => list.AddValue(2, 2),
                    Throws.Exception.TypeOf<ArgumentOutOfRangeException>()); // Already exists
                Assert.That(list.Length, Is.EqualTo(2));
#endif
            }
        }

        [Test]
        public void SetValue()
        {
            const int listLength = 50;
            using (var list = new NativeSparseList<int>(listLength, Allocator.Persistent))
            {
                const int sparseIndex = 12345;
                const int value1 = 555;
                const int value2 = 666;

                Assert.That(() => list.SetValue(value1, sparseIndex),
                    Throws.Exception.TypeOf<IndexOutOfRangeException>());
                list.AddValue(value1, sparseIndex);
                list.SetValue(value2, sparseIndex);
                Assert.That(list[sparseIndex], Is.EqualTo(value2));
            }
        }

        [Test]
        public void RemoveAt()
        {
            const int listLength = 50;
            using (var list = new NativeSparseList<int>(listLength, Allocator.Persistent))
            {
                list.AddValue(1, 100);
                list.AddValue(2, 200);
                list.AddValue(3, 300);

                list.RemoveAt(200);
                Assert.That(list.Length, Is.EqualTo(2));
                Assert.That(list.ContainsIndex(200), Is.False);
            }
        }

        [Test]
        public void Enumerator()
        {
            using (var list = new NativeSparseList<int>(3, Allocator.Persistent))
            {
                list.AddValue(1, 100);
                list.AddValue(2, 200);
                list.AddValue(3, 300);

                IEnumerator e = list.GetEnumerator();
                Assert.That(e.MoveNext, Is.True);
                Assert.That(e.Current, Is.EqualTo(1));
                Assert.That(e.MoveNext, Is.True);
                Assert.That(e.Current, Is.EqualTo(2));
                Assert.That(e.MoveNext, Is.True);
                Assert.That(e.Current, Is.EqualTo(3));
                Assert.That(e.MoveNext, Is.False);
            }
        }
    }
}