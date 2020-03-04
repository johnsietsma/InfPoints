using InfPoints.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Calculate the number of points that can be added to a node.
    /// </summary>
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, CompileSynchronously = true)]
    public struct CalculatePointsToAddCount : IJob
    {
        [ReadOnly] NativeSparsePagedArrayXYZ m_Storage;
        [ReadOnly] readonly ulong m_MortonCode;
        [ReadOnly] readonly int m_MaximumPointsPerNode;
        [DeallocateOnJobCompletion] [ReadOnly] NativeInt m_RequestedPointCount;
        NativeInt m_CalculatedPointCount;


        /// <summary>
        /// The calculate the number of points that can be added to a node.
        /// </summary>
        /// <param name="storage">The storage that the points will be added to.</param>
        /// <param name="mortonCode">The morton code of the node in the storage.</param>
        /// <param name="requestedPointCount">The requested amount of points to add.</param>
        /// <param name="maximumPointsPerNode">The maximum number of points that can be added to a node.</param>
        /// <param name="calculatedPointCount">The result of the calculation, how many points to add to the node.</param>
        public CalculatePointsToAddCount(NativeSparsePagedArrayXYZ storage, ulong mortonCode,
            NativeInt requestedPointCount, int maximumPointsPerNode, NativeInt calculatedPointCount)
        {
            m_Storage = storage;
            m_MortonCode = mortonCode;
            m_CalculatedPointCount = calculatedPointCount;
            m_MaximumPointsPerNode = maximumPointsPerNode;
            m_RequestedPointCount = requestedPointCount;
        }

        public void Execute()
        {
            // The amount of points that can be added to the ndoe storage
            int availablePointCount = m_MaximumPointsPerNode - m_Storage.GetLength(m_MortonCode);
            m_CalculatedPointCount.Value = math.min(m_RequestedPointCount.Value, availablePointCount);
        }
    }
}