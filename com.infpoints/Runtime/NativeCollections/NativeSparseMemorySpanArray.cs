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
    }

    /// <summary>
    /// A <see cref="NativeSparseArray{T}"/> that contains <see cref="NativeArray{T}"/>.
    /// This can be used to store very large amount of data split over separate arrays.
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

        public bool IsCreated => m_Indices.IsCreated;
        public int Length => m_Indices.Length;
        public int Capacity => m_Capacity;
        public int SpanCapacity => m_SpanCapacity;

        int m_Capacity;
        int m_SpanCapacity;
        NativeSparseArray<MemorySpan> m_Indices;
        [NativeDisableUnsafePtrRestriction] T** m_Data;
        Allocator m_Allocator;

        public SparseMemorySpanArray(int capacity, int spanCapacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (allocator == Allocator.Invalid)
            {
                throw new Exception("NativeSparseIndexedArray is not initialized, it must be initialized with allocator before use.");
            }
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
            // Other checks happen in Indices constructor
#endif
            m_Capacity = capacity;
            m_SpanCapacity = spanCapacity;
            m_Indices = new NativeSparseArray<MemorySpan>(capacity, allocator);
            m_Data = (T**)UnsafeUtility.Malloc(IntPtr.Size * capacity, UnsafeUtility.AlignOf<T>(), allocator);
            m_Allocator = allocator;
        }

        public MemorySpan AddSpan(int sparseIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_Indices.ContainsIndex(sparseIndex))
                throw new ArgumentException($"Span at index {sparseIndex} already exists");
#endif

            int index = m_Indices.Length;
            var span = new MemorySpan() {DataIndex = index, Length = 0, StartIndex = 0};
            m_Indices.AddValue(span, sparseIndex);
            m_Data[index] = (T*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * m_SpanCapacity, UnsafeUtility.AlignOf<T>(),
                m_Allocator);

            return span;
        }

        public void AddData(ref MemorySpan span, NativeArray<T> data)
        {
            void* destination = m_Data[span.DataIndex] + UnsafeUtility.SizeOf<T>() * span.StartIndex;
            void* source = data.GetUnsafeReadOnlyPtr();
            UnsafeUtility.MemCpy(destination, source, UnsafeUtility.SizeOf<T>() * data.Length);
            span.Length += data.Length;
        }

        public NativeArray<T> AsArray(MemorySpan span)
        {
            void* dataPointer = m_Data[span.DataIndex] + span.StartIndex * UnsafeUtility.SizeOf<T>();
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(dataPointer, span.Length, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, m_Safety);
#endif
            return array;
        }

        public void Dispose()
        {
            m_Indices.Dispose();
            for (int index = 0; index < Length; index++)
            {
                UnsafeUtility.Free(m_Data[index], m_Allocator);
            }
            UnsafeUtility.Free(m_Data, m_Allocator);
        }
    }
}