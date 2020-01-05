using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Assertions;

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
    public unsafe struct NativeSparseOctree<T> : IDisposable where T : unmanaged
    {
        const int MaximumLevelCount = 7; // Based on morton code limits

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif
        public bool IsCreated { get; private set; }
        public int LevelCount { get; private set; }

        Allocator m_Allocator;
        
        NativeSparseList<T> m_Level0;
        NativeSparseList<T> m_Level1;
        NativeSparseList<T> m_Level2;
        NativeSparseList<T> m_Level3;
        NativeSparseList<T> m_Level4;
        NativeSparseList<T> m_Level5;
        NativeSparseList<T> m_Level6;

        public NativeSparseOctree(Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent.
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));

            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
            m_Allocator = allocator;
            var level0 = new NativeSparseList<T>(OctreeUtils.GetNodeCount(0), allocator);
            //m_ppLevels = &(level0.GetUnsafePointer());
            
            LevelCount = 0;
            m_Level0 = default;
            m_Level1 = default;
            m_Level2 = default;
            m_Level3 = default;
            m_Level4 = default;
            m_Level5 = default;
            m_Level6 = default;
            IsCreated = true;
        }

        public void AddLevel()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (LevelCount == MaximumLevelCount - 1)
                throw new ArgumentOutOfRangeException();
#endif
            int nodeCount = OctreeUtils.GetNodeCount(LevelCount);
            var initialCapacity = math.min(1024, nodeCount);
            switch (LevelCount)
            {
                case 0:
                    m_Level0 = new NativeSparseList<T>(initialCapacity, Allocator.Persistent);
                    break;
                case 1:
                    m_Level1 = new NativeSparseList<T>(initialCapacity, Allocator.Persistent);
                    break;
                case 2:
                    m_Level2 = new NativeSparseList<T>(initialCapacity, Allocator.Persistent);
                    break;
                case 3:
                    m_Level3 = new NativeSparseList<T>(initialCapacity, Allocator.Persistent);
                    break;
                case 4:
                    m_Level4 = new NativeSparseList<T>(initialCapacity, Allocator.Persistent);
                    break;
                case 5:
                    m_Level5 = new NativeSparseList<T>(initialCapacity, Allocator.Persistent);
                    break;
                case 6:
                    m_Level6 = new NativeSparseList<T>(initialCapacity, Allocator.Persistent);
                    break;
                default:
                    Assert.IsTrue(false, "Internal error");
                    break;
            }

            LevelCount++;
        }


        public void Dispose()
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            if (m_Level0.IsCreated) m_Level0.Dispose();
            if (m_Level1.IsCreated) m_Level1.Dispose();
            if (m_Level2.IsCreated) m_Level2.Dispose();
            if (m_Level3.IsCreated) m_Level3.Dispose();
            if (m_Level4.IsCreated) m_Level4.Dispose();
            if (m_Level5.IsCreated) m_Level5.Dispose();
            if (m_Level6.IsCreated) m_Level6.Dispose();
            IsCreated = false;
        }
    }
}