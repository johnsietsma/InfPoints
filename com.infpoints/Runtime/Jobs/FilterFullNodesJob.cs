using InfPoints.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Remove any nodes that are full in storage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct FilterFullNodesJob : IJob
    {
        [ReadOnly] NativeSparsePagedArrayXYZ m_Storage;
        NativeSparseArray<ulong,int> m_MortonCodes;

        /// <summary>
        /// Remove any nodes that are full.
        /// The codes list has the codes and their counts. The counts are not used.
        /// </summary>
        /// <param name="storage">The storage that keeps track of nodes allocations</param>
        /// <param name="mortonCodes">The codes to filter</param>
        public FilterFullNodesJob(NativeSparsePagedArrayXYZ storage, NativeSparseArray<ulong,int> mortonCodes)
        {
            m_Storage = storage;
            m_MortonCodes = mortonCodes;
        }
        
        public void Execute()
        {
            var indices = m_MortonCodes.Indices;
            for (int index = 0; index < m_MortonCodes.Length; index++)
            {
                ulong code = indices[index];
                if (m_Storage.ContainsNode(code) && m_Storage.IsFull(code))
                    m_MortonCodes.RemoveAtSwapBack(code);
            }
        }
    }
}