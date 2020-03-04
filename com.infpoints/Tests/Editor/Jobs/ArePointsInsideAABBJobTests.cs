using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Tests.Editor.Jobs
{
    public class ArePointsInsideAABBJobTests
    {
        [Test]
        public void CountsOutsidePointsCorrectly()
        {
            float3[] pointsArray = new[]
            {
                new float3(20, 20, 20),
                new float3(0, 0, 0),
                new float3(20, 23, 20),
            };
            AABB aabb = new AABB(20, 5);
            using (var xyzPoints = NativeArrayXYZUtils.MakeNativeArrayXYZ(pointsArray, Allocator.TempJob))
            using(var outsideCount = new NativeInt(0, Allocator.TempJob))
            {
                var pointsOutsideJob = new CountPointsOutsideAABBJob(aabb, xyzPoints, outsideCount)
                    .Schedule(xyzPoints.Length, 4);
                pointsOutsideJob.Complete();
                
                Assert.That(outsideCount.Value, Is.EqualTo(2));
            }
        }
    }
}