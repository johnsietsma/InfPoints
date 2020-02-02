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

        public override string ToString()
        {
            return $"Page Index: {PageIndex} Start Index: {StartIndex} Length: {Capacity}";
        }
    }

    /// <summary>
    /// Used to store very large amount of data split over separate arrays.
    /// It allows for consolidation of stored data, while keeping a consistent handle to ranges of that data.
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

        readonly int m_AllocationSize;
        readonly int m_PageSize;
        readonly int m_MaximumPageCount;
        int m_PageCount;

        readonly Allocator m_Allocator;

        // Sorted array
        NativeSparseArray<PageAllocation> m_PageAllocations;
        [NativeDisableUnsafePtrRestriction] T** m_Pages;

        /// <summary>
        /// Only page pointers are allocated during construction. The actual page memory is only allocated as needed. 
        /// </summary>
        /// <param name="aloocationSize">The size of each allocation from a page</param>
        /// <param name="pageSize">The size of each allocated page</param>
        /// <param name="maximumPageCount">The maximum number of pages that can be allocated</param>
        /// <param name="allocator">The <see cref="Unity.Collections.Allocator"/> to use to allocate each <see cref="PageAllocation"/></param>
        /// <exception cref="Exception"></exception>
        public NativeSparsePagedArray(int allocationSize, int pageSize, int maximumPageCount, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);

            // Checks happen in NativeSparseArray constructor. I wont repeat them here.
#endif
            m_AllocationSize = allocationSize;
            m_PageSize = pageSize;
            m_MaximumPageCount = maximumPageCount;
            m_PageAllocations = new NativeSparseArray<PageAllocation>(maximumPageCount, allocator);
            m_Pages = (T**) UnsafeUtility.Malloc(IntPtr.Size * maximumPageCount, UnsafeUtility.AlignOf<T>(), allocator);
            m_Allocator = allocator;
            m_PageCount = 0;
        }

        bool ContainsIndex(ulong sparseIndex)
        {
            return m_PageAllocations.ContainsIndex(sparseIndex);
        }

        public void AddIndex(ulong sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (ContainsIndex(sparseIndex))
                throw new ArgumentException($"Sparse index {sparseIndex} already exists.", nameof(sparseIndex));
#endif

            int pageIndex = -1;
            int startIndex = -1;

            // See if a new allocation will fit in the current page
            if (Length > 0)
            {
                var lastAllocation = m_PageAllocations[m_PageAllocations.Length - 1];
                if (lastAllocation.StartIndex + lastAllocation.Capacity + m_AllocationSize < m_PageSize)
                {
                    pageIndex = lastAllocation.PageIndex;
                    startIndex = lastAllocation.StartIndex + lastAllocation.Capacity;
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
                Capacity = m_AllocationSize
            };

            m_PageAllocations.AddValue(allocation, sparseIndex);
        }

        /// <summary>
        /// A data to the <see cref="PageAllocation"/> the belongs to the sparseIndex. If this is a new sparseIndex, then a
        /// new <see cref="PageAllocation"/> will be created and data added to it. Otherwise the data will be added to the
        /// existing <see cref="PageAllocation"/>.
        /// </summary>
        /// <param name="sparseIndex">The sparse index at add the data to</param>
        /// <param name="data">The data to add</param>
        public void AddRange(ulong sparseIndex, NativeArray<T> data)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (data == default) throw new ArgumentNullException(nameof(data));
            if (!m_PageAllocations.ContainsIndex(sparseIndex))
                throw new InvalidOperationException("Adding data to non-existent index");
            if (m_PageAllocations[sparseIndex].StartIndex + m_PageAllocations[sparseIndex].Length + data.Length >
                m_PageAllocations[sparseIndex].Capacity)
                throw new ArgumentOutOfRangeException(
                    $"Data of length {data.Length} will not fit in allocation {m_PageAllocations[sparseIndex]}");
#endif
            var pageAllocation = m_PageAllocations[sparseIndex];

            void* destination = m_Pages[pageAllocation.PageIndex] +
                                UnsafeUtility.SizeOf<T>() * pageAllocation.StartIndex;
            void* source = data.GetUnsafeReadOnlyPtr();
            UnsafeUtility.MemCpy(destination, source, UnsafeUtility.SizeOf<T>() * data.Length);
            pageAllocation.Length += data.Length;
            m_PageAllocations[sparseIndex] = pageAllocation;
        }

        /// <summary>
        /// Return the <see cref="PageAllocation"/> at sparseIndex as a <see cref="NativeArray{T}"/>.
        /// No memory is allocated.
        /// </summary>
        /// <param name="sparseIndex">The sparse index to retrieve</param>
        /// <returns>A <see cref="NativeArray{T}"/>containing the data</returns>
        public NativeArray<T> AsArray(ulong sparseIndex)
        {
            var allocation = m_PageAllocations[sparseIndex];
            void* dataPointer = m_Pages[allocation.PageIndex] + allocation.StartIndex;
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(dataPointer, allocation.Length,
                Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, m_Safety);
#endif
            return array;
        }

        public void Dispose()
        {
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
    }
}