using System;
using System.Collections.Generic;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    /// <summary>
    /// The first level of the Octree is the root node, which has an AABB which encapsulates the entire tree.
    /// Each level down has 8 nodes, each with its own AABB.
    /// The AABB of a node can be determined by the level and coords of a node. The only information that needs to be
    ///   stored for each node is its points.
    /// At deeper levels there may only be a few nodes. So a sparse array is used to hold the actual points that
    ///   belong to a node.
    /// This is not a Native Collection because Native Collections cannot contain other collections.
    /// </summary>
    public class SparseOctree<T> : IDisposable where T : unmanaged
    {
        public const int MaxLevelCount = 7;

        public bool IsCreated => m_NodesPerLevel != null;

        public int LevelCount { get; private set; }

        // ReSharper disable once InconsistentNaming
        public AABB AABB { get; private set; }

        const int NodesPerPage = 4;
        readonly int m_MaximumPointsPerNode;
        readonly int m_StoragePageSize;
        readonly Allocator m_Allocator;
        List<NativeSparseArray<T>> m_NodesPerLevel;
        List<NativeSparsePagedArray<T>> m_LevelStorage;

        public SparseOctree(AABB aabb, int maximumPointsPerNode, Allocator allocator)
        {
            AABB = aabb;
            m_MaximumPointsPerNode = maximumPointsPerNode;
            m_StoragePageSize = maximumPointsPerNode * NodesPerPage;
            m_Allocator = allocator;
            m_NodesPerLevel = new List<NativeSparseArray<T>>(MaxLevelCount);
            LevelCount = 0;
            m_LevelStorage = new List<NativeSparsePagedArray<T>>(MaxLevelCount);
        }

        public static int GetNodeCount(int levelIndex)
        {
            return (int) math.pow(2, levelIndex);
        }

        public void AddLevel(int maximumPointsPerNode)
        {
            var nodeCount = GetNodeCount(LevelCount);
            m_NodesPerLevel.Add(new NativeSparseArray<T>(nodeCount, m_Allocator));
            int maximumPageCount = nodeCount / NodesPerPage;
            m_LevelStorage.Add(new NativeSparsePagedArray<T>(m_MaximumPointsPerNode, m_StoragePageSize,
                maximumPageCount, m_Allocator));
            LevelCount++;
        }

        public void Dispose()
        {
            if (m_NodesPerLevel == null) throw new InvalidOperationException();

            for (int i = 0; i < LevelCount; i++)
            {
                m_NodesPerLevel[i].Dispose();
                m_LevelStorage[i].Dispose();
            }

            LevelCount = 0;
            m_NodesPerLevel = null;
        }
    }
}