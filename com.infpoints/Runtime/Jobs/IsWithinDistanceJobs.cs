using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Find all the points within a certain distance of a point.
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IsWithinDistanceJob : IJob
    {
        [ReadOnly] readonly int m_PointIndex;
        [ReadOnly] NativeArray<float> m_PointsX;
        [ReadOnly] NativeArray<float> m_PointsY;
        [ReadOnly] NativeArray<float> m_PointsZ;
        [ReadOnly] readonly float m_DistanceSquared;

        [NativeDisableContainerSafetyRestriction] [ReadOnly]
        NativeInt m_PointCount;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<int> m_PointIndices;

        /// <summary>
        /// Find all the points with a a distance.
        /// The points within distance have their index set to -1.
        /// The point count is decremented for each point within distance. When the job is finished it will have the
        /// total number of points that are outside of the distance from the test point.
        /// </summary>
        /// <param name="pointIndex">The index of the point to test</param>
        /// <param name="pointIndices">The indices of the the points</param>
        /// <param name="points">The points to test against</param>
        /// <param name="pointCount">The total number of points in the points array, this will contain the number of points that are out of distance</param>
        /// <param name="distance">The distance the points need to be from each other</param>
        public IsWithinDistanceJob(int pointIndex, NativeArray<int> pointIndices, NativeArrayXYZ<float> points,
            NativeInt pointCount, float distance)
        {
            m_PointIndex = pointIndex;
            m_PointIndices = pointIndices;
            m_PointsX = points.X;
            m_PointsY = points.Y;
            m_PointsZ = points.Z;
            m_DistanceSquared = math.pow(distance, 2);
            m_PointCount = pointCount;
        }

        public void Execute()
        {
            var point = new float3(m_PointsX[m_PointIndex], m_PointsY[m_PointIndex], m_PointsZ[m_PointIndex]);
            int length = m_PointCount.Value;
            for (int index = 0; index < length; index++)
            {
                var pointIndex = m_PointIndices[index];
                var testPoint = new float3(m_PointsX[pointIndex], m_PointsY[pointIndex], m_PointsZ[pointIndex]);
                if (m_PointIndex == pointIndex || !(math.distancesq(point, testPoint) < m_DistanceSquared)) continue;
                m_PointIndices[index] = -1;
                m_PointCount.Decrement();
            }
        }

        /// <summary>
        /// Create and schedule the jobs to test each point against every other point.
        /// </summary>
        /// <param name="points">The points to test against each other</param>
        /// <param name="pointCount">The number of points in the points array</param>
        /// <param name="pointIndices">The indices of the points</param>
        /// <param name="deps">Any job dependency</param>
        /// <returns>A handle for all the scheduled jobs</returns>
        public static JobHandle BuildJobChain(NativeArrayXYZ<float> points, NativeInt pointCount,
            NativeArray<int> pointIndices, JobHandle deps = default)
        {
            var withinDistanceJobHandles = new NativeArray<JobHandle>(pointIndices.Length, Allocator.TempJob);
            for (int index = 0; index < pointIndices.Length; index++)
            {
                withinDistanceJobHandles[index] =
                    new IsWithinDistanceJob(index, pointIndices, points, pointCount, 0.1f)
                        .Schedule(deps);
            }

            var withinDistanceJobHandle = JobHandle.CombineDependencies(withinDistanceJobHandles);
            return new DeallocateNativeArrayJob<JobHandle>(withinDistanceJobHandles).Schedule(withinDistanceJobHandle);
        }
    }
}