using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Copy all the points by index. 
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CollectPointsJob : IJob
    {
        [ReadOnly] public NativeList<int> CollectedPointsIndices;
        [ReadOnly] public NativeArray<float> PointsX;
        [ReadOnly] public NativeArray<float> PointsY;
        [ReadOnly] public NativeArray<float> PointsZ;
        public NativeArray<float> CollectedPointsX;
        public NativeArray<float> CollectedPointsY;
        public NativeArray<float> CollectedPointsZ;

        public CollectPointsJob(NativeList<int> collectedPointIndices, NativeArrayXYZ<float> points,
            NativeArrayXYZ<float> collectedPoints)
        {
            CollectedPointsIndices = collectedPointIndices;
            PointsX = points.X;
            PointsY = points.Y;
            PointsZ = points.Z;
            CollectedPointsX = collectedPoints.X;
            CollectedPointsY = collectedPoints.Y;
            CollectedPointsZ = collectedPoints.Z;
        }

        public void Execute()
        {
            for (int index = 0; index < CollectedPointsIndices.Length; index++)
            {
                var pointIndex = CollectedPointsIndices[index];
                CollectedPointsX[index] = PointsX[pointIndex];
                CollectedPointsY[index] = PointsY[pointIndex];
                CollectedPointsZ[index] = PointsZ[pointIndex];
            }
        }
    }
}