using InfPoints.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Copy all the points by index. 
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct CopyPointsByIndexJob : IJob
    {
        [DeallocateOnJobCompletion] [ReadOnly] NativeArray<int> m_CollectedPointsIndices;
        NativeArray<float> m_PointsX;
        NativeArray<float> m_PointsY;
        NativeArray<float> m_PointsZ;
        [ReadOnly] NativeInt m_MaximumPointCount;
        NativeArray<float> m_CollectedPointsX;
        NativeArray<float> m_CollectedPointsY;
        NativeArray<float> m_CollectedPointsZ;

        /// <summary>
        /// Copy all the points by index into a contiguous array.
        /// The points are removed from the array, so just the un-copied points remain in a contiguous array.
        /// </summary>
        /// <param name="collectedPointIndices">The indices of the points to copy</param>
        /// <param name="points">The array of points to copy from</param>
        /// <param name="collectedPoints">The contiguous array of points</param>
        /// <param name="maximumPointCount">The most amount of points to copys</param>
        public CopyPointsByIndexJob(NativeArray<int> collectedPointIndices, NativeArrayXYZ<float> points,
            NativeArrayXYZ<float> collectedPoints, NativeInt maximumPointCount)
        {
            m_CollectedPointsIndices = collectedPointIndices;
            m_PointsX = points.X;
            m_PointsY = points.Y;
            m_PointsZ = points.Z;
            m_MaximumPointCount = maximumPointCount;
            m_CollectedPointsX = collectedPoints.X;
            m_CollectedPointsY = collectedPoints.Y;
            m_CollectedPointsZ = collectedPoints.Z;
        }

        public void Execute()
        {
            var length = math.min(m_MaximumPointCount.Value, m_CollectedPointsIndices.Length);
            for (int index = 0; index < length; index++)
            {
                var pointIndex = m_CollectedPointsIndices[index];
                m_CollectedPointsX[index] = m_PointsX[pointIndex];
                m_CollectedPointsY[index] = m_PointsY[pointIndex];
                m_CollectedPointsZ[index] = m_PointsZ[pointIndex];
                m_PointsX.RemoveAtSwapBack(pointIndex);
                m_PointsY.RemoveAtSwapBack(pointIndex);
                m_PointsZ.RemoveAtSwapBack(pointIndex);
            }
        }
    }
}