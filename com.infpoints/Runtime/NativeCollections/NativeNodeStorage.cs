using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    [NativeContainer]
    public struct NativeNodeStorage<T> : IDisposable where T : unmanaged
    {
        public bool IsCreated => m_Nodes.IsCreated;
        public int Length => m_Nodes.Length;
        public NativeSparseArray<T> Nodes => m_Nodes;
        public NativeSparsePagedArray<T> Storage => m_Storage;

        NativeSparseArray<T> m_Nodes;
        NativeSparsePagedArray<T> m_Storage;

        const int NodesPerPage = 4;

        public NativeNodeStorage(int maximumNodeCount, int maximumPointsPerNode, Allocator allocator)
        {
            var storagePageSize = maximumPointsPerNode * NodesPerPage;
            int maximumPageCount = maximumNodeCount / NodesPerPage;

            m_Nodes = new NativeSparseArray<T>(maximumNodeCount, allocator);
            m_Storage = new NativeSparsePagedArray<T>(maximumPointsPerNode, storagePageSize, maximumPageCount,
                allocator);
        }

        public bool IsFull(ulong mortonCode)
        {
            return Storage.IsFull(mortonCode);
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!m_Nodes.IsCreated) throw new InvalidOperationException();
#endif
            m_Nodes.Dispose();
            m_Storage.Dispose();
            m_Nodes = default;
            m_Storage = default;
        }
    }
}