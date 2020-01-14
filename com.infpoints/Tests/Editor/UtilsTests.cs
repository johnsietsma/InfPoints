using System;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace InfPoints.Tests.Editor
{
    public class UtilsTests
    {
        [Test]
        public void PointsToCoordsGivesCorrectResult()
        {
            AABB aabb = new AABB(0, 10);

            float3[] inPoints = new[]
            {
                new float3(0, 0, 0),
                new float3(-5, -5, -5),
                new float3(5, 5, 5),
            };

            uint3[] inCoords = new[]
            {
                new uint3(5, 5, 5),
                new uint3(0, 0, 0),
                new uint3(10, 10, 10),
            };

            const int cellCount = 10;
            for (int i = 0; i < inPoints.Length; i++)
            {
                Assert.That(() => Utils.PointToCoords(inPoints[i], cellCount, aabb), Is.EqualTo(inCoords[i]),
                    $"Index:{i}");
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
                Assert.That(() => Utils.PointToCoords(inPoints[i], cellCount, aabb),
                    Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
            }
        }
#endif
    }
}