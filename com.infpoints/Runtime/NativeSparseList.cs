using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints
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
    /// <typeparam name="T"></typeparam>
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}")]
    [DebuggerTypeProxy(typeof(NativeSparseListDebugView<>))]
    public struct NativeSparseList<T> : IEnumerable<T>, IDisposable
        where T : unmanaged
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
        public NativeList<int> Indices;

        /// <summary>
        /// The data in the List.
        /// This data is store contiguously, even though the indices are far apart. 
        /// </summary>
        public NativeList<T> Data;

        /// <summary>
        /// Create a new empty `SparseList`.
        /// </summary>
        /// <param name="initialCapacity">The initial number of items allocated in the `SparseList`</param>
        /// <param name="allocator">The allocator, <see cref="NativeList<T>"/> constructor for documentation of the
        /// different allocator types.</param>
        public NativeSparseList(int initialCapacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent.
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Length must be >= 0");

            var totalSize = UnsafeUtility.SizeOf<T>() * (long) initialCapacity;
            if (totalSize > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity),
                    $"Capacity * sizeof(T) cannot exceed {int.MaxValue} bytes");

            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif

            Indices = new NativeList<int>(initialCapacity, allocator);
            Data = new NativeList<T>(initialCapacity, allocator);
        }

        /// <summary>
        /// Access an element of the SparseList.
        /// When assigning to a new index, this add a new entry to the List.
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. 
        /// </summary>
        /// <param name="sparseIndex"></param>
        public T this[int sparseIndex]
        {
            get
            {
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                CheckIndexExistsOrThrow(sparseIndex);
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
        /// Explicitly add a new index and value to the `SparseList`.
        /// The list will grow by one, allocating memory if it's beyond capacity.
        /// </summary>
        /// <param name="value">The value to add</param>
        /// <param name="sparseIndex">The sparse index of the data</param>
        public void AddValue(T value, int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            CheckIndexDoesntExistOrThrow(sparseIndex);
            
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
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        // `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. Else it will silently fail.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="sparseIndex">The sparse List index</param>
        public void SetValue(T value, int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            CheckIndexExistsOrThrow(sparseIndex);

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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void RemoveAt(int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            CheckIndexExistsOrThrow(sparseIndex);

            int dataIndex = FindDataIndex(sparseIndex);
            Indices.RemoveAt(dataIndex);
            Data.RemoveAt(dataIndex);
        }

        int FindDataIndex(int sparseIndex)
        {
            return Indices.BinarySearch(sparseIndex, 0, Indices.Length);
        }


        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIndexDoesntExistOrThrow(int sparseIndex)
        {
            if (ContainsIndex(sparseIndex))
                throw new ArgumentOutOfRangeException($"Index {sparseIndex} already exists");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIndexExistsOrThrow(int sparseIndex)
        {
            int dataIndex = FindDataIndex(sparseIndex);
            if (dataIndex < 0 || dataIndex >= Data.Length)
            {
                throw new ArgumentOutOfRangeException($"Index {sparseIndex} does not exist");
            }
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

    sealed class NativeSparseListDebugView<T>
#if CSHARP_7_3_OR_NEWER
        where T : unmanaged
#else
	   	where T : struct
#endif
    {
        NativeSparseList<T> m_List;

        public NativeSparseListDebugView(NativeSparseList<T> list)
        {
            m_List = list;
        }

        // Used for the debugger inspector
        // ReSharper disable once UnusedMember.Global
        public T[] Items => m_List.Data.ToArray();
    }
}