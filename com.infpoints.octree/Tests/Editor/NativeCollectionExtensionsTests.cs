using System;
using NUnit.Framework;
using Unity.Collections;

namespace InfPoints.Octree.Tests.Editor
{
    public class NativeCollectionExtensionsTests
    {
        [Test]
        public void Swap()
        {
            using (var data = new NativeArray<int>(new int[] {1, 2, 3, 4}, Allocator.Temp))
            {
                Assert.AreEqual(1, data[0]);
                Assert.AreEqual(4, data[3]);
                data.Swap(0, 3);
                Assert.AreEqual(4, data[0]);
                Assert.AreEqual(1, data[3]);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.Throws<ArgumentException>(() => data.Swap(0, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => data.Swap(-1, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => data.Swap(4, 0));
#endif
            }
        }

        [Test]
        public void Insert()
        {
            using (var data = new NativeArray<int>(new int[] {1, 2, 3, 4}, Allocator.Temp))
            {
                data.Insert(1, 5);
                Assert.AreEqual(5, data[1]);
                Assert.AreEqual(2, data[2]);

                // Test writing to the last element
                data.Insert(3, 6);
                Assert.AreEqual(6, data[3]);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.Throws<ArgumentOutOfRangeException>(() => data.Insert(5, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => data.Insert(-1, 0));
#endif
            }
        }
        
        [Test]
        public void RemoveAt()
        {
            using (var data = new NativeArray<long>(new long[] {1, 2, 3, 4}, Allocator.Temp))
            {
                data.RemoveAt(1);
                Assert.AreEqual(3, data[1]);
                Assert.AreEqual(default(long), data[3]);

                
                // Test removing the last element
                data.RemoveAt(3);
                Assert.AreEqual(default(long), data[3]);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.Throws<ArgumentOutOfRangeException>(() => data.RemoveAt(5));
                Assert.Throws<ArgumentOutOfRangeException>(() => data.RemoveAt(-1));
#endif
            }
        }

        [Test]
        public void BinarySearch()
        {
            using (var data = new NativeArray<int>(new int[] {5, 4, 2, 1}, Allocator.Temp))
            {
                Assert.AreEqual(2, data.BinarySearch(2));
                Assert.AreEqual(2, ~data.BinarySearch(3));
                Assert.AreEqual(0, ~data.BinarySearch(6));
                Assert.AreEqual(4, ~data.BinarySearch(0));
            }            
        }
        
    }
}