
namespace InfPoints.Octree.Tests.Editor
{
    using Morton;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine.TestTools.Constraints;
    using System.Diagnostics.CodeAnalysis;
    using Is = UnityEngine.TestTools.Constraints.Is;

    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
    public class MortonTests
    {
        static readonly int NUM_COORDS = 1000000;

        private static uint3[] WellKnownCoordinates { get; } =
        {
            new uint3(0, 0, 0),
            new uint3(1, 0, 0),
            new uint3(1, 0, 1),
            new uint3(5, 9, 1)
        };

        static readonly uint[] WellKnownCodes = {
            0,
            1,
            5,
            1095
        };


        private readonly uint3[] _RandomCoordinates = new uint3[NUM_COORDS];
        private readonly uint[] _RandomCodes = new uint[NUM_COORDS];

        [SetUp]
        public void SetupTests()
        {
            var r = new Unity.Mathematics.Random(1);
            for (int i = 0; i < _RandomCoordinates.Length; i++)
            {
                _RandomCoordinates[i] = r.NextUInt3();
            }

            for (int i = 0; i < _RandomCoordinates.Length; i++)
            {
                _RandomCodes[i] = Morton.EncodeMorton3(_RandomCoordinates[i]);
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
            for (int i = 0; i < WellKnownCodes.Length; i++)
            {
                var code = Morton.EncodeMorton3(WellKnownCoordinates[i]);
                var decodedCoordinate = Morton.DecodeMorton3(code);

                Assert.AreEqual(code, WellKnownCodes[i]);
                Assert.AreEqual(WellKnownCoordinates[i], decodedCoordinate);
            }
        }

        [Test]
        public void WellKnowNumbers_Packed()
        {
            var packedCoordinatesIn = new uint3x4(WellKnownCoordinates[0], WellKnownCoordinates[1], WellKnownCoordinates[2], WellKnownCoordinates[3]);
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
                }, NUnit.Framework.Is.Not.AllocatingGCMemory());
        }


        [Test]
        public void MortonJobs()
        {
            NativeArray<uint3> coordinates = new NativeArray<uint3>(
                _RandomCoordinates,
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
            void EncodeArray()
            {
                for (int i = 0; i < _RandomCoordinates.Length; i++)
                {
                    _RandomCodes[i] = Morton.EncodeMorton3(_RandomCoordinates[i]);
                }
            }

            Measure.Method(EncodeArray).MeasurementCount(20).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_Decode()
        {
            void DecodeArray()
            {
                for (int i = 0; i < _RandomCoordinates.Length; i++)
                {
                    _RandomCoordinates[i] = Morton.DecodeMorton3(_RandomCodes[i]);
                }
            }

            Measure.Method(DecodeArray).MeasurementCount(20).WarmupCount(5).Run();
        }
    }

}