using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints.Tests.Editor
{
    public class PointCloudTests
    {
        [Test]
        public void CanAddPointsToPointCloud()
        {
            const int PointCount = 64;
            var aabb = new AABB(float3.zero,10);
            using(var pointCloud = new PointCloud(aabb))
            using (var points = PointCloudGenerator.PointsInGrid(PointCount, 1, Allocator.TempJob))
            {
                pointCloud.AddPoints(points);
                var storage = pointCloud.Octree.GetNodeStorage(0);
                Assert.That(storage.Length, Is.EqualTo(PointCount));

                foreach (var index in storage.Indices)
                {
                    Assert.That(storage.GetLength(index), Is.EqualTo(1));
                }
            }
        }
    }
}