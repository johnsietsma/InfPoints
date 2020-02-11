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
            using (var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob))
            {
                PointCloudUtils.SchedulePointsToCoordinates(points, coordinates, aabb.Minimum, cellSize).Complete();
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
            using (var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob))
            {
                PointCloudUtils.SchedulePointsToCoordinates(points, coordinates, aabb.Minimum, cellSize).Complete();
                var mortonCodes = PointCloudUtils.EncodeMortonCodes(points, coordinates);
                for (int index = 0; index < mortonCodes.Length; index++)
                {
                    var code = mortonCodes[index];
                    Assert.That(code, Is.EqualTo(0));
                }
                mortonCodes.Dispose();
            }
        }

        [Test]
        public void CanFilterWithEmptyNodeStorage()
        {
            const int cellSize = 10;
            var aabb = new AABB(float3.zero, cellSize);
            using (var points = PointCloudGenerator.RandomPointsOnSphere(1024, 5, Allocator.TempJob))
            using (var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob))
            using (var nodeStorage = new NativeNodeStorage(1, 1, 1, Allocator.TempJob))
            {
                PointCloudUtils.SchedulePointsToCoordinates(points, coordinates, aabb.Minimum, cellSize).Complete();
                var mortonCodes = PointCloudUtils.EncodeMortonCodes(points, coordinates);
                var filteredMortonCodeIndices = PointCloudUtils.FilterFullNodes(mortonCodes, nodeStorage);
            }
        }
    }
}