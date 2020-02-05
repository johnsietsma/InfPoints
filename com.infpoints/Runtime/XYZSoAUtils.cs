using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    public static class XYZSoAUtils
    {
        public static XYZSoA<float> MakeXYZSoA(NativeArray<float3> points, Allocator allocator)
        {
            var xyzPoints = new XYZSoA<float>(points.Length, allocator);
            for (int index = 0; index < points.Length; index++)
            {
                var p = points[index];
                xyzPoints.X[index] = p.x;
                xyzPoints.Y[index] = p.y;
                xyzPoints.Z[index] = p.z;
            }

            return xyzPoints;
        }
    }
}