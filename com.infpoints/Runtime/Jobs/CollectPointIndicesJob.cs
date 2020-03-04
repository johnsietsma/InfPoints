﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Copy(collect) all the indices of points matching the code. 
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CollectPointIndicesJob : IJob
    {
        [ReadOnly] readonly ulong m_MortonCode;
        [ReadOnly] readonly NativeArray<ulong> m_PointCodes;
        NativeArray<int> m_CollectedPointsIndices;
        NativeInt m_CollectedPointsCount;

        /// <summary>
        /// Copy all point indices that belong to a node.
        /// </summary>
        /// <param name="mortonCode">The morton code of the node</param>
        /// <param name="pointCodes">The morton codes of all the points</param>
        /// <param name="collectedPointIndices">The indices that match the code</param>
        /// <param name="collectedPointsCount">The number of points collected</param>
        public CollectPointIndicesJob(ulong mortonCode, NativeArray<ulong> pointCodes,
            NativeArray<int> collectedPointIndices, NativeInt collectedPointsCount)
        {
            m_MortonCode = mortonCode;
            m_PointCodes = pointCodes;
            m_CollectedPointsIndices = collectedPointIndices;
            m_CollectedPointsCount = collectedPointsCount;
        }

        public void Execute()
        {
            int count = 0;
            for (int index = 0; index < m_PointCodes.Length; index++)
            {
                if (m_PointCodes[index].Equals(m_MortonCode))
                {
                    m_CollectedPointsIndices[count] = index;
                    count++;
                }
            }
            m_CollectedPointsCount.Value = count;
            Logger.LogFormat(LogMessage.PointsCollected, count);
        }
    }
}