using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    public static unsafe class NativeCollectionExtensions
    {
        /// <summary>
        /// Exchange the values in index positions `i` and `j`.
        /// </summary>
        /// <param name="data">The container in which the data will be swapped</param>
        /// <param name="i">The first index to swap</param>
        /// <param name="j">The second index to swap</param>
        /// <typeparam name="T">An unmanaged type</typeparam>
        /// <exception cref="ArgumentException">If the indices are the same</exception>
        /// <exception cref="ArgumentOutOfRangeException">If any of the indices are out of bounds of the container</exception>
        public static void Swap<T>(this NativeArray<T> data, int i, int j)
            where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (i < 0 || i >= data.Length) throw new ArgumentOutOfRangeException(nameof(i));
            if (j < 0 || j >= data.Length) throw new ArgumentOutOfRangeException(nameof(j));
#endif
            NativeCollectionUnsafe.Swap<T>(data.GetUnsafePtr(), i, j);
        }

        /// <summary>
        /// See <see cref="Swap{T}(NativeArray{t},int,int)"/>
        /// </summary>
        public static void Swap<T>(this NativeSlice<T> data, int i, int j)
            where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (i < 0 || i >= data.Length) throw new ArgumentOutOfRangeException(nameof(i));
            if (j < 0 || j >= data.Length) throw new ArgumentOutOfRangeException(nameof(j));
#endif
            NativeCollectionUnsafe.Swap<T>(data.GetUnsafePtr(), i, j);
        }

        /// <summary>
        /// Insert an element into an array. Does not grow the size of the array, the last element will be lost.
        /// </summary>
        /// <param name="data">The container in which the data will be inserted</param>
        /// <param name="index">The index in which to insert</param>
        /// <param name="value">The value to insert</param>
        /// <typeparam name="T">An unmanaged type</typeparam>
        public static void Insert<T>(this NativeArray<T> data, int index, T value)
            where T : unmanaged
        {
            NativeCollectionUnsafe.Insert(data.GetUnsafePtr(), index, data.Length, value);
        }

        public static void Insert<T>(this NativeSlice<T> data, int index, T value)
            where T : unmanaged
        {
            NativeCollectionUnsafe.Insert(data.GetUnsafePtr(), index, data.Length, value);
        }

        public static void Insert<T>(this NativeList<T> data, int index, T value)
            where T : unmanaged
        {
            NativeCollectionUnsafe.Insert(data.GetUnsafePtr(), index, data.Length, value);
        }

        /// <summary>
        /// Remove an element from the collection and keep the same order.
        /// Does not change the size of collection. The lst element of the container will be set to default.
        /// </summary>
        /// <param name="data">The container to remove from</param>
        /// <param name="index">The index to remove</param>
        /// <typeparam name="T">An unmanaged type</typeparam>
        public static void RemoveAt<T>(this NativeArray<T> data, int index)
            where T : unmanaged
        {
            NativeCollectionUnsafe.RemoveAt<T>(data.GetUnsafePtr(), index, data.Length);
        }

        public static void RemoveAt<T>(this NativeSlice<T> data, int index)
            where T : unmanaged
        {
            NativeCollectionUnsafe.RemoveAt<T>(data.GetUnsafePtr(), index, data.Length);
        }
        
        /// <summary>
        /// Searches the entire array for a specific value. Uses IComparable to compare elements.
        /// The container elements must be sorted.
        /// </summary>
        /// <param name="data">A sorted collection through which to search</param>
        /// <param name="key">The value to find</param>
        /// <typeparam name="T">An unmanaged type</typeparam>
        /// <returns>The 0 based index of the element or a negative number that is th bitwise complement of the index of
        /// the next largest element. This can be used to find an insertion point for elements not in the array.</returns>
        public static int BinarySearch<T>(this NativeArray<T> data, T key)
            where T : unmanaged
            , IComparable<T>
        {
            return data.BinarySearch(key, 0, data.Length);
        }

        public static int BinarySearch<T>(this NativeArray<T> data, T key, int startIndex, int count)
            where T : unmanaged
            , IComparable<T>
        {
            CheckInRangeOrThrow(startIndex, count, data.Length);
            return NativeCollectionUnsafe.BinarySearch(data.GetUnsafeReadOnlyPtr(), key, startIndex, count);
        }

        public static int BinarySearch<T>(this NativeSlice<T> data, T key)
            where T : unmanaged
            , IComparable<T>
        {
            return data.BinarySearch(key, 0, data.Length);
        }

        public static int BinarySearch<T>(this NativeSlice<T> data, T key, int startIndex, int count)
            where T : unmanaged
            , IComparable<T>
        {
            CheckInRangeOrThrow(startIndex, count, data.Length);
            return NativeCollectionUnsafe.BinarySearch(data.GetUnsafeReadOnlyPtr(), key, startIndex, count);
        }

        public static int BinarySearch<T>(this NativeList<T> data, T key)
            where T : unmanaged
            , IComparable<T>
        {
            return data.BinarySearch(key, 0, data.Length);
        }

        public static int BinarySearch<T>(this NativeList<T> data, T key, int startIndex, int count)
            where T : unmanaged
            , IComparable<T>
        {
            CheckInRangeOrThrow(startIndex, count, data.Length);
            return NativeCollectionUnsafe.BinarySearch(data.GetUnsafeReadOnlyPtr(), key, startIndex, count);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void CheckInRangeOrThrow(int startIndex, int count, int length)
        {
            if (startIndex + count > length)
                throw new ArgumentOutOfRangeException($"{startIndex} + {count} >= {length}");
        }
    }
}