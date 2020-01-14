using System;
using System.Collections.Generic;
using Unity.Collections;

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
        public bool IsCreated => m_Levels != null;

        public int LevelCount { get; private set; }
        
        // ReSharper disable once InconsistentNaming
        public AABB AABB { get; private set; }

        readonly Allocator m_Allocator;
        List<NativeSparseArray<T>> m_Levels;

        public SparseOctree(AABB aabb, int initialLevelCount, Allocator allocator)
        {
            AABB = aabb;
            m_Allocator = allocator;
            m_Levels = new List<NativeSparseArray<T>>(initialLevelCount);
            LevelCount = 0;
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
        }


    }

}