using System;
using InfPoints.NativeCollections;
using NUnit.Framework;
using Unity.Collections;

namespace InfPoints.Tests.Editor.NativeCollections
{
    public class NativeCollectionExtensionsTests
    {
        [Test]
        public void Swap()
        {
            using (var data = new NativeArray<int>(new int[] {1, 2, 3, 4}, Allocator.Persistent))
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
        public void InsertIntoArray()
        {
            using (var data = new NativeArray<int>(new int[] {1, 2, 3, 4}, Allocator.Persistent))
            {
                data.Insert(1, 5); // 1,5,2,3
                Assert.AreEqual(5, data[1]);
                Assert.AreEqual(2, data[2]);

                // Test writing to the last element
                data.Insert(3, 6); // 1,5,2,6
                Assert.AreEqual(6, data[3]);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.Throws<ArgumentOutOfRangeException>(() => data.Insert(4, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => data.Insert(-1, 0));
#endif
            }
        }

        [Test]
        public void InsertIntoNativeList()
        {
            using (var list = new NativeList<int>(1, Allocator.Persistent))
            {
                var capacity = list.Capacity;
                for (int i = 0; i < capacity; i++)
                {
                    list.Add(default);
                    list.Insert(0, 100);
                    Assert.That(list.Length, Is.EqualTo(i+1));
                    Assert.That(list.Capacity, Is.EqualTo(capacity));
                }

                list.Add(default);
                list.Insert(0,200);
                Assert.That(list.Length, Is.EqualTo(capacity+1));
                Assert.That(list.Capacity, Is.EqualTo(capacity*2));
                Assert.That(list[0], Is.EqualTo(200));
            }
        }

        [Test]
        public void InsertAscending()
        {
            using (var data = new NativeArray<int>(4, Allocator.Persistent))
            {
                data.Insert(0,100);
                data.Insert(1,200);
                data.Insert(2,300);
                
                Assert.That(data[0], Is.EqualTo(100));
                Assert.That(data[1], Is.EqualTo(200));
                Assert.That(data[2], Is.EqualTo(300));
            }
        }

        [Test]
        public void RemoveAtFromNativeArray()
        {
            using (var data = new NativeArray<long>(new long[] {1, 2, 3, 4}, Allocator.Persistent))
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
        public void RemoveAtFromNativeList()
        {
            using (var list = new NativeList<int>(5, Allocator.Persistent))
            {
                list.Add(1);
                list.Add(2);
                list.Add(3);
                list.RemoveAtSwapBack(1);
                Assert.That(list[0], Is.EqualTo(1));
                Assert.That(list[1], Is.EqualTo(3));
                Assert.That(list.Length, Is.EqualTo(2));
            }
        }

        [Test]
        public void BinarySearch()
        {
            using (var data = new NativeArray<int>(new int[] {1, 2, 4, 5}, Allocator.Persistent))
            {
                Assert.AreEqual(1, data.BinarySearch(2));
                Assert.AreEqual(2, ~data.BinarySearch(3));
                Assert.AreEqual(4, ~data.BinarySearch(6));
                Assert.AreEqual(0, ~data.BinarySearch(0));
            }            
        }

        [Test]
        public void BinarySearchInBounds()
        {
            using (var data = new NativeArray<int>( new []{100,200,300}, Allocator.Persistent))
            {
                Assert.That(data.BinarySearch(100,0,1), Is.EqualTo(0));
                Assert.That(data.BinarySearch(200,0,2), Is.EqualTo(1));
                Assert.That(data.BinarySearch(300,0,3), Is.EqualTo(2));
                Assert.That(data.BinarySearch(200,0,1), Is.EqualTo(~1));
                Assert.That(data.BinarySearch(300,0,1), Is.EqualTo(~1));
            }
        }
        
    }
}