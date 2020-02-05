using NUnit.Framework;
using Unity.Mathematics;

namespace InfPoints.Tests.Editor
{
    public class PointCloudTests
    {
        [Test]
        public void CanAddPointsToPointCloud()
        {
            var aabb = new AABB(float3.zero,10);
            var pointCloud = new PointCloud(aabb);
            var points = PointCloudGenerator.Sphere(1024, 5);
            pointCloud.AddPoints( points );

            points.Dispose();
            pointCloud.Dispose();
        }
    }
}