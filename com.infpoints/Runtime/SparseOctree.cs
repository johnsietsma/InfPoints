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

        public bool IsCreated => m_NodesStorage!=null;

        public int LevelCount => m_NodesStorage.Count;

        // ReSharper disable once InconsistentNaming
        public AABB AABB { get; private set; }
        
        readonly Allocator m_Allocator;
        List<NativeNodeStorage<T>> m_NodesStorage;

        public SparseOctree(AABB aabb, int maximumPointsPerNode, Allocator allocator)
        {
            AABB = aabb;
            m_Allocator = allocator;
            m_NodesStorage = new List<NativeNodeStorage<T>>(MaxLevelCount);
        }

        public static int GetNodeCount(int levelIndex)
        {
            return (int) math.pow(2, levelIndex);
        }

        public NativeNodeStorage<T> GetNodeStorage(int levelIndex)
        {
            return m_NodesStorage[levelIndex];
        }

        public void AddLevel(int maximumPointsPerNode)
        {
            var nodeCount = GetNodeCount(LevelCount);
            var nodeStorage = new NativeNodeStorage<T>(nodeCount, maximumPointsPerNode, m_Allocator);
            m_NodesStorage.Add(nodeStorage);
        }

        public void Dispose()
        {
            if( !IsCreated) throw new InvalidOperationException();
            
            for (int i = 0; i < LevelCount; i++)
            {
                m_NodesStorage[i].Dispose();
            }

            m_NodesStorage = null;
        }
    }
}