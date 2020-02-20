using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Count how many points are outside an AABB
    /// </summary>
    public struct CountPointsOutsideAABBJob : IJobParallelFor
    {
        [ReadOnly] public AABB aabb;
        [ReadOnly] public NativeArrayXYZ<float> Points;
        public NativeInt OutsideCount;

        public CountPointsOutsideAABBJob(AABB aabb, NativeArrayXYZ<float> points, NativeInt outsideCount)
        {
            this.aabb = aabb;
            Points = points;
            OutsideCount = outsideCount;
        }

        public void Execute(int index)
        {
            if(!aabb.Contains(Points.X[index], Points.Y[index], Points.Z[index]))
                OutsideCount.Increment();
        }
    }
}