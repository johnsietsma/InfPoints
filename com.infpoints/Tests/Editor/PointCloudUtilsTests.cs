using InfPoints.NativeCollections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints.Tests.Editor
{
    public class PointCloudUtilsTests
    {
        [Test]
        public void PointsToCoordinatesGivesTheCorrectResults()
        {
            const int cellSize = 10;
            var aabb = new AABB(float3.zero, cellSize);
            using (var points = PointCloudGenerator.RandomPointsOnSphere(1024, 5, Allocator.TempJob))
            using (var coordinates = PointCloudUtils.PointsToCoordinates(points, aabb.Minimum, cellSize))
            {
                for (int index = 0; index < coordinates.Length; index++)
                {
                    uint3 coord = new uint3(coordinates.X[index], coordinates.Y[index], coordinates.Z[index]);
                    Assert.That(coord, Is.EqualTo(uint3.zero));
                }
            }
        }

        [Test]
        public void EncodingGivesTheCorrectResult()
        {
            const int cellSize = 10;
            var aabb = new AABB(float3.zero, cellSize);
            using (var points = PointCloudGenerator.RandomPointsOnSphere(1024, 5, Allocator.TempJob))
            using (var coordinates = PointCloudUtils.PointsToCoordinates(points, aabb.Minimum, cellSize))
            using (var mortonCodes = PointCloudUtils.EncodeMortonCodes(points, coordinates))
            {
                for (int index = 0; index < mortonCodes.Length; index++)
                {
                    var code = mortonCodes[index];
                    Assert.That(code, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void CanFilterWithEmptyNodeStorage()
        {
            const int cellSize = 10;
            var aabb = new AABB(float3.zero, cellSize);
            using (var points = PointCloudGenerator.RandomPointsOnSphere(1024, 5, Allocator.TempJob))
            using (var coordinates = PointCloudUtils.PointsToCoordinates(points, aabb.Minimum, cellSize))
            using (var mortonCodes = PointCloudUtils.EncodeMortonCodes(points, coordinates))
            using (var nodeStorage = new NativeNodeStorage(1, 1, 1, Allocator.TempJob))
            {
                var filteredMortonCodeIndices = PointCloudUtils.FilterFullNodes(mortonCodes, nodeStorage);
            }
        }
    }
}