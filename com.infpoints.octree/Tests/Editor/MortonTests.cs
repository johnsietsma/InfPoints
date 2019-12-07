namespace InfPoints.Octree.Tests.Editor
{
    using InfPoints.Octree.Morton;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine.TestTools.Constraints;
    using Is = UnityEngine.TestTools.Constraints.Is;

    public class MortonTests
    {
        static readonly int NUM_COORDS = 1000000;
        private uint3[] m_RandomCoordinates = new uint3[NUM_COORDS];
        private uint[] m_RandomCodes = new uint[NUM_COORDS];

        [SetUp]
        public void SetupTests()
        {
            Random r = new Random(1);
            for( int i=0;i< m_RandomCoordinates.Length; i++)
            {
                m_RandomCoordinates[i] = r.NextUInt3();
            }

            for (int i = 0; i < m_RandomCoordinates.Length; i++)
            {
                m_RandomCodes[i] = Morton.EncodeMorton3(m_RandomCoordinates[i]);
            }
        }

        [Test]
        public void ConvertCoordinatesToWellKnownCodes()
        {
            TestEncoding(new uint3(0), 0);
            TestEncoding(new uint3(1, 0, 1), 5);
        }

        [Test]
        public void NoAllocation()
        {
            Assert.That(
                () => {
                    Morton.EncodeMorton3(new uint3(0));
                    Morton.DecodeMorton3(0);
                }, Is.Not.AllocatingGCMemory() );
        }


        [Test]
        public void MortonJobs()
        {
            NativeArray<uint3> coordinates = new NativeArray<uint3>(
                m_RandomCoordinates,
                Allocator.TempJob);
            NativeArray<uint> codes = new NativeArray<uint>(coordinates.Length, Allocator.TempJob);

            MortonEncodeJob job = new MortonEncodeJob() {
                m_Coordinates = coordinates,
                m_Codes = codes
            };

            job.Execute();

            coordinates.Dispose();
            codes.Dispose();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_Encode()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < m_RandomCoordinates.Length; i++)
                {
                    m_RandomCodes[i] = Morton.EncodeMorton3(m_RandomCoordinates[i]);
                }
            }).MeasurementCount(20).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_Decode()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < m_RandomCoordinates.Length; i++)
                {
                    m_RandomCoordinates[i] = Morton.DecodeMorton3(m_RandomCodes[i]);
                }
            }).MeasurementCount(20).WarmupCount(5).Run();
        }


        private void TestEncoding(uint3 coordinate, uint expectedCode)
        {
            var code = Morton.EncodeMorton3(coordinate);
            var decodedCoorindate = Morton.DecodeMorton3(code);

            Assert.AreEqual(code, expectedCode);
            Assert.AreEqual(coordinate, decodedCoorindate);
        }


    }

}