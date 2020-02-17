using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace InfPoints
{
    public static class PointCloudGenerator
    {
        public static NativeArrayXYZ<float> RandomPointsOnSphere(int pointCount, float radius, Allocator allocator)
        {
            Random rand = new Random();
            rand.InitState();
            var points = new NativeArrayXYZ<float>(pointCount, allocator);
            for (int index = 0; index < pointCount; index++)
            {
                var p = rand.NextFloat3Direction() * radius;
                points.X[index] = p.x;
                points.Y[index] = p.y;
                points.Z[index] = p.z;
            }

            return points;
        }

        public static NativeArrayXYZ<float> RandomPointsInAABB(int pointCount, AABB aabb, Allocator allocator)
        {
            Random rand = new Random();
            rand.InitState();
            var points = new NativeArrayXYZ<float>(pointCount, allocator);
            for (int index = 0; index < pointCount; index++)
            {
                var p = rand.NextFloat3(aabb.Minimum, aabb.Maximum);
                points.X[index] = p.x;
                points.Y[index] = p.y;
                points.Z[index] = p.z;
            }

            return points;
        }

        public static NativeArrayXYZ<float> PointsInGrid(int pointCount, float3 cellSize, Allocator allocator)
        {
            float3 offset = cellSize / 2;
            var points = new NativeArrayXYZ<float>(pointCount, allocator);
            for (int index = 0; index < pointCount; index++)
            {
                uint3 xyzIndex = Morton.DecodeMorton32((uint)index);
                float3 pos = offset + xyzIndex * cellSize;
                points.X[index] = pos.x;
                points.Y[index] = pos.y;
                points.Z[index] = pos.z;
            }

            return points;
        }
    }
}