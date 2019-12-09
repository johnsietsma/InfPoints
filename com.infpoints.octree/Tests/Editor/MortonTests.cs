namespace InfPoints.Octree.Tests.Editor
{
    using InfPoints.Octree.Morton;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine;
    using UnityEngine.TestTools.Constraints;
    using Is = UnityEngine.TestTools.Constraints.Is;

    public class MortonTests
    {
        static readonly int NUM_COORDS = 1000000;
        static readonly uint3[] WELL_KNOWN_COORDINATES = new uint3[]
        {
            new uint3(0,0,0),
            new uint3(1,0,0),
            new uint3(1,0,1),
            new uint3(5,9,1)
        };
        static readonly uint[] WELL_KNOWN_CODES = new uint[]
        {
            0,
            1,
            5,
            1095
        };


        private uint3[] m_RandomCoordinates = new uint3[NUM_COORDS];
        private uint[] m_RandomCodes = new uint[NUM_COORDS];

        [SetUp]
        public void SetupTests()
        {
            var r = new Unity.Mathematics.Random(1);
            for (int i = 0; i < m_RandomCoordinates.Length; i++)
            {
                m_RandomCoordinates[i] = r.NextUInt3();
            }

            for (int i = 0; i < m_RandomCoordinates.Length; i++)
            {
                m_RandomCodes[i] = Morton.EncodeMorton3(m_RandomCoordinates[i]);
            }
        }

        [Test]
        public void First8()
        {
            Assert.AreEqual(expected: Morton.DecodeMorton3(0), actual: new uint3(0, 0, 0));
            Assert.AreEqual(Morton.DecodeMorton3(1), new uint3(1, 0, 0));
            Assert.AreEqual(Morton.DecodeMorton3(2), new uint3(0, 1, 0));
            Assert.AreEqual(Morton.DecodeMorton3(3), new uint3(1, 1, 0));
            Assert.AreEqual(Morton.DecodeMorton3(4), new uint3(0, 0, 1));
            Assert.AreEqual(Morton.DecodeMorton3(5), new uint3(1, 0, 1));
            Assert.AreEqual(Morton.DecodeMorton3(6), new uint3(0, 1, 1));
            Assert.AreEqual(Morton.DecodeMorton3(7), new uint3(1, 1, 1));
        }

        [Test]
        public void WellKnowNumbers()
        {
            for (int i = 0; i < WELL_KNOWN_CODES.Length; i++)
            {
                var coordinate = WELL_KNOWN_COORDINATES[i];
                var code = Morton.EncodeMorton3(WELL_KNOWN_COORDINATES[i]);
                var decodedCoordinate = Morton.DecodeMorton3(code);

                Assert.AreEqual(code, WELL_KNOWN_CODES[i]);
                Assert.AreEqual(WELL_KNOWN_COORDINATES[i], decodedCoordinate);
            }
        }

        [Test]
        public void WellKnowNumbers_Packed()
        {
            var packedCoordinatesIn = new uint3x4(WELL_KNOWN_COORDINATES[0], WELL_KNOWN_COORDINATES[1], WELL_KNOWN_COORDINATES[2], WELL_KNOWN_COORDINATES[3]);
            var packedCodes = Morton.EncodeMorton3(math.transpose((packedCoordinatesIn)));
            var packedCoordinatesOut = Morton.DecodeMorton3(packedCodes);

            Assert.AreEqual(packedCoordinatesIn[0], packedCoordinatesOut[0]);
            Assert.AreEqual(packedCoordinatesIn[1], packedCoordinatesOut[1]);
            Assert.AreEqual(packedCoordinatesIn[2], packedCoordinatesOut[2]);
            Assert.AreEqual(packedCoordinatesIn[3], packedCoordinatesOut[3]);
        }

        [Test]
        public void NoAllocation()
        {
            Assert.That(
                () =>
                {
                    Morton.EncodeMorton3(new uint3(0));
                    Morton.DecodeMorton3(0);
                }, Is.Not.AllocatingGCMemory());
        }


        [Test]
        public void MortonJobs()
        {
            NativeArray<uint3> coordinates = new NativeArray<uint3>(
                m_RandomCoordinates,
                Allocator.TempJob);
            NativeArray<uint> codes = new NativeArray<uint>(coordinates.Length, Allocator.TempJob);

            MortonEncodeJob job = new MortonEncodeJob()
            {
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