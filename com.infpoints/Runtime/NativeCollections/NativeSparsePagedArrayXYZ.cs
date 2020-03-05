using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace InfPoints.NativeCollections
{
    /// <summary>
    /// A SoA wrapper around <see cref="NativeSparseArray{T}"/>.
    /// Contains X,Y,Z arrays to hold points.
    /// </summary>
    [NativeContainer]
    public struct NativeSparsePagedArrayXYZ : IDisposable 
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public bool IsCreated => m_DataX.IsCreated;
        public int Length => m_DataX.Length;

        public int DataLength
        {
            get
            {
                int length = 0;
                for (int index = 0; index < m_DataX.Length; index++)
                {
                    ulong sparseIndex = m_DataX.Indices[index];
                    length += m_DataX.GetAllocation(sparseIndex).Length;
                }

                return length;
            }
        }

        public NativeSlice<ulong> Indices => m_DataX.Indices.Slice(0,Length);
        
        NativeSparsePagedArray<float> m_DataX;
        NativeSparsePagedArray<float> m_DataY;
        NativeSparsePagedArray<float> m_DataZ;

        public NativeSparsePagedArrayXYZ(int allocationSize, int pageSize, int maximumPageCount, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
            m_DataX = new NativeSparsePagedArray<float>(allocationSize, pageSize, maximumPageCount, allocator);
            m_DataY = new NativeSparsePagedArray<float>(allocationSize, pageSize, maximumPageCount, allocator);
            m_DataZ = new NativeSparsePagedArray<float>(allocationSize, pageSize, maximumPageCount, allocator);
        }

        public bool ContainsNode(ulong sparseIndex)
        {
            return m_DataX.ContainsAllocation(sparseIndex);
        }

        public bool IsFull(ulong sparseIndex)
        {
            return m_DataX.GetAllocation(sparseIndex).IsFull;
        }
        
        public bool IsEmpty(ulong sparseIndex)
        {
            return m_DataX.GetAllocation(sparseIndex).Length==0;
        }

        public int GetLength(ulong sparseIndex)
        {
            return m_DataX.GetAllocation(sparseIndex).Length;
        }

        public void AddNode(ulong sparseIndex)
        {
            m_DataX.AddIndex(sparseIndex);
            m_DataY.AddIndex(sparseIndex);
            m_DataZ.AddIndex(sparseIndex);
        }
        
        public void Add(ulong sparseIndex, float3 point)
        {
            m_DataX.Add(sparseIndex, point.x);
            m_DataY.Add(sparseIndex, point.y);
            m_DataZ.Add(sparseIndex, point.z);
        }

        public void AddData(ulong sparseIndex, NativeArrayXYZ<float> data)
        {
            AddData(sparseIndex, data, data.Length);
        }
        
        public void AddData(ulong sparseIndex, NativeArrayXYZ<float> data, int count)
        {
            m_DataX.AddRange(sparseIndex, data.X, count);
            m_DataY.AddRange(sparseIndex, data.Y, count);
            m_DataZ.AddRange(sparseIndex, data.Z, count);
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            m_DataX.Dispose();
            m_DataY.Dispose();
            m_DataZ.Dispose();
        }
    }
}
