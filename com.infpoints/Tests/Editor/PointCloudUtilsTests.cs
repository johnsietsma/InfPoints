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
            using (var mortonCodes = new NativeArray<ulong>(points.Length, Allocator.TempJob))
            {
                var pointsToCoordinatesJobHandle =
                    PointCloudUtils.SchedulePointsToCoordinates(points, coordinates, aabb.Minimum, cellSize);
                PointCloudUtils.ScheduleEncodeMortonCodes(coordinates, mortonCodes, pointsToCoordinatesJobHandle)
                    .Complete();
                for (int index = 0; index < mortonCodes.Length; index++)
                {
                    var code = mortonCodes[index];
                    Assert.That(code, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void GetUniqueCodesGivesTheCorrectResult()
        {
            ulong[] codesData = new ulong[] {1, 2, 2, 3, 4, 5, 5, 5};
            using (var codes = new NativeArray<ulong>(codesData, Allocator.TempJob))
            using (var codesUniqueMap = new NativeHashMap<ulong,uint>(codes.Length, Allocator.TempJob))
            using (var uniqueCodes = new NativeList<ulong>(Allocator.TempJob))
            {
                PointCloudUtils.ScheduleGetUniqueCodes(codes, codesUniqueMap, uniqueCodes).Complete();
                Assert.That(uniqueCodes.Length, Is.EqualTo(5));
            }
        }
        

        [Test]
        public void CanFilterWithEmptyNodeStorage()
        {
            const int cellSize = 10;
            var aabb = new AABB(float3.zero, cellSize);
            using (var points = PointCloudGenerator.RandomPointsOnSphere(1024, 5, Allocator.TempJob))
            using (var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob))
            using (var mortonCodes = new NativeArray<ulong>(points.Length, Allocator.TempJob))
            using (var nodeStorage = new NativeNodeStorage(1, 1, 1, Allocator.TempJob))
            {
                var pointsToCoordinatesJobHandle = PointCloudUtils.SchedulePointsToCoordinates(points, coordinates, aabb.Minimum, cellSize);
                PointCloudUtils.ScheduleEncodeMortonCodes(coordinates, mortonCodes, pointsToCoordinatesJobHandle).Complete();
                var filteredMortonCodeIndices = PointCloudUtils.FilterFullNodes(mortonCodes, nodeStorage);
                filteredMortonCodeIndices.Dispose();
            }
        }
    }
}