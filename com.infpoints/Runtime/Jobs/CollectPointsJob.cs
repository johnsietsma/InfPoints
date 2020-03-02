using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Copy all the points by index. 
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CollectPointsJob : IJob
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> CollectedPointsIndices;
        [ReadOnly] public NativeArray<float> PointsX;
        [ReadOnly] public NativeArray<float> PointsY;
        [ReadOnly] public NativeArray<float> PointsZ;
        [ReadOnly] NativeInt MaximumPointCount;
        public NativeArray<float> CollectedPointsX;
        public NativeArray<float> CollectedPointsY;
        public NativeArray<float> CollectedPointsZ;

        public CollectPointsJob(NativeArray<int> collectedPointIndices, NativeArrayXYZ<float> points,
            NativeArrayXYZ<float> collectedPoints, NativeInt maximumPointCount)
        {
            CollectedPointsIndices = collectedPointIndices;
            PointsX = points.X;
            PointsY = points.Y;
            PointsZ = points.Z;
            MaximumPointCount = maximumPointCount;
            CollectedPointsX = collectedPoints.X;
            CollectedPointsY = collectedPoints.Y;
            CollectedPointsZ = collectedPoints.Z;
        }

        public void Execute()
        {
            var length = math.min(MaximumPointCount.Value, CollectedPointsIndices.Length);
            for (int index = 0; index < length; index++)
            {
                var pointIndex = CollectedPointsIndices[index];
                CollectedPointsX[index] = PointsX[pointIndex];
                CollectedPointsY[index] = PointsY[pointIndex];
                CollectedPointsZ[index] = PointsZ[pointIndex];
            }
        }
    }
}