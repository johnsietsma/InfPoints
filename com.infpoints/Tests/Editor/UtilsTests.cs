using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine;

namespace InfPoints.Tests.Editor
{
    public class UtilsTests
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
            for (int i = 0; i < InsidePoints.Length; i++)
            {
                var coord = OctreeUtils.ConvertPointToCoord(aabb, CellCount, InsidePoints[i]);
                Assert.That(coord, Is.EqualTo(InsideCoords[i]));
            }
        }

        [Test]
        public void WidePointsToCoordsGivesCorrectResult()
        {
            using (var points =
                new NativeArray<float3>(InsidePoints, Allocator.Persistent).Reinterpret<float4x3>(
                    UnsafeUtility.SizeOf<float3>()))
            using (var coords =
                new NativeArray<uint3>(InsideCoords, Allocator.Persistent).Reinterpret<uint4x3>(
                    UnsafeUtility.SizeOf<uint3>()))
            {
                for (int i = 0; i < points.Length; i++)
                {
                    //var c = Utils.PointToCoords(points[i], CellCount, aabb);
                    //Assert.That(c, Is.EqualTo(coords[i]));
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

            const int cellCount = 10;
            for (int i = 0; i < inPoints.Length; i++)
            {
                //Assert.That(() => Utils.PointToCoords(inPoints[i], cellCount, aabb),
                   // Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            }
        }
#endif
    }
}