using System;
using System.Collections.Generic;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

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
    public class SparseOctree : IDisposable
    {
        const int MaxLevelCount = 7;
        const int AllocationsPerPage =4;

        public bool IsCreated => m_NodeStoragePerLevel != null;

        public int LevelCount => m_NodeStoragePerLevel.Count;

        // ReSharper disable once InconsistentNaming
        public AABB AABB { get; }

        readonly Allocator m_Allocator;
        readonly int m_MaximumPointsPerNode;
        List<NativeSparsePagedArrayXYZ> m_NodeStoragePerLevel;

        public SparseOctree(AABB aabb, int maximumPointsPerNode, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (aabb.Extents <= 0) throw new ArgumentException("AABB must be bigger then 0", nameof(aabb));
            if (maximumPointsPerNode <= 0)
                throw new ArgumentException("Must be greater then 0", nameof(maximumPointsPerNode));
            if ((long)maximumPointsPerNode * AllocationsPerPage > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(maximumPointsPerNode), $"maximumPointsPerNode * AllocationsPerPage cannot exceed {int.MaxValue} bytes");

#endif
            AABB = aabb;
            m_MaximumPointsPerNode = maximumPointsPerNode;
            m_Allocator = allocator;
            m_NodeStoragePerLevel = new List<NativeSparsePagedArrayXYZ>(MaxLevelCount);
        }

        public NativeSparsePagedArrayXYZ GetNodeStorage(int levelIndex)
        {
            return m_NodeStoragePerLevel[levelIndex];
        }

        /// <summary>
        /// Add a new level to the SparseOctree
        /// </summary>
        /// <returns>The number of levels in the SparseOctree</returns>
        public int AddLevel()
        {
            var nodeCount = SparseOctreeUtils.GetNodeCount(LevelCount);
            int maximumPageCount = ((nodeCount * m_MaximumPointsPerNode) / AllocationsPerPage)+1;
            int allocationSize = m_MaximumPointsPerNode * UnsafeUtility.SizeOf<float>();

            var nodeStorage =
                new NativeSparsePagedArrayXYZ(allocationSize, AllocationsPerPage, maximumPageCount, m_Allocator);
            m_NodeStoragePerLevel.Add(nodeStorage);
            return m_NodeStoragePerLevel.Count;
        }

        public void Dispose()
        {
            if (!IsCreated) throw new InvalidOperationException();

            for (int i = 0; i < LevelCount; i++)
            {
                m_NodeStoragePerLevel[i].Dispose();
            }

            m_NodeStoragePerLevel = null;
        }
    }
}