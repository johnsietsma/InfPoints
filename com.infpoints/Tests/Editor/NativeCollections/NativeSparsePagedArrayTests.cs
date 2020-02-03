using InfPoints.NativeCollections;
using NUnit.Framework;
using Unity.Collections;

namespace InfPoints.Tests.Editor.NativeCollections
{
    public class NativeSparsePagedArrayTests
    {
        [Test]
        public void AllocationAtCapacityIsFull()
        {
            var allocationFull = new PageAllocation()
            {
                Capacity = 1,
                Length = 1,
                PageIndex = 0,
                StartIndex = 0
            };
            
            var allocationNotFull = new PageAllocation()
            {
                Capacity = 2,
                Length = 1,
                PageIndex = 0,
                StartIndex = 0
            };
            
            Assert.That(allocationFull.IsFull, Is.True);
            Assert.That(allocationNotFull.IsFull, Is.False);
        }
        
        
        [Test]
        public void CreationGivesCorrectValues()
        {
            int allocationSize = 10;
            int pageSize = 10;
            int maximumPageCount = 1;
            var array = new NativeSparsePagedArray<int>(allocationSize, pageSize, maximumPageCount,
                Allocator.Persistent);
            Assert.That(array.IsCreated, Is.True);
            Assert.That(array.Length, Is.EqualTo(0));
            Assert.That(array.MaximumPageCount, Is.EqualTo(maximumPageCount));
            Assert.That(array.PageSize, Is.EqualTo(pageSize));
            array.Dispose();
            Assert.That(array.IsCreated, Is.False);
        }

        [Test]
        public void AddingIndexGivesCorrectResult()
        {
            int allocationSize = 10;
            int pageSize = 10;
            int maximumPageCount = 1;
            ulong sparseIndex = 12345;
            using (var array =
                new NativeSparsePagedArray<int>(allocationSize, pageSize, maximumPageCount, Allocator.Persistent))
            {
                Assert.That(array.Length, Is.EqualTo(0));
                Assert.That(array.MaximumPageCount, Is.EqualTo(1));
                Assert.That(array.ContainsIndex(sparseIndex), Is.False);
                array.AddIndex(sparseIndex);
                Assert.That(array.ContainsIndex(sparseIndex), Is.True);
                Assert.That(array.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void AddingDataGivesCorrectResult()
        {
            int allocationSize = 10;
            int pageSize = 10;
            int maximumPageCount = 1;
            ulong sparseIndex = 12345;
            int[] dataArray = {1, 2, 3, 4, 5};
            using (var array =
                new NativeSparsePagedArray<int>(allocationSize, pageSize, maximumPageCount, Allocator.Persistent))
            using (var data = new NativeArray<int>(dataArray, Allocator.Persistent))
            {
                Assert.That(array.Length, Is.EqualTo(0));
                Assert.That(array.MaximumPageCount, Is.EqualTo(1));
                array.AddIndex(sparseIndex);
                array.AddRange(sparseIndex, data);
                Assert.That(array.Length, Is.EqualTo(1));
                var returnedData = array.ToArray(sparseIndex);
                Assert.That(returnedData.Length, Is.EqualTo(data.Length));
                Assert.That(returnedData.ToArray(), Is.EqualTo(dataArray));
            }
        }

        [Test]
        public void DoubleDisposeThrowsException()
        {
            var array = new NativeSparsePagedArray<int>(1, 1, 1, Allocator.Persistent);
            array.Dispose();
            Assert.That(() => array.Dispose(), Throws.InvalidOperationException);
        }

        [Test]
        public void AddingDataTwiceToTheSamePageGivesCorrectResult()
        {
            int allocationSize = 10;
            int pageSize = 10;
            int maximumPageCount = 1;
            ulong sparseIndex = 12345;
            int[] dataArray = {1, 2, 3, 4, 5};
            using (var array =
                new NativeSparsePagedArray<int>(allocationSize, pageSize, maximumPageCount, Allocator.Persistent))
            using (var data = new NativeArray<int>(dataArray, Allocator.Persistent))
            {
                array.AddIndex(sparseIndex);
                array.AddRange(sparseIndex, data);
                array.AddRange(sparseIndex, data);
                Assert.That(array.Length, Is.EqualTo(1));
                Assert.That(array.PageCount, Is.EqualTo(1));
                var returnedData = array.ToArray(sparseIndex);
                Assert.That(returnedData.Length, Is.EqualTo(data.Length * 2));

                for (int index = 0; index < dataArray.Length * 2; index++)
                {
                    int index2 = index % dataArray.Length;
                    Assert.That(data[index2], Is.EqualTo(returnedData[index2]));
                }
            }
        }

        [Test]
        public void AddingDataTwiceToTheDifferentPagesGivesCorrectResult()
        {
            int allocationSize = 5;
            int pageSize = 5;
            int maximumPageCount = 2;
            ulong sparseIndex1 = 12345;
            ulong sparseIndex2 = 54321;
            int[] dataArray = {1, 2, 3, 4, 5};
            using (var array =
                new NativeSparsePagedArray<int>(allocationSize, pageSize, maximumPageCount, Allocator.Persistent))
            using (var data = new NativeArray<int>(dataArray, Allocator.Persistent))
            {
                array.AddIndex(sparseIndex1);
                array.AddRange(sparseIndex1, data);
                Assert.That(array.Length, Is.EqualTo(1));
                array.AddIndex(sparseIndex2);
                array.AddRange(sparseIndex2, data);
                Assert.That(array.PageCount, Is.EqualTo(2));
                var returnedData1 = array.ToArray(sparseIndex1);
                var returnedData2 = array.ToArray(sparseIndex2);
                Assert.That(returnedData1.ToArray(), Is.EqualTo(returnedData2.ToArray()));
            }
        }

        
        [Test]
        public void AddingNullDataThrowsException()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            using (var array = new NativeSparsePagedArray<int>(1, 1, 1, Allocator.Persistent))
            {
                array.AddIndex(1);
                Assert.That(() => array.AddRange(1, default), Throws.ArgumentNullException);
            }
#endif
        }

        [Test]
        public void AddingDataWithoutIndexThrowsException()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            using (var array = new NativeSparsePagedArray<int>(1, 1, 1, Allocator.Persistent))
            using (var data = new NativeArray<int>(1, Allocator.Persistent))
            {
                Assert.That(() => array.AddRange(1, data), Throws.InvalidOperationException);
            }
#endif
        }

        [Test]
        public void AddingMoreDataThenCapacityThrowsException()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            using (var array = new NativeSparsePagedArray<int>(1, 1, 1, Allocator.Persistent))
            using (var data = new NativeArray<int>(2, Allocator.Persistent))
            {
                Assert.That(() => array.AddRange(1, data), Throws.InvalidOperationException);
            }
#endif
        }
    }
}