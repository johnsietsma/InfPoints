using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints
{
    /// <summary>
    /// Store array data contiguously, while allowing indices that are far apart.
    /// For a large amount of data a Dictionary may have better performance. Ideal for first populating and then
    /// processing the data as an array.
    ///
    /// Turn on `ENABLE_UNITY_COLLECTIONS_CHECKS` for runtime checks.
    ///
    /// Prefer using the explicit interface `AddValue`, `IsFull` etc rather then the index operator. The index operator
    /// may silently fail if the array is full and `ENABLE_UNITY_COLLECTIONS_CHECKS` is off.
    ///
    /// There is no explicit thread safety and add or remove operations are not atomic. Use a read only within Jobs.
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
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif
        
        public bool IsCreated => m_NativeSparseArrayData.Data.IsCreated;

        /// <summary>
        /// Have all the indices been filled.
        /// </summary>
        public bool IsFull => m_NativeSparseArrayData.UsedElementCount == m_NativeSparseArrayData.Data.Length;

        public int Length => m_NativeSparseArrayData.Data.Length;

        /// <summary>
        /// The number of array elements that have been used.
        /// Every time a unique index is used, this count goes up.
        /// When the `SparseArray` is full no more items can be added.
        /// </summary>
        public int UsedElementCount => m_NativeSparseArrayData.UsedElementCount;

        /// <summary>
        /// The sorted indices of data in the `SparseArray`.
        /// These indices can be non-contiguous and far apart.
        /// </summary>
        public NativeArray<int> Indices => m_NativeSparseArrayData.Indices;
        
        /// <summary>
        /// The data in the array.
        /// This data is store contiguously, even though the indices are far apart. 
        /// </summary>
        public NativeArray<T> Data => m_NativeSparseArrayData.Data;

        NativeSparseArrayData<T> m_NativeSparseArrayData;

        /// <summary>
        /// Create a new empty `SparseArray`.
        /// </summary>
        /// <param name="length">The number of items the `SparseArray` can hold</param>
        /// <param name="allocator">The allocator, <see cref="NativeArray<T>"/> constructor for documentation of the
        /// different allocator types.</param>
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

            m_NativeSparseArrayData = new NativeSparseArrayData<T>(length, allocator);
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
        }

        /// <summary>
        /// Access an element of the SparseArray.
        /// When assigning to a new index, this add a new entry to the array.
        /// Prefer to use <see cref="IsFull"/>,<see cref="ContainsIndex"/> and <see cref="SetValue"/> to explicitly
        /// write to the array.
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. 
        /// </summary>
        /// <param name="sparseIndex"></param>
        public T this[int sparseIndex]
        {
            get
            {
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
                return FindDataOrThrow(sparseIndex, ref m_NativeSparseArrayData);
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
            return ContainsIndex(sparseIndex, ref m_NativeSparseArrayData);
        }
        
        public static bool ContainsIndex(int sparseIndex, ref NativeSparseArrayData<T> data)
        {
            return data.UsedElementCount != 0 && FindDataIndex(sparseIndex, ref data) >= 0;
        }

        /// <summary>
        /// Explicitly add a new index and value to the `SparseArray`.
        /// </summary>
        /// <param name="value">The value to add</param>
        /// <param name="sparseIndex">The sparse index of the data</param>
        /// <returns>False if the array is full or the index has already been added.</returns>
        public bool AddValue(T value, int sparseIndex)
        {
            return AddValue(value, sparseIndex, ref m_NativeSparseArrayData);
        }
        
        public static bool AddValue(T value, int sparseIndex, ref NativeSparseArrayData<T> data)
        {
            if (data.UsedElementCount == data.Data.Length)
            {
                // Array is full
                return false;
            }

            int dataIndex = FindDataIndex(sparseIndex, ref data);

            if (dataIndex >= 0)
            {
                // Already exists
                return false;
            }

            // Doesn't exist yet, insert it
            dataIndex = ~dataIndex; // Two's complement is the insertion point
            data.Indices.Insert(dataIndex, sparseIndex);
            data.Data.Insert(dataIndex, value);
            data.UsedElementCount++;

            return true;
        }

        /// <summary>
        /// Set the value of an existing sparse array index.
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="sparseIndex">The sparse array index</param>
        /// <returns>False if the index does not exist and `ENABLE_UNITY_COLLECTIONS_CHECKS` has not been set.</returns>
        public bool SetValue(T value, int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            return SetValue(value, sparseIndex, ref m_NativeSparseArrayData);
        }
        
        public static bool SetValue(T value, int sparseIndex, ref NativeSparseArrayData<T> data)
        {
            int dataIndex = FindDataIndex(sparseIndex, ref data);
            if (dataIndex >= 0 && dataIndex < data.Data.Length)
            {
                // Update the data
                data.Data[dataIndex] = value;
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

        /// <summary>
        /// Remove the sparse array element.
        /// Throws <exception cref="ArgumentOutOfRangeException"></exception> if the index does not exist and
        /// `ENABLE_UNITY_COLLECTIONS_CHECKS` is enabled. Else it will silently fail.
        /// </summary>
        /// <param name="sparseIndex"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool RemoveAt(int sparseIndex)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            return RemoveAt(sparseIndex, ref m_NativeSparseArrayData);
        }
        
        public static bool RemoveAt(int sparseIndex, ref NativeSparseArrayData<T> data)
        {
            int dataIndex = FindDataIndex(sparseIndex, ref data);

            if (dataIndex < 0)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                throw new ArgumentOutOfRangeException();
#else
                return false;
#endif
            }

            data.Indices.RemoveAt(dataIndex);
            data.Data.RemoveAt(dataIndex);
            data.UsedElementCount--;

            return true;
        }
        
        static T FindDataOrThrow(int sparseIndex, ref NativeSparseArrayData<T> data)
        {
            int dataIndex = FindDataIndex(sparseIndex, ref data);
            if (dataIndex < 0) throw new ArgumentOutOfRangeException(nameof(sparseIndex));
            return data.Data[dataIndex];
        }

        static int FindDataIndex(int sparseIndex, ref NativeSparseArrayData<T> data)
        {
            return data.Indices.BinarySearch(sparseIndex, 0, data.UsedElementCount);
        }

        public void Dispose()
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            
            m_NativeSparseArrayData.Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_NativeSparseArrayData.Data.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_NativeSparseArrayData.Data.GetEnumerator();
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

        // Used for the debugger inspector
        // ReSharper disable once UnusedMember.Global
        public T[] Items => m_Array.Data.ToArray();
    }
}