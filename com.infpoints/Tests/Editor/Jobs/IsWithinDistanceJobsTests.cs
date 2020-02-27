using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.PerformanceTesting;

namespace InfPoints.Tests.Editor.Jobs
{
    public class IsWithinDistanceJobsTests
    {
        NativeArray<float3> m_Points;
        NativeArrayXYZ<float> m_PointsXYZ;
        NativeInt m_WithinDistanceCount;

        [SetUp]
        public void Setup()
        {
            float3[] m_PointData = new float3[1024 * 1024 * 10];
            for (int index = 0; index < m_PointData.Length; index++)
            {
                m_PointData[index] = new float3(index, index + 1, index + 2);
            }

            m_Points = new NativeArray<float3>(m_PointData, Allocator.Persistent);
            m_PointsXYZ = NativeArrayXYZUtils.MakeNativeArrayXYZ(m_PointData, Allocator.Persistent);
            m_WithinDistanceCount = new NativeInt(Allocator.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            m_Points.Dispose();
            m_PointsXYZ.Dispose();
            m_WithinDistanceCount.Dispose();
        }


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
            using (var withinDistanceCount = new NativeInt(Allocator.TempJob))
            {
                new IsWithinDistanceJob(points, new float3(1, 0, 0), 1.1f, withinDistanceCount)
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
            using (var withinDistanceCount = new NativeInt(Allocator.TempJob))
            {
                var jobHandle =
                    new IsWithinDistanceJob_NativeArrayXYZ(points, new float3(1, 0, 0), 1.1f, withinDistanceCount);
                jobHandle.Schedule(jobHandle.Length, 4)
                    .Complete();

                Assert.That(withinDistanceCount.Value, Is.EqualTo(3));
            }
        }

        [Test, Performance]
        [Version("1")]
        public void IsWithinDistancePerformance_NativeArrayXYZ()
        {
            Measure.Method(RunIsWithinDistanceJob_NativeArrayXYZ).Run();
        }
        
        [Test, Performance]
        [Version("1")]
        public void IsWithinDistancePerformance_NativeArray()
        {
            Measure.Method(RunIsWithinDistanceJob_NativeArray).Run();
        }

        void RunIsWithinDistanceJob_NativeArray()
        {
            var jobHandle =
                new IsWithinDistanceJob(m_Points, new float3(1, 0, 0), 1.1f, m_WithinDistanceCount);
            jobHandle.Schedule(m_Points.Length, 4)
                .Complete();
        }
        
        void RunIsWithinDistanceJob_NativeArrayXYZ()
        {
            var jobHandle =
                new IsWithinDistanceJob_NativeArrayXYZ(m_PointsXYZ, new float3(1, 0, 0), 1.1f, m_WithinDistanceCount);
            jobHandle.Schedule(jobHandle.Length, 4)
                .Complete();
        }
    }
}