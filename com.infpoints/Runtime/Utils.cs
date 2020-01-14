using System;
using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    public class Utils
    {
        public static uint3 PointToCoords(float3 point, int cellCount, AABB aabb)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!aabb.Contains(point)) throw new ArgumentOutOfRangeException($"Point:{point} is not inside AABB:{aabb}");
#endif

            float cellSize = aabb.Size / cellCount;
            point -= aabb.Minimum;
            uint3 coord = new uint3(
                (uint) math.floor(point.x / cellSize),
                (uint) math.floor(point.y / cellSize),
                (uint) math.floor(point.z / cellSize)
            );

            return coord;
        }

        public static void PointToCoords(NativeArray<float4x3> points, int cellCount, AABB aabb,
            NativeArray<uint4x3> coords)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (points.Length != coords.Length) throw new ArgumentException();
#endif

            for (int i = 0; i < points.Length; i++)
            {
                float4x3 point = points[i];
                float4 x = point[0];
                float4 y = point[1];
                float4 z = point[2];


#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (!aabb.Contains(x, y, z)) throw new ArgumentOutOfRangeException();
#endif

                float cellSize = aabb.Size / cellCount;

                uint4 coordX = (uint4) math.floor(x / cellSize);
                uint4 coordY = (uint4) math.floor(y / cellSize);
                uint4 coordZ = (uint4) math.floor(z / cellSize);

                coords[i] = new uint4x3(coordX,coordY,coordZ);
            }
        }
    }
}