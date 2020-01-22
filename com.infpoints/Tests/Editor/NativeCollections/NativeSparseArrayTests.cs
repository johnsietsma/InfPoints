using System;
using System.Collections;
using InfPoints.NativeCollections;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable HeapView.BoxingAllocation

namespace InfPoints.Tests.Editor.NativeCollections
{
    public class NativeSparseArrayTests
    {
        [Test]
        public void Create()
        {
            var array = default(NativeSparseArray<int>);
            Assert.That(array.IsCreated, Is.False);
            array = new NativeSparseArray<int>(10, Allocator.Persistent);
            Assert.That(array.IsCreated, Is.True);
            array.Dispose();
        }

        [Test]
        public void Dispose()
        {
            var array = new NativeSparseArray<int>(1, Allocator.Persistent);
            array.Dispose();
            Assert.That(array.IsCreated, Is.False);
            Assert.That(delegate { array.Dispose(); }, Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.That(() => array.AddValue(1, 2), Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void AddValueGivesCorrectValue()
        {
            const int arrayLength = 2;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Persistent))
            {
                const int sparseIndex = 999;
                const int value = 5;

                Assert.That(arrayLength, Is.EqualTo(array.Capacity));

                array.AddValue(value, sparseIndex);
                Assert.That(array[sparseIndex], Is.EqualTo(value));
                Assert.That(array.ContainsIndex(sparseIndex), Is.True);
                Assert.That(array.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void AddUsingIndexingOperatorGivesCorrectValue()
        {
            var array = new NativeSparseArray<int>(1, Allocator.Persistent);

            array[100] = 200;
            Assert.That(array[100], Is.EqualTo(200));
            Assert.That(array.Length, Is.EqualTo(1));

            array[100] = 300;
            Assert.That(array[100], Is.EqualTo(300));
            Assert.That(array.Length, Is.EqualTo(1));

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.That(array.IsFull, Is.True);
            Assert.That(() => { return array[101] = 201; }, Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(array.Length, Is.EqualTo(1));
#endif
            array.Dispose();
        }

        [Test]
        public void AddMultipleGivesCorrectValues()
        {
            const int arrayLength = 3;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Persistent))
            {
                array.AddValue(1, 100);
                array.AddValue(2, 200);
                array.AddValue(3, 300);

                Assert.That(array[100], Is.EqualTo(1));
                Assert.That(array[200], Is.EqualTo(2));
                Assert.That(array[300], Is.EqualTo(3));
                Assert.That(array.Length, Is.EqualTo(3));
            }
        }


        [Test]
        public void HandlesNonExistentSparseIndexes()
        {
            const int arrayLength = 2;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Persistent))
            {
                const int sparseIndex = 999;

                Assert.That(() =>
                {
                    var v = array[sparseIndex];
                }, Throws.Exception.TypeOf<IndexOutOfRangeException>());
                Assert.That(array.ContainsIndex(sparseIndex), Is.False);
                Assert.That(array.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void AddFails()
        {
            const int arrayLength = 2;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Persistent))
            {
                array.AddValue(1, 1);
                Assert.That(array.Length, Is.EqualTo(1));
                array.AddValue(2, 2);
                Assert.That(array.Length, Is.EqualTo(2));

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.That(() => array.AddValue(2, 2),
                    Throws.Exception.TypeOf<ArgumentOutOfRangeException>()); // Already exists
                Assert.That(array.Length, Is.EqualTo(2));
                Assert.That(() => array.AddValue(3, 3),
                    Throws.Exception.TypeOf<ArgumentOutOfRangeException>()); // Array is full
                Assert.That(array.Length, Is.EqualTo(2));
                Assert.That(array.IsFull, Is.True);
#endif
            }
        }

        [Test]
        public void SetValue()
        {
            const int arrayLength = 50;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Persistent))
            {
                const int sparseIndex = 12345;
                const int value1 = 555;
                const int value2 = 666;

                Assert.That(() => array.SetValue(value1, sparseIndex),
                    Throws.Exception.TypeOf<IndexOutOfRangeException>());
                array.AddValue(value1, sparseIndex);
                array.SetValue(value2, sparseIndex);
                Assert.That(array[sparseIndex], Is.EqualTo(value2));
            }
        }

        [Test]
        public void RemoveAt()
        {
            const int arrayLength = 50;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Persistent))
            {
                array.AddValue(1, 100);
                array.AddValue(2, 200);
                array.AddValue(3, 300);

                array.RemoveAt(200);
                Assert.That(array.Length, Is.EqualTo(2));
                Assert.That(array.ContainsIndex(200), Is.False);
            }
        }

        [Test]
        public void Enumerator()
        {
            using (var array = new NativeSparseArray<int>(3, Allocator.Persistent))
            {
                array.AddValue(1, 100);
                array.AddValue(2, 200);
                array.AddValue(3, 300);

                IEnumerator e = array.GetEnumerator();
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