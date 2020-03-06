using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    /// <summary>
    /// Store data contiguously, while allowing indices that are far apart.
    /// For a large amount of data a Dictionary may have better performance. Ideal for first populating and then
    /// processing the data as an List.
    ///
    /// Data is stored internally in <see cref="NativeLists"/>, and will have the same memory usage when the list is
    /// past capacity.
    ///
    /// Turn on `ENABLE_UNITY_COLLECTIONS_CHECKS` for runtime checks. It will throw exceptions for any illegal access.
    ///
    /// There is no explicit thread safety and add or remove operations are not atomic. Can be used within an IJob, but
    /// not within an IJobParallelFor unless it is readonly.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}")]
    [DebuggerTypeProxy(typeof(NativeSparseListDebugView<,>))]
    public struct NativeSparseList<TIndex, TData> : IEnumerable<TData>, IDisposable
        where TData : unmanaged
        where TIndex : unmanaged, IComparable<TIndex>
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public bool IsCreated => Data.IsCreated;

        public int Capacity => Data.Capacity;

        /// <summary>
        /// The number of List elements that have been used.
        /// Every time a unique index is used, this count goes up.
        /// </summary>
        public int Length => Data.Length;

        /// <summary>
        /// The sorted indices of data in the `SparseList`.
        /// These indices can be non-contiguous and far apart.
        /// </summary>
        public NativeList<TIndex> Indices;

        /// <summary>
        /// The data in the List.
        /// This data is store contiguously, even though the indices are far apart. 
        /// </summary>
        public NativeList<TData> Data;

        /// <summary>
        /// Create a new empty `SparseList`.
        /// </summary>
        /// <param name="initialCapacity">The initial number of items allocated in the `SparseList`</param>
        /// <param name="allocator">The allocator, <see cref="NativeList<T>"/> constructor for documentation of the
        /// different allocator types.</param>
        public NativeSparseList(int initialCapacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
            // An initial capacity of 0 means the pointer is null and Find throws an exception
            if (initialCapacity == 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity must be > 0");
#endif

            Indices = new NativeList<TIndex>(initialCapacity, allocator);
            Data = new NativeList<TData>(initialCapacity, allocator);
        }

        public NativeSparseList(NativeList<TIndex> indices, NativeList<TData> data, Allocator allocator)
            : this(indices.Length, allocator)
        {
            Indices.AddRangeNoResize(indices);
            Data.AddRangeNoResize(data);
        }
        
        public NativeSparseList(TIndex[] indices, TData[] data, Allocator allocator)
            : this(indices.Length, allocator)
        {
            Indices.CopyFrom(indices);
            Data.CopyFrom(data);
        }

        /// <summary>
        /// Access an element of the SparseList.
        /// When assigning to a new index, this add a new entry to the List.
        /// Throws <exception cref="IndexOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. 
        /// </summary>
        /// <param name="sparseIndex"></param>
        public TData this[TIndex sparseIndex]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                var dataIndex = FindDataIndex(sparseIndex);
                return Data[dataIndex];
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                if (!ContainsIndex(sparseIndex)) AddValue(sparseIndex, value);
                else SetValue(value, sparseIndex);
            }
        }

        public bool ContainsIndex(TIndex sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            return FindDataIndex(sparseIndex) >= 0;
        }

        /// <summary>
        /// Explicitly add a new index and value to the `SparseList`.
        /// The list will grow by one, allocating memory if it's beyond capacity.
        /// </summary>
        /// <param name="sparseIndex">The sparse index of the data</param>
        /// <param name="value">The value to add</param>
        [WriteAccessRequired]
        public void AddValue(TIndex sparseIndex, TData value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            CheckIndexDoesntExistOrThrow(sparseIndex);
#endif

            int dataIndex = FindDataIndex(sparseIndex);
            dataIndex = ~dataIndex; // Two's complement is the insertion point

            // Make room in the list to insert
            Indices.Add(default);
            Data.Add(default);

            Indices.Insert(dataIndex, sparseIndex);
            Data.Insert(dataIndex, value);
        }

        /// <summary>
        /// Set the value of an existing sparse List index.
        /// Throws <exception cref="IndexOutOfRangeException"></exception> if the index does not exist and
        // `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. Else it will silently fail.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="sparseIndex">The sparse List index</param>
        [WriteAccessRequired]
        public void SetValue(TData value, TIndex sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            int dataIndex = FindDataIndex(sparseIndex);
            // Update the data
            Data[dataIndex] = value;
        }

        /// <summary>
        /// Remove the sparse List element.
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. Else it will silently fail.
        /// </summary>
        /// <param name="sparseIndex"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        [WriteAccessRequired]
        public void RemoveAtSwapBack(TIndex sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            int dataIndex = FindDataIndex(sparseIndex);
            Indices.RemoveAtSwapBack(dataIndex);
            Data.RemoveAtSwapBack(dataIndex);
        }

        int FindDataIndex(TIndex sparseIndex)
        {
            return Indices.BinarySearch(sparseIndex, 0, Indices.Length);
        }


        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIndexDoesntExistOrThrow(TIndex sparseIndex)
        {
            if (ContainsIndex(sparseIndex))
                throw new ArgumentOutOfRangeException($"Index {sparseIndex} already exists");
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            Indices.Dispose();
            Data.Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public IEnumerator<TData> GetEnumerator()
        {
            return Data.GetEnumerator();
        }
    }

    sealed class NativeSparseListDebugView<TIndex, TData>
        where TData : unmanaged
        where TIndex : unmanaged, IComparable<TIndex>
    {
        NativeSparseList<TIndex, TData> m_List;

        public NativeSparseListDebugView(NativeSparseList<TIndex, TData> list)
        {
            m_List = list;
        }

        // Used for the debugger inspector
        // ReSharper disable once UnusedMember.Global
        public TData[] Items => m_List.Data.ToArray();
    }
}