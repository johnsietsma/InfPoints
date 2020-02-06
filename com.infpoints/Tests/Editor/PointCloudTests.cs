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
            var aabb = new AABB(float3.zero,10);
            using(var pointCloud = new PointCloud(aabb))
            using (var points = PointCloudGenerator.RandomPointsOnSphere(1024, 5, Allocator.TempJob))
            {
                pointCloud.AddPoints(points);
            }
        }
    }
}