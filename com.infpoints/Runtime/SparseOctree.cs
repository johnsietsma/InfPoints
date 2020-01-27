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
            
        public bool IsCreated => m_Levels != null;

        public int LevelCount { get; private set; }
        
        // ReSharper disable once InconsistentNaming
        public AABB AABB { get; private set; }

        readonly Allocator m_Allocator;
        List<NativeSparseArray<T>> m_Levels;
        NodePointsMap m_NodesPointsMap;

        public SparseOctree(AABB aabb, Allocator allocator)
        {
            AABB = aabb;
            m_Allocator = allocator;
            m_Levels = new List<NativeSparseArray<T>>(MaxLevelCount);
            LevelCount = 0;
            m_NodesPointsMap= new NodePointsMap(1, allocator);
        }
        
        public static int GetCellCount(int levelIndex)
        {
            return (int)math.pow(2, levelIndex);
        }

        public void AddLevel(int maxNodeCountForLevel)
        {
            m_Levels.Add(new NativeSparseArray<T>(maxNodeCountForLevel, m_Allocator));
            LevelCount++;
        }

        public void Dispose()
        {
            if (m_Levels == null) throw new InvalidOperationException();
            
            for (int i = 0; i < LevelCount; i++)
            {
                m_Levels[i].Dispose();
            }

            LevelCount = 0;
            m_Levels = null;
            m_NodesPointsMap.Dispose();
        }


    }

}