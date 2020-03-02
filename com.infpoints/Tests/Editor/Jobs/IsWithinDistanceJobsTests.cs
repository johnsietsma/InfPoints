using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class IsWithinDistanceJobsTests
    {
        static float3[] pointData = new[]
        {
            float3.zero,
            new float3(1),
            new float3(1, 0, 0),
            new float3(2, 0, 0),
            new float3(0, 2, 0),
            new float3(0, 0, 2),
            new float3(2.5f),
        };

        static int[] pointIndexData = new[] {0, 1, 2, 3, 4, 5, 6};

        NativeArrayXYZ<float> points = NativeArrayXYZUtils.MakeNativeArrayXYZ(pointData, Allocator.TempJob);
        NativeArray<int> pointIndices = new NativeArray<int>(pointIndexData, Allocator.TempJob);

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
            points.Dispose();
            pointIndices.Dispose();
        }

        [Test]
        public void FindPointWithinDistanceCorrectly()
        {
            new IsWithinDistanceJob(2, pointIndices, points, 1.1f)
                .Schedule()
                .Complete();

            Assert.That(pointIndices[0], Is.EqualTo(-1));
            Assert.That(pointIndices[1], Is.GreaterThanOrEqualTo(0));
            Assert.That(pointIndices[2], Is.GreaterThanOrEqualTo(0));
            Assert.That(pointIndices[3], Is.EqualTo(-1));
            Assert.That(pointIndices[4], Is.GreaterThanOrEqualTo(0));
            Assert.That(pointIndices[5], Is.GreaterThanOrEqualTo(0));
            Assert.That(pointIndices[6], Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void BuildsJobChainCorrectly()
        {
            IsWithinDistanceJob.BuildJobChain(points, pointIndices);
        }
    }
}