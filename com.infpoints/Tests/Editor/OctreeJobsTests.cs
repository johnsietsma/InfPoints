using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine;

namespace InfPoints.Tests.Editor
{
    public class OctreeJobsTests
    {
        private static readonly float3[] InsidePoints = new[]
        {
            new float3(0, 0, 0),
            new float3(-5, -5, -5),
            new float3(5, 5, 5),
            new float3(1, 1, 1),
        };

        private static readonly uint3[] InsideCoords = new[]
        {
            new uint3(5, 5, 5),
            new uint3(0, 0, 0),
            new uint3(10, 10, 10),
            new uint3(6, 6, 6),
        };

        const int CellCount = 10;
        static readonly AABB aabb = new AABB(0, 10);

        [Test]
        public void PointsToCoordsGivesCorrectResult()
        {
            using (var points =
                new NativeArray<float3>(InsidePoints, Allocator.Persistent))
            using (var coords =
                new NativeArray<uint3>(InsidePoints.Length, Allocator.Persistent))
            {
                var jobHandle =
                    OctreeJobs.ScheduleConvertPointsToCoordsJobs(points, coords, aabb, CellCount, 4);
                jobHandle.Complete();
                for (int i = 0; i < coords.Length; i++)
                {
                    Assert.That(coords[i], Is.EqualTo(InsideCoords[i]));
                }
            }
        }

        [Test]
        public void WidePointsToCoordsGivesCorrectResult()
        {
            using (var points =
                new NativeArray<float3>(InsidePoints, Allocator.Persistent).Reinterpret<float4x3>(
                    UnsafeUtility.SizeOf<float3>()))
            using (var coords =
                new NativeArray<uint4x3>(points.Length, Allocator.Persistent))
            using (var coordsCompare =
                new NativeArray<uint3>(InsideCoords, Allocator.Persistent).Reinterpret<uint4x3>(
                    UnsafeUtility.SizeOf<uint3>()))
            {
                var jobHandle =
                    OctreeJobs.ScheduleConvertPointsToCoordsJobs(points, coords, aabb, CellCount, 4);
                jobHandle.Complete();
                for (int i = 0; i < coords.Length; i++)
                {
                    Assert.That(coords[i], Is.EqualTo(coordsCompare[i]));
                }
            }
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS

        [Test]
        public void PointsOutsideAABBThrowException()
        {
            AABB aabb = new AABB(0, 10);

            float3[] inPoints = new[]
            {
                new float3(-11, -5, -5),
                new float3(5, 5, 15),
            };

            //const int cellCount = 10;
            for (int i = 0; i < inPoints.Length; i++)
            {
                //Assert.That(() => Utils.PointToCoords(inPoints[i], cellCount, aabb),
                // Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            }
        }
#endif
    }
}