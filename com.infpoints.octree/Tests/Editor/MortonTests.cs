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
            Assert.AreEqual(Morton.DecodeMorton3(0), new uint3(0, 0, 0));
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
                var decodedCoorindate = Morton.DecodeMorton3(code);

                Assert.AreEqual(code, WELL_KNOWN_CODES[i]);
                Assert.AreEqual(WELL_KNOWN_COORDINATES[i], decodedCoorindate);
            }
        }

        uint4x3 PackCoordinates(uint3 coordinates0, uint3 coordinates1, uint3 coordinates2, uint3 coordinates3)
        {
            uint4 x = new uint4(coordinates0.x, coordinates1.x, coordinates2.x, coordinates3.x);
            uint4 y = new uint4(coordinates0.y, coordinates1.y, coordinates2.y, coordinates3.y);
            uint4 z = new uint4(coordinates0.z, coordinates1.z, coordinates2.z, coordinates3.z);

            return new uint4x3(x, y, z);
        }

        uint3x4 UnpackCoordinates(uint4x3 packedCoordinates)
        {
            uint4 coordinatesX = packedCoordinates[0];
            uint4 coordinatesY = packedCoordinates[1];
            uint4 coordinatesZ = packedCoordinates[2];

            uint3 coordinates0 = new uint3(coordinatesX[0], coordinatesY[0], coordinatesZ[0]);
            uint3 coordinates1 = new uint3(coordinatesX[1], coordinatesY[1], coordinatesZ[1]);
            uint3 coordinates2 = new uint3(coordinatesX[2], coordinatesY[2], coordinatesZ[2]);
            uint3 coordinates3 = new uint3(coordinatesX[3], coordinatesY[3], coordinatesZ[3]);

            return new uint3x4(coordinates0, coordinates1, coordinates2, coordinates3);
        }

        [Test]
        public void WellKnowNumbers_Horizontal()
        {
            var packedCoordinates = PackCoordinates(WELL_KNOWN_COORDINATES[0], WELL_KNOWN_COORDINATES[1], WELL_KNOWN_COORDINATES[2], WELL_KNOWN_COORDINATES[3]);
            var codes = Morton.EncodeMorton3(packedCoordinates[0], packedCoordinates[1], packedCoordinates[2]);

            var coordinates = Morton.DecodeMorton3(codes);

            uint4 decodedX = coordinates[0];
            uint4 decodedY = coordinates[1];
            uint4 decodedZ = coordinates[2];

            uint3 decoded0 = new uint3(decodedX[0], decodedY[0], decodedZ[0]);
            uint3 decoded1 = new uint3(decodedX[1], decodedY[1], decodedZ[1]);
            uint3 decoded2 = new uint3(decodedX[2], decodedY[2], decodedZ[2]);
            uint3 decoded3 = new uint3(decodedX[3], decodedY[3], decodedZ[3]);

            Assert.AreEqual(coord0, decoded0);
            Assert.AreEqual(coord1, decoded1);
            Assert.AreEqual(coord2, decoded2);
            Assert.AreEqual(coord3, decoded3);
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