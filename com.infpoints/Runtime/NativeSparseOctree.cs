using System;
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
    /// </summary>
    [NativeContainer]
    public struct NativeSparseOctree<T> : IDisposable where T: unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif
        
        public bool IsCreated => m_Levels.IsCreated;
        
        public int LevelCount { get; private set; }


        Allocator m_Allocator;
        NativeArray<NativeSparseArray<T>> m_Levels;

        public NativeSparseOctree(int maxLevelCount, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent.
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (maxLevelCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLevelCount), "Must be >= 0");
            
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
            m_Allocator = allocator;
            LevelCount = 0;
            m_Levels = new NativeArray<NativeSparseArray<T>>(maxLevelCount, allocator);
        }

        public void AddLevel(int maxNodeCountForLevel)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if( LevelCount==m_Levels.Length-1 )
                throw new ArgumentOutOfRangeException();
#endif            
        }

        public void Dispose()
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            m_Levels.Dispose();
        }
    }
}