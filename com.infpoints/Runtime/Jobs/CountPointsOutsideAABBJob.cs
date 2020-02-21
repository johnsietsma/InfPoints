using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Count how many points are outside an AABB
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CountPointsOutsideAABBJob : IJobParallelFor
    {
        [ReadOnly] public AABB aabb;
        [ReadOnly] public NativeArrayXYZ<float> Points;
        public NativeInt.Concurrent OutsideCount;

        public CountPointsOutsideAABBJob(AABB aabb, NativeArrayXYZ<float> points, NativeInt outsideCount)
        {
            this.aabb = aabb;
            Points = points;
            OutsideCount = outsideCount.ToConcurrent();
        }

        public void Execute(int index)
        {
            if(!aabb.Contains(Points.X[index], Points.Y[index], Points.Z[index]))
                OutsideCount.Increment();
        }
    }
}