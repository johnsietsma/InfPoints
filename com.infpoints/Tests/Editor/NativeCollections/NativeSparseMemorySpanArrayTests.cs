using InfPoints.NativeCollections;
using NUnit.Framework;
using Unity.Collections;

namespace InfPoints.Tests.Editor.NativeCollections
{
    public class NativeSparseMemorySpanArrayTests
    {
        [Test]
        public void CreationGivesCorrectValues()
        {
            int capacity = 5;
            int spanCapacity = 10;
            var array = new SparseMemorySpanArray<int>(capacity, spanCapacity, Allocator.Persistent);
            Assert.That(array.IsCreated, Is.True);
            Assert.That(array.Length, Is.EqualTo(0));
            Assert.That(array.Capacity, Is.EqualTo(capacity));
            Assert.That(array.SpanCapacity, Is.EqualTo(spanCapacity));
            array.Dispose();
            Assert.That(array.IsCreated, Is.False);
        }

        [Test]
        public void AddingDataGivesCorrectResult()
        {
            int capacity = 5;
            int spanCapacity = 10;
            int sparseIndex = 12345;
            int[] dataArray = {1, 2, 3, 4, 5};
            using (var array = new SparseMemorySpanArray<int>(capacity, spanCapacity, Allocator.Persistent))
            using( var data = new NativeArray<int>(dataArray, Allocator.Persistent))
            {
                var span = array.AddSpan(sparseIndex);
                Assert.That(array.Length, Is.EqualTo(1));
                Assert.That(span.DataIndex, Is.EqualTo(0));
                
                array.AddData(ref span, data);
                var returnedData = array.AsArray(span);
                Assert.That(returnedData.Length, Is.EqualTo(data.Length));

                for (int index = 0; index < array.Length; index++)
                {
                    Assert.That(data[index], Is.EqualTo(returnedData[index]));
                }

            }
        }

        [Test]
        public void DoulbeDisposeThrowsException()
        {
            
        }

        [Test]
        public void AddingMoreSpansThenCapacityThrwosException()
        {
            
        }

        [Test]
        public void AddingMoreDataThenCapacityThrowsException()
        {
            
        }
    }
}