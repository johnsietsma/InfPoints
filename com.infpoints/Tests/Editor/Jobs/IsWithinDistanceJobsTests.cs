using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class IsWithinDistanceJobsTests
    {
        [Test]
        public void FindPointWithinDistanceCorrectly()
        {
            float3[] pointData = new[]
            {
                float3.zero,
                new float3(1),
                new float3(1, 0, 0),
                new float3(2, 0, 0),
                new float3(0, 2, 0),
                new float3(0, 0, 2),
                new float3(2.5f),
            };

            using (var points = new NativeArray<float3>(pointData, Allocator.TempJob))
            using(var withinDistanceCount = new NativeInt(Allocator.TempJob))
            {
                new IsWithinDistanceJob(points, new float3(1,0,0), 1.1f, withinDistanceCount)
                    .Schedule(points.Length, 4)
                    .Complete();
                
                Assert.That(withinDistanceCount.Value, Is.EqualTo(3));
            }
        }
        
        [Test]
        public void FindPointWithinDistanceCorrectly_NativeArrayXYZ()
        {
            float3[] pointData = new[]
            {
                float3.zero,
                new float3(1),
                new float3(1, 0, 0),
                new float3(2, 0, 0),
                new float3(0, 2, 0),
                new float3(0, 0, 2),
                new float3(2.5f),
                new float3(2.5f),
            };

            using (var points = NativeArrayXYZUtils.MakeNativeArrayXYZ(pointData, Allocator.TempJob))
            using(var withinDistanceCount = new NativeInt(Allocator.TempJob))
            {
                var jobHandle = new IsWithinDistanceJob_NativeArrayXYZ(points, new float3(1,0,0), 1.1f, withinDistanceCount);
                jobHandle.Schedule(jobHandle.Length, 4)
                    .Complete();
                
                Assert.That(withinDistanceCount.Value, Is.EqualTo(3));
            }
        }
        
    }
}