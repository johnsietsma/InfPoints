using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    /// <summary>
    /// Store array data contiguously, while allowing indices that are far apart.
    /// For a large amount of data a Dictionary may have better performance. Ideal for first populating and then
    /// processing the data as an array.
    ///
    /// Turn on `ENABLE_UNITY_COLLECTIONS_CHECKS` for runtime checks. It will throw exceptions for any illegal array
    /// access.
    ///
    /// There is no explicit thread safety and add or remove operations are not atomic. Can be used within an IJob, but
    /// not within an IJobParallelFor unless it is readonly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [NativeContainer]
    [DebuggerDisplay("Length = {Capacity}")]
    [DebuggerTypeProxy(typeof(NativeSparseArrayDebugView<>))]
    public struct NativeSparseArray<T> : IEnumerable<T>, IDisposable
        where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public bool IsCreated => Data.IsCreated;

        /// <summary>
        /// Have all the indices been filled.
        /// </summary>
        public bool IsFull => Length == Data.Length;

        public int Capacity => Data.Length;

        /// <summary>
        /// The number of array elements that have been used.
        /// Every time a unique index is used, this count goes up.
        /// When the `SparseArray` is full no more items can be added.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The sorted indices of data in the `SparseArray`.
        /// These indices can be non-contiguous and far apart.
        /// </summary>
        public NativeArray<int> Indices;

        /// <summary>
        /// The data in the array.
        /// This data is store contiguously, even though the indices are far apart. 
        /// </summary>
        public NativeArray<T> Data;

        /// <summary>
        /// Create a new empty `SparseArray`.
        /// </summary>
        /// <param name="capacity">The number of items the `SparseArray` can hold</param>
        /// <param name="allocator">The allocator, <see cref="NativeArray<T>"/> constructor for documentation of the
        /// different allocator types.</param>
        public NativeSparseArray(int capacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Other checks happen in Data and Indices constructors
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif

            Indices = new NativeArray<int>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
            Data = new NativeArray<T>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
            Length = 0;
        }

        /// <summary>
        /// Access an element of the SparseArray.
        /// When assigning to a new index, this add a new entry to the array.
        /// Prefer to use <see cref="IsFull"/>,<see cref="ContainsIndex"/> and <see cref="SetValue"/> to explicitly
        /// write to the array to avoid silent add failures.
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. 
        /// </summary>
        /// <param name="sparseIndex"></param>
        public T this[int sparseIndex]
        {
            get
            {
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                var dataIndex = FindDataIndex(sparseIndex);
                return Data[dataIndex];
            }
            set
            {
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                if (!ContainsIndex(sparseIndex)) AddValue(value, sparseIndex);
                else SetValue(value, sparseIndex);
            }
        }

        public bool ContainsIndex(int sparseIndex)
        {
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            return FindDataIndex(sparseIndex) >= 0;
        }

        /// <summary>
        /// If a copy of this struct has elements added to it, then the `UsedElementCount` becomes out of date.
        /// Use this to update the count if you are required to use a copy, in a job for example.
        /// </summary>
        /// <param name="count"></param>
        public void IncrementUsedElementCount(int count)
        {
            Length += count;
        }


        /// <summary>
        /// Explicitly add a new index and value to the `SparseArray`.
        /// </summary>
        /// <param name="value">The value to add</param>
        /// <param name="sparseIndex">The sparse index of the data</param>
        public void AddValue(T value, int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            CheckFullAndThrow();
            CheckIndexDoesntExistOrThrow(sparseIndex);

            int dataIndex = FindDataIndex(sparseIndex);
            dataIndex = ~dataIndex; // Two's complement is the insertion point
            Indices.Insert(dataIndex, sparseIndex);
            Data.Insert(dataIndex, value);
            Length++;
        }

        /// <summary>
        /// Set the value of an existing sparse array index.
        /// Throws <exception cref="IndexOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. Else it will silently fail.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="sparseIndex">The sparse array index</param>
        public void SetValue(T value, int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            int dataIndex = FindDataIndex(sparseIndex);
            // Update the data
            Data[dataIndex] = value;
        }

        /// <summary>
        /// Remove the sparse array element.
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. Else it will silently fail.
        /// </summary>
        /// <param name="sparseIndex"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void RemoveAt(int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            int dataIndex = FindDataIndex(sparseIndex);
            Indices.RemoveAt(dataIndex);
            Data.RemoveAt(dataIndex);
            Length--;
        }

        int FindDataIndex(int sparseIndex)
        {
            return Indices.BinarySearch(sparseIndex, 0, Length);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckFullAndThrow()
        {
            if (IsFull)
                throw new ArgumentOutOfRangeException("Adding value to full array");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIndexDoesntExistOrThrow(int sparseIndex)
        {
            if (ContainsIndex(sparseIndex))
                throw new ArgumentOutOfRangeException($"Index {sparseIndex} already exists");
        }

        public void Dispose()
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            Indices.Dispose();
            Data.Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Data.GetEnumerator();
        }
    }

    sealed class NativeSparseArrayDebugView<T>
        where T : unmanaged
    {
        NativeSparseArray<T> m_Array;

        public NativeSparseArrayDebugView(NativeSparseArray<T> array)
        {
            m_Array = array;
        }

        // Used for the debugger inspector
        // ReSharper disable once UnusedMember.Global
        public T[] Items => m_Array.Data.ToArray();
    }
}