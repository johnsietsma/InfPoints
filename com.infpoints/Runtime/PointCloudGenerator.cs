using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    public static class PointCloudGenerator
    {
        public static XYZSoA<float> RandomPointsOnSphere(int pointCount, float radius, Allocator allocator)
        {
            Random rand = new Random();
            rand.InitState();
            var points = new XYZSoA<float>(pointCount, allocator);
            for (int index = 0; index < pointCount; index++)
            {
                var p = rand.NextFloat3Direction() * radius;
                points.X[index] = p.x;
                points.Y[index] = p.y;
                points.Z[index] = p.z;
            }

            return points;
        }
        
        public static XYZSoA<float> RandomPointsInAABB(int pointCount, AABB aabb, Allocator allocator)
        {
            Random rand = new Random();
            rand.InitState();
            var points = new XYZSoA<float>(pointCount, allocator);
            for (int index = 0; index < pointCount; index++)
            {
                var p = rand.NextFloat3(aabb.Minimum, aabb.Maximum);
                points.X[index] = p.x;
                points.Y[index] = p.y;
                points.Z[index] = p.z;
            }

            return points;
        }
    }
}