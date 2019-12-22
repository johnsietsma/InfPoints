using System;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable HeapView.BoxingAllocation

namespace InfPoints.Octree.Tests.Editor
{
    public class NativeSparseArrayTests
    {
        [Test]
        public void Add()
        {
            const int arrayLength = 2;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Temp))
            {
                Assert.That(arrayLength, Is.EqualTo(array.Length));
                const int sparseIndex = 999;
                const int value = 5;

                // Doesn't exist yet
                Assert.That(() =>
                {
                    var v = array[sparseIndex];
                }, Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
                Assert.That(array.Contains(sparseIndex), Is.False);
                Assert.That(array.UsedElementCount, Is.EqualTo(0));

                bool ret = array.AddValue(value, sparseIndex);
                Assert.That(ret, Is.True);
                Assert.That(array[sparseIndex], Is.EqualTo(value));
                Assert.That(array.Contains(sparseIndex), Is.True);
                Assert.That(array.UsedElementCount, Is.EqualTo(1));

                Assert.That(array.AddValue(1234, 1234), Is.True);
                Assert.That(array.AddValue(1234, 1234), Is.False); // Already exists
                Assert.That(array.AddValue(5678, 5678), Is.False); // Array is full
                Assert.That(array.IsFull, Is.True);
            }
        }

        [Test]
        public void SetValue()
        {
            const int arrayLength = 50;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Temp))
            {
                const int sparseIndex = 12345;
                const int value1 = 555;
                const int value2 = 666;

                Assert.That(() => array.SetValue(value1, sparseIndex), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
                array.AddValue(value1, sparseIndex);
                array.SetValue(value2, sparseIndex);
                Assert.That(array[sparseIndex], Is.EqualTo(value2));
            }
        }

        [Test]
        public void RemoveAt()
        {
            const int arrayLength = 50;
            using (var array = new NativeSparseArray<int>(arrayLength, Allocator.Temp))
            {
                array.AddValue(1, 100);
                array.AddValue(2, 200);
                array.AddValue(3, 300);

                array.RemoveAt(200);
                Assert.That(array.UsedElementCount, Is.EqualTo(2));
                Assert.That(array.Contains(200), Is.False);
            }
        }
    }
}