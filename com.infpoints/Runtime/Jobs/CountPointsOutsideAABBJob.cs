using InfPoints.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Count how many points are outside an AABB
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct CountPointsOutsideAABBJob : IJobParallelFor
    {
        [ReadOnly] AABB m_AABB;
        [ReadOnly] NativeArrayXYZ<float> m_Points;
        NativeInt.Concurrent m_OutsideCount;

        /// <summary>
        /// Calculate how many points are outside an AABB.
        /// </summary>
        /// <param name="aabb">The AABB to test against</param>
        /// <param name="points">The points to test</param>
        /// <param name="outsideCount">How many points are outside</param>
        public CountPointsOutsideAABBJob(AABB aabb, NativeArrayXYZ<float> points, NativeInt outsideCount)
        {
            m_AABB = aabb;
            m_Points = points;
            m_OutsideCount = outsideCount.ToConcurrent();
        }

        public void Execute(int index)
        {
            if(!m_AABB.Contains(m_Points.X[index], m_Points.Y[index], m_Points.Z[index]))
                m_OutsideCount.Increment();
        }
    }
}