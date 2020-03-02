using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IsWithinDistanceJob : IJob
    {
        [ReadOnly] int PointIndex;
        [ReadOnly] NativeArray<float> PointsX;
        [ReadOnly] NativeArray<float> PointsY;
        [ReadOnly] NativeArray<float> PointsZ;
        [ReadOnly] float DistanceSquared;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<int> PointIndices;

        public IsWithinDistanceJob(int pointIndex, NativeArray<int> pointIndices, NativeArrayXYZ<float> points, float distance)
        {
            PointIndex = pointIndex;
            PointIndices = pointIndices;
            PointsX = points.X;
            PointsY = points.Y;
            PointsZ = points.Z;
            DistanceSquared = math.pow(distance, 2);
        }

        public void Execute()
        {
            var point = new float3(PointsX[PointIndex], PointsY[PointIndex], PointsZ[PointIndex]);

            for (int index = 0; index < PointIndices.Length; index++)
            {
                var pointIndex = PointIndices[index];
                var testPoint = new float3(PointsX[pointIndex], PointsY[pointIndex], PointsZ[pointIndex]);
                if (PointIndex != pointIndex && math.distancesq(point, testPoint) < DistanceSquared)
                {
                    PointIndices[index] = -1;
                }
            }
        }

        public static void BuildJobChain(NativeArrayXYZ<float> points, NativeArray<int> pointIndices)
        {
            var withinDistanceJobHandles = new NativeArray<JobHandle>(points.Length, Allocator.TempJob);
            for (int index = 0; index < pointIndices.Length; index++)
            {
                withinDistanceJobHandles[index] = new IsWithinDistanceJob(index, pointIndices, points, 0.1f)
                    .Schedule();
                //withinDistanceJobHandles[index].Complete();
            }

            var withinDistanceJobHandle = JobHandle.CombineDependencies(withinDistanceJobHandles);
            withinDistanceJobHandle.Complete();
            withinDistanceJobHandles.Dispose();
        }
    }
}