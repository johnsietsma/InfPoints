using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Debug = UnityEngine.Debug;

namespace InfPoints.Octree
{
    /// <summary>
    /// Store array data contiguously, while allowing indices that are far apart.
    /// For a large amount of data a Dictionary may have better performance. Ideal for first populating and then
    /// processing the data as an array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(NativeSparseArrayDebugView<>))]
    public struct NativeSparseArray<T> : IEnumerable<T>, IDisposable
#if CSHARP_7_3_OR_NEWER
        where T : unmanaged
#else
        where T : struct
#endif
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public bool IsCreated => m_Data.IsCreated;
        public bool IsFull => UsedElementCount == Length;
        public int UsedElementCount => m_UsedElementCount;
        public int Length => m_Data.Length;
        public NativeArray<T> Data => m_Data;

        NativeArray<T> m_Data;
        NativeArray<int> m_Indices;
        int m_UsedElementCount;

        public NativeSparseArray(int length, Allocator allocator)
        {
            var totalSize = UnsafeUtility.SizeOf<T>() * (long) length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent.
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be >= 0");
            if (totalSize > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(length),
                    $"Capacity * sizeof(T) cannot exceed {int.MaxValue} bytes");

            CollectionHelper.CheckIsUnmanaged<T>();
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
#endif

            m_Indices = new NativeArray<int>(length, allocator, NativeArrayOptions.UninitializedMemory);
            m_Data = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
            m_UsedElementCount = 0;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
        }

        public T this[int sparseIndex]
        {
            get
            {
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                return FindDataOrThrow(sparseIndex);
            }
            set
            {
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
                SetValue(value, sparseIndex);
            }
        }

        public bool Contains(int sparseIndex)
        {
            return m_UsedElementCount != 0 && FindDataIndex(sparseIndex) >= 0;
        }
        
        public bool AddValue(T value, int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            if (m_UsedElementCount == m_Data.Length)
            {
                // Array is full
                return false;
            }

            int dataIndex = ~0;
            if (m_UsedElementCount != 0)
            {
                dataIndex = FindDataIndex(sparseIndex);

                if (dataIndex >= 0)
                {
                    // Already exists
                    return false;
                }
            }

            // Doesn't exist yet, insert it
            dataIndex = ~dataIndex; // Two's complement is the insertion point
            m_Indices.Insert(dataIndex, sparseIndex);
            m_Data.Insert(dataIndex, value);
            m_UsedElementCount++;

            return true;
        }

        public bool SetValue(T value, int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            int dataIndex = FindDataIndex(sparseIndex);
            if (dataIndex >= 0 && dataIndex < m_Data.Length)
            {
                // Update the data
                m_Data[dataIndex] = value;
                return true;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            else
            {
                throw new ArgumentOutOfRangeException(nameof(sparseIndex));
            }
#else
            return false;
#endif
        }

        public void RemoveAt(int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            int dataIndex = FindDataIndex(sparseIndex);
            if (dataIndex < 0) throw new ArgumentOutOfRangeException();
            m_Indices.RemoveAt(dataIndex);
            m_Data.RemoveAt(dataIndex);
            m_UsedElementCount--;
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            m_Indices.Dispose();
            m_Data.Dispose();
        }

        T FindDataOrThrow(int sparseIndex)
        {
            int dataIndex = FindDataIndex(sparseIndex);
            if (dataIndex < 0) throw new ArgumentOutOfRangeException(nameof(sparseIndex));
            return m_Data[dataIndex];
        }

        int FindDataIndex(int sparseIndex)
        {
            return m_Indices.BinarySearch(sparseIndex, 0, m_UsedElementCount);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Data.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_Data.GetEnumerator();
        }
    }

    sealed class NativeSparseArrayDebugView<T>
#if CSHARP_7_3_OR_NEWER
        where T : unmanaged
#else
	   	where T : struct
#endif
    {
        NativeSparseArray<T> m_Array;

        public NativeSparseArrayDebugView(NativeSparseArray<T> array)
        {
            m_Array = array;
        }

        public T[] Items => m_Array.Data.ToArray();
    }
}