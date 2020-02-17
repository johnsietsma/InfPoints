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
        public NativeArray<ulong> Indices => m_DataX.Indices;
        
        NativeSparsePagedArray<float> m_DataX;
        NativeSparsePagedArray<float> m_DataY;
        NativeSparsePagedArray<float> m_DataZ;

        public NativeSparsePagedArrayXYZ(int maximumNodeCount, int maximumPointsPerNode, int nodesPerPage, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
            var storagePageSize = maximumPointsPerNode * nodesPerPage;
            int maximumPageCount = maximumNodeCount / nodesPerPage;

            m_DataX = new NativeSparsePagedArray<float>(maximumPointsPerNode, storagePageSize, maximumPageCount,
                allocator);
            m_DataY = new NativeSparsePagedArray<float>(maximumPointsPerNode, storagePageSize, maximumPageCount,
                allocator);
            m_DataZ = new NativeSparsePagedArray<float>(maximumPointsPerNode, storagePageSize, maximumPageCount,
                allocator);
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
            m_DataX.AddRange(sparseIndex, data.X);
            m_DataY.AddRange(sparseIndex, data.Y);
            m_DataZ.AddRange(sparseIndex, data.Z);
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
            if (!m_DataX.IsCreated) throw new InvalidOperationException();
#endif
            m_DataX.Dispose();
            m_DataY.Dispose();
            m_DataZ.Dispose();
        }
    }
}