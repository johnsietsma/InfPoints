using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    public struct MemorySpan
    {
        public int DataIndex;
        public int StartIndex;
        public int Length;

        public override string ToString()
        {
            return $"Data Index: {DataIndex} Start Index: {StartIndex} Length: {Length}";
        }
    }

    /// <summary>
    /// Used to store very large amount of data split over separate arrays.
    /// It allows for consolidation of stored data, while keeping a consistent handle to ranges of that data.
    /// </summary>
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}, IsCreated = {IsCreated}")]
    public unsafe struct SparseMemorySpanArray<T> : IDisposable where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public bool IsCreated => m_MemorySpans.IsCreated;
        
        /// <summary>
        /// The number of <see cref="MemorySpan"/> that have been added.
        /// </summary>
        public int Length => m_MemorySpans.Length;
        
        /// <summary>
        /// The number of <see cref="MemorySpan"/> that can be stored.
        /// </summary>
        public int Capacity => m_Capacity;
        
        /// <summary>
        /// The maximum size of each <see cref="MemorySpan"/>
        /// </summary>
        public int SpanCapacity => m_SpanCapacity;

        readonly int m_Capacity;
        readonly int m_SpanCapacity;
        readonly Allocator m_Allocator;
        NativeSparseArray<MemorySpan> m_MemorySpans;
        [NativeDisableUnsafePtrRestriction] T** m_Data;

        /// <summary>
        /// SparseMemorySpanArray constructor.
        /// Only <see cref="MemorySpan"/> pointers are allocated during construction. The actual <see cref="MemorySpan"/>
        /// are allocated as points are added. 
        /// </summary>
        /// <param name="capacity">The maximum number of <see cref="MemorySpan"/></param>
        /// <param name="spanCapacity">The maximum size of each <see cref="MemorySpan"/></param>
        /// <param name="allocator">The <see cref="Unity.Collections.Allocator"/> to use to allocate each <see cref="MemorySpan"/></param>
        /// <exception cref="Exception"></exception>
        public SparseMemorySpanArray(int capacity, int spanCapacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
            
            // Checks happen in NativeSparseArray constructor. I wont repeat them here.
#endif
            m_Capacity = capacity;
            m_SpanCapacity = spanCapacity;
            m_MemorySpans = new NativeSparseArray<MemorySpan>(capacity, allocator);
            m_Data = (T**)UnsafeUtility.Malloc(IntPtr.Size * capacity, UnsafeUtility.AlignOf<T>(), allocator);
            m_Allocator = allocator;
        }

        /// <summary>
        /// A data to the <see cref="MemorySpan"/> the belongs to the sparseIndex. If this is a new sparseIndex, then a
        /// new <see cref="MemorySpan"/> will be created and data added to it. Otherwise the data will be added to the
        /// existing <see cref="MemorySpan"/>.
        /// </summary>
        /// <param name="sparseIndex">The sparse index at add the data to</param>
        /// <param name="data">The data to add</param>
        public void AddData(int sparseIndex, NativeArray<T> data)
        {
            if (!m_MemorySpans.ContainsIndex(sparseIndex))
            {
                AddSpan(sparseIndex);
            }

            var span = m_MemorySpans[sparseIndex];
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if( span.StartIndex + span.Length + data.Length > m_SpanCapacity )
                throw new ArgumentOutOfRangeException($"Data of length {data.Length} will not fit in Span {span}");
#endif            
            void* destination = m_Data[span.DataIndex] + UnsafeUtility.SizeOf<T>() * span.StartIndex;
            void* source = data.GetUnsafeReadOnlyPtr();
            UnsafeUtility.MemCpy(destination, source, UnsafeUtility.SizeOf<T>() * data.Length);
            span.Length += data.Length;
        }

        /// <summary>
        /// Return the <see cref="MemorySpan"/> at sparseIndex as a <see cref="NativeArray{T}"/>.
        /// No memory is allocated.
        /// </summary>
        /// <param name="sparseIndex">The sparse index to retrieve</param>
        /// <returns>A <see cref="NativeArray{T}"/>containing the data</returns>
        public NativeArray<T> AsArray(int sparseIndex)
        {
            var span = m_MemorySpans[sparseIndex];
            void* dataPointer = m_Data[span.DataIndex] + span.StartIndex * UnsafeUtility.SizeOf<T>();
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(dataPointer, span.Length, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, m_Safety);
#endif
            return array;
        }

        public void Dispose()
        {
            m_MemorySpans.Dispose();
            for (int index = 0; index < Length; index++)
            {
                UnsafeUtility.Free(m_Data[index], m_Allocator);
            }
            UnsafeUtility.Free(m_Data, m_Allocator);
        }

        void AddSpan(int sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_MemorySpans.ContainsIndex(sparseIndex))
                throw new ArgumentException($"Span at index {sparseIndex} already exists");
#endif

            int index = m_MemorySpans.Length;
            var span = new MemorySpan() {DataIndex = index, Length = 0, StartIndex = 0};
            m_MemorySpans.AddValue(span, sparseIndex);
            m_Data[index] = (T*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * m_SpanCapacity, UnsafeUtility.AlignOf<T>(),
                m_Allocator);
        }
    }
}