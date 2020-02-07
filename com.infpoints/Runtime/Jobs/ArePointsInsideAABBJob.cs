using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct ArePointsInsideAABBJob : IJobParallelFor
    {
        [ReadOnly] public AABB aabb;
        [ReadOnly] public XYZSoA<float> Points;
        public NativeInt OutsideCount;

        public void Execute(int index)
        {
            if(!aabb.Contains(Points.X[index], Points.Y[index], Points.Z[index]))
                OutsideCount.Increment();
        }
    }
}