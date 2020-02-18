using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    public struct PageAllocation
    {
        public int PageIndex;
        public int StartIndex;
        public int Capacity;
        public int Length;

        public bool IsFull => Length == Capacity;

        public int FreeLength => Capacity - Length - StartIndex;

        public override string ToString()
        {
            return $"Page Index: {PageIndex} Start Index: {StartIndex} Length: {Length} Capacity: {Capacity}";
        }
    }

    /// <summary>
    /// Used to store very large amount of data split over separate arrays.
    /// It allows for consolidation of stored data, while keeping a consistent index to ranges of that data.
    /// </summary>
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}, IsCreated = {IsCreated}")]
    public unsafe struct NativeSparsePagedArray<T> : IDisposable where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public bool IsCreated => m_PageAllocations.IsCreated;

        /// <summary>
        /// The number of <see cref="PageAllocation"/> that have been added.
        /// </summary>
        public int Length => m_PageAllocations.Length;

        public int PageCount => m_PageCount;


        /// <summary>
        /// The number of <see cref="PageAllocation"/> that can be stored.
        /// </summary>
        public int MaximumPageCount => m_MaximumPageCount;

        /// <summary>
        /// The maximum size of each <see cref="PageAllocation"/>
        /// </summary>
        public int PageSize => m_PageSize;

        public NativeArray<ulong> Indices => m_PageAllocations.Indices;

        readonly int m_AllocationSize;
        readonly int m_PageSize;
        readonly int m_MaximumPageCount;
        int m_PageCount;
        PageAllocation m_LastPageAllocation;

        readonly Allocator m_Allocator;

        // Sorted array
        NativeSparseArray<PageAllocation> m_PageAllocations;
        [NativeDisableUnsafePtrRestriction] T** m_Pages;

        /// <summary>
        /// Only page pointers are allocated during construction. The actual page memory is only allocated as needed. 
        /// </summary>
        /// <param name="allocationSize">The size of each allocation from a page</param>
        /// <param name="pageSize">The size of each allocated page</param>
        /// <param name="maximumPageCount">The maximum number of pages that can be allocated</param>
        /// <param name="allocator">The <see cref="Unity.Collections.Allocator"/> to use to allocate each <see cref="PageAllocation"/></param>
        /// <exception cref="Exception"></exception>
        public NativeSparsePagedArray(int allocationSize, int allocationsPerPage, int maximumPageCount,
            Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
            if (allocationsPerPage <= 0)
                throw new ArgumentException("Must be greater then 0", nameof(allocationsPerPage));
            if (allocationSize <= 0) throw new ArgumentException("Must be greater then 0", nameof(allocationSize));
            if (maximumPageCount <= 0) throw new ArgumentException("Must be greater then 0", nameof(maximumPageCount));
#endif
            m_AllocationSize = allocationSize;
            m_PageSize = allocationsPerPage * allocationSize;
            m_MaximumPageCount = maximumPageCount;
            m_PageAllocations = new NativeSparseArray<PageAllocation>(maximumPageCount * allocationsPerPage, allocator);
            m_Pages = (T**) UnsafeUtility.Malloc(IntPtr.Size * maximumPageCount, UnsafeUtility.AlignOf<T>(), allocator);
            m_Allocator = allocator;
            m_PageCount = 0;
            m_LastPageAllocation = default;
        }

        public bool ContainsAllocation(ulong sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            return m_PageAllocations.ContainsIndex(sparseIndex);
        }


        public PageAllocation GetAllocation(ulong sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
            if (!ContainsAllocation(sparseIndex)) throw new ArgumentException(nameof(sparseIndex));
#endif
            return m_PageAllocations[sparseIndex];
        }

        /// <summary>
        /// Return the <see cref="PageAllocation"/> at sparseIndex as a <see cref="NativeArray{T}"/>.
        /// No memory is allocated.
        /// </summary>
        /// <param name="sparseIndex">The sparse index to retrieve</param>
        /// <returns>A <see cref="NativeArray{T}"/>containing the data</returns>
        public NativeArray<T> ToArray(ulong sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            var allocation = m_PageAllocations[sparseIndex];
            void* dataPointer = m_Pages[allocation.PageIndex] + allocation.StartIndex;
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(dataPointer, allocation.Length,
                Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, m_Safety);
#endif
            return array;
        }

        public void AddIndex(ulong sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            if (ContainsAllocation(sparseIndex))
                throw new ArgumentException($"Sparse index {sparseIndex} already exists.", nameof(sparseIndex));
            if (m_PageAllocations.IsFull)
                throw new InvalidOperationException("Page allocations at capacity. Increase maximum page count.");
#endif

            int pageIndex = -1;
            int startIndex = -1;

            // See if a new allocation will fit in the current page
            if (Length > 0)
            {
                if (m_LastPageAllocation.StartIndex + m_LastPageAllocation.Capacity + m_AllocationSize < m_PageSize)
                {
                    pageIndex = m_LastPageAllocation.PageIndex;
                    startIndex = m_LastPageAllocation.StartIndex + m_LastPageAllocation.Capacity;
                }
            }

            if (pageIndex == -1)
            {
                pageIndex = AddPage(sparseIndex);
                startIndex = 0;
            }

            var allocation = new PageAllocation()
            {
                PageIndex = pageIndex,
                StartIndex = startIndex,
                Length = 0,
                Capacity = m_AllocationSize
            };

            m_PageAllocations.AddValue(allocation, sparseIndex);
        }

        public void Add(ulong sparseIndex, T data)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            CheckContainsIndexAndThrow(sparseIndex);
            CheckHasCapacityAndThrow(sparseIndex, 1);
#endif
            var pageAllocation = m_PageAllocations[sparseIndex];
            void* destination = m_Pages[pageAllocation.PageIndex] +
                                UnsafeUtility.SizeOf<T>() * pageAllocation.StartIndex;
            UnsafeUtility.WriteArrayElement(destination, pageAllocation.Length * UnsafeUtility.SizeOf<T>(), data);
            pageAllocation.Length++;
            m_PageAllocations[sparseIndex] = pageAllocation;
            m_LastPageAllocation = pageAllocation;
        }

        /// <summary>
        /// A data to the <see cref="PageAllocation"/> the belongs to the sparseIndex. If this is a new sparseIndex, then a
        /// new <see cref="PageAllocation"/> will be created and data added to it. Otherwise the data will be added to the
        /// existing <see cref="PageAllocation"/>.
        /// </summary>
        /// <param name="sparseIndex">The sparse index at add the data to</param>
        /// <param name="data">The data to add</param>
        public void AddRange(ulong sparseIndex, NativeSlice<T> data)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Checks.CheckNullAndThrow(data, nameof(data));
#endif

            AddRange(sparseIndex, data.GetUnsafeReadOnlyPtr(), data.Length);
        }

        public void AddRange(ulong sparseIndex, NativeArray<T> data)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Checks.CheckNullAndThrow(data, nameof(data));
#endif

            AddRange(sparseIndex, data.GetUnsafeReadOnlyPtr(), data.Length);
        }

        public void AddRange(ulong sparseIndex, NativeSlice<T> data, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Checks.CheckNullAndThrow(data, nameof(data));
#endif

            AddRange(sparseIndex, data.GetUnsafeReadOnlyPtr(), count);
        }

        public void AddRange(ulong sparseIndex, NativeArray<T> data, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Checks.CheckNullAndThrow(data, nameof(data));
#endif

            AddRange(sparseIndex, data.GetUnsafeReadOnlyPtr(), count);
        }

        void AddRange(ulong sparseIndex, void* data, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            Checks.CheckNullAndThrow(data, nameof(data));
            CheckContainsIndexAndThrow(sparseIndex);
            CheckHasCapacityAndThrow(sparseIndex, count);
#endif
            var pageAllocation = m_PageAllocations[sparseIndex];

            void* destination = m_Pages[pageAllocation.PageIndex] +
                                UnsafeUtility.SizeOf<T>() * pageAllocation.StartIndex;
            UnsafeUtility.MemCpy(destination, data, UnsafeUtility.SizeOf<T>() * count);
            pageAllocation.Length += count;
            m_PageAllocations[sparseIndex] = pageAllocation;
            m_LastPageAllocation = pageAllocation;
        }


        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            m_PageAllocations.Dispose();
            for (int index = 0; index < m_PageCount; index++)
            {
                UnsafeUtility.Free(m_Pages[index], m_Allocator);
            }

            UnsafeUtility.Free(m_Pages, m_Allocator);
        }

        int AddPage(ulong sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_MaximumPageCount == PageCount) throw new InvalidOperationException("Ran out of pages");
            if (m_PageAllocations.ContainsIndex(sparseIndex))
                throw new ArgumentException($"Span at index {sparseIndex} already exists");
#endif

            int index = m_PageAllocations.Length;
            m_Pages[index] = (T*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * m_PageSize,
                UnsafeUtility.AlignOf<T>(),
                m_Allocator);
            m_PageCount++;
            return index;
        }

        void CheckHasCapacityAndThrow(ulong sparseIndex, int length)
        {
            if (m_PageAllocations[sparseIndex].FreeLength < length)
                throw new ArgumentOutOfRangeException(
                    $"Data of length {length} will not fit in allocation {m_PageAllocations[sparseIndex]}");
        }

        void CheckContainsIndexAndThrow(ulong sparseIndex)
        {
            if (!m_PageAllocations.ContainsIndex(sparseIndex))
                throw new ArgumentOutOfRangeException(nameof(sparseIndex));
        }
    }
}