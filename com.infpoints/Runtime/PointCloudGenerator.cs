using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    public static class PointCloudGenerator
    {
        public static XYZSoA<float> Sphere(int pointCount, float radius)
        {
            Random rand = new Random();
            rand.InitState();
            var points = new XYZSoA<float>(pointCount, Allocator.TempJob);
            for (int index = 0; index < pointCount; index++)
            {
                var p = rand.NextFloat3Direction() * radius;
                points.X[index] = p.x;
                points.Y[index] = p.y;
                points.Z[index] = p.z;
            }

            return points;
        }
    }
}