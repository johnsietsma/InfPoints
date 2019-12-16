using System;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace InfPoints.Octree.Tests.Editor
{
    using Morton;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.PerformanceTesting;
    using UnityEngine.TestTools.Constraints;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
    public class MortonTests
    {
        static readonly int NUM_COORDS = 1000000;

        private static uint3[] WellKnownCoordinates { get; } =
        {
            new uint3(0, 0, 0),
            new uint3(1, 0, 0),
            new uint3(1, 0, 1),
            new uint3(0b0101, 0b1001, 0b0001)
        };

        static readonly uint[] WellKnownCodes =
        {
            0,
            1,
            5,
            0b10001000111
        };

        private static uint3[] WellKnownCoordinates64 { get; } =
        {
            new uint3(0b001_001_001_001_001_001_001, 0b010_010_010_010_010_010_010, 0b100_100_100_100_100_100_100)
        };


        static readonly uint[] WellKnownCodes64 =
        {
            0b100_001_010_100_001_010_100
        };

        readonly uint3[] m_RandomCoordinates = new uint3[NUM_COORDS];
        readonly uint[] m_RandomCodes = new uint[NUM_COORDS];

        NativeArray<uint3> m_Coordinates;
        NativeArray<uint4x3> m_CoordinatesPacked;
        NativeArray<uint3> m_CoordinatesDecoded;
        NativeArray<uint4x3> m_CoordinatesDecodedPacked;
        NativeArray<uint> m_Codes;

        [SetUp]
        public void SetupTests()
        {
            var r = new Random(1);
            for (int i = 0; i < m_RandomCoordinates.Length; i++)
            {
                m_RandomCoordinates[i] = r.NextUInt3(Morton.MaxCoordinateValue32);
            }

            for (int i = 0; i < m_RandomCoordinates.Length; i++)
            {
                m_RandomCodes[i] = Morton.EncodeMorton32(m_RandomCoordinates[i]);
            }

            int coordinatesLength = m_RandomCoordinates.Length;
            int packedLength = coordinatesLength / 4;
            m_Coordinates = new NativeArray<uint3>(m_RandomCoordinates, Allocator.TempJob);
            m_CoordinatesPacked = new NativeArray<uint4x3>(packedLength, Allocator.TempJob);
            m_CoordinatesDecoded = new NativeArray<uint3>(coordinatesLength, Allocator.TempJob);
            m_CoordinatesDecodedPacked = new NativeArray<uint4x3>(packedLength, Allocator.TempJob);
            m_Codes = new NativeArray<uint>(coordinatesLength, Allocator.TempJob);
        }

        [TearDown]
        public void TearDown()
        {
            m_Coordinates.Dispose();
            m_CoordinatesPacked.Dispose();
            m_CoordinatesDecoded.Dispose();
            m_CoordinatesDecodedPacked.Dispose();
            m_Codes.Dispose();
        }

        [Test]
        public void First8()
        {
            Assert.AreEqual(Morton.DecodeMorton32(0), new uint3(0, 0, 0));
            Assert.AreEqual(Morton.DecodeMorton32(1), new uint3(1, 0, 0));
            Assert.AreEqual(Morton.DecodeMorton32(2), new uint3(0, 1, 0));
            Assert.AreEqual(Morton.DecodeMorton32(3), new uint3(1, 1, 0));
            Assert.AreEqual(Morton.DecodeMorton32(4), new uint3(0, 0, 1));
            Assert.AreEqual(Morton.DecodeMorton32(5), new uint3(1, 0, 1));
            Assert.AreEqual(Morton.DecodeMorton32(6), new uint3(0, 1, 1));
            Assert.AreEqual(Morton.DecodeMorton32(7), new uint3(1, 1, 1));
        }

        [Test]
        public void WellKnownNumbers()
        {
            for (int i = 0; i < WellKnownCodes.Length; i++)
            {
                var code = Morton.EncodeMorton32(WellKnownCoordinates[i]);
                var decodedCoordinate = Morton.DecodeMorton32(code);

                Assert.AreEqual(code, WellKnownCodes[i]);
                Assert.AreEqual(WellKnownCoordinates[i], decodedCoordinate);
            }
        }
        
        [Test]
        public void WellKnownNumbers64()
        {
            for (int i = 0; i < WellKnownCodes.Length; i++)
            {
                var code = Morton.EncodeMorton32(WellKnownCoordinates[i]);
                var decodedCoordinate = Morton.DecodeMorton32(code);

                Assert.AreEqual(code, WellKnownCodes[i]);
                Assert.AreEqual(WellKnownCoordinates[i], decodedCoordinate);
            }
            
            for (int i = 0; i < WellKnownCodes64.Length; i++)
            {
                var code = Morton.EncodeMorton64(WellKnownCoordinates[i]);
                var decodedCoordinate = Morton.DecodeMorton64(code);

                Assert.AreEqual(code, WellKnownCodes[i]);
                Assert.AreEqual(WellKnownCoordinates[i], decodedCoordinate);
            }
        }

        [Test]
        public void WellKnownNumbers_Packed()
        {
            var packedCoordinatesIn = new uint3x4(WellKnownCoordinates[0], WellKnownCoordinates[1],
                WellKnownCoordinates[2], WellKnownCoordinates[3]);
            var coordinates = math.transpose(packedCoordinatesIn);
            var packedCodes = Morton.EncodeMorton32(coordinates[0], coordinates[1], coordinates[2]);
            var packedCoordinatesOut = math.transpose(Morton.DecodeMorton32(packedCodes));

            Assert.AreEqual(packedCoordinatesIn, packedCoordinatesOut);
        }

        [Test]
        public void NoAllocation()
        {
            Assert.That(
                () =>
                {
                    Morton.EncodeMorton32(new uint3(0));
                    Morton.DecodeMorton32(0);
                }, NUnit.Framework.Is.Not.AllocatingGCMemory());
        }

        [Test]
        [Conditional("DEBUG")]
        public void Limits()
        {
            var maxCoordinate = new uint3(Morton.MaxCoordinateValue32);
            var maxMorton = Morton.EncodeMorton32(maxCoordinate);
            Assert.AreEqual(maxMorton, 1073741823);
            Assert.AreEqual(maxCoordinate, Morton.DecodeMorton32(maxMorton));
            Assert.Throws<OverflowException>(() => Morton.EncodeMorton32(new uint3(Morton.MaxCoordinateValue32 + 1)));
        }

        [Test]
        public void Morton_EncodeDecode()
        {
            DoEncodeDecode();
            Assert.IsTrue(m_Coordinates.ArraysEqual(m_CoordinatesDecoded));
        }

        [Test]
        public void MortonJob_EncodeDecode()
        {
            DoEncodeDecodeJob();
            Assert.IsTrue(m_Coordinates.ArraysEqual(m_CoordinatesDecoded));
        }

        [Test]
        public void MortonJob_EncodeDecodePacked()
        {
            DoTransposePacked();
            DoEncodeDecodeJobPacked();
            DoTransposeUnpacked();

            Assert.IsTrue(m_Coordinates.ArraysEqual(m_CoordinatesDecoded));
            Assert.IsTrue(m_CoordinatesPacked.ArraysEqual(m_CoordinatesDecodedPacked));
        }

        [Test]
        public void MortonJob_EncodeDecodePackedFor()
        {
            DoTransposePacked();
            DoEncodeDecodeJobPackedFor();
            DoTransposeUnpacked();

            Assert.IsTrue(m_Coordinates.ArraysEqual(m_CoordinatesDecoded));
            Assert.IsTrue(m_CoordinatesPacked.ArraysEqual(m_CoordinatesDecodedPacked));
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecode()
        {
            Measure.Method(DoEncodeDecode).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecodeJob()
        {
            Measure.Method(DoEncodeDecodeJob).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecodeJobPacked()
        {
            DoTransposePacked();
            Measure.Method(DoEncodeDecodeJobPacked).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecodeJobPackedWithTranspose()
        {
            Measure.Method(() =>
            {
                DoTransposePacked();
                DoEncodeDecodeJobPacked();
                DoTransposeUnpacked();
            }).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecodeJobPackedFor()
        {
            DoTransposePacked();
            Measure.Method(DoEncodeDecodeJobPackedFor).WarmupCount(2).Run();
        }

        void DoEncodeDecode()
        {
            for (int i = 0; i < m_Coordinates.Length; i++)
            {
                m_Codes[i] = Morton.EncodeMorton32(m_Coordinates[i]);
            }

            for (int i = 0; i < m_Codes.Length; i++)
            {
                m_CoordinatesDecoded[i] = Morton.DecodeMorton32(m_Codes[i]);
            }
        }

        void DoEncodeDecodeJob()
        {
            var encodeJob = new MortonEncodeJob()
            {
                Coordinates = m_Coordinates,
                Codes = m_Codes
            };

            var decodeJob = new MortonDecodeJob()
            {
                Codes = m_Codes,
                Coordinates = m_CoordinatesDecoded
            };

            var encodeJobHandle = encodeJob.Schedule();
            var decodeJobHandle = decodeJob.Schedule(encodeJobHandle);

            decodeJobHandle.Complete();
        }

        void DoTransposePacked()
        {
            var transposePackedJob = new TransposePackedJob()
            {
                SourceArray = m_Coordinates.Reinterpret<uint3x4>(UnsafeUtility.SizeOf<uint3>()),
                TransposedArray = m_CoordinatesPacked
            };

            transposePackedJob.Execute();
        }

        void DoTransposeUnpacked()
        {
            var transposeUnpackedJob = new TransposeUnpackedJob()
            {
                SourceArray = m_CoordinatesDecodedPacked,
                TransposedArray = m_CoordinatesDecoded.Reinterpret<uint3x4>(UnsafeUtility.SizeOf<uint3>())
            };

            transposeUnpackedJob.Execute();
        }


        void DoEncodeDecodeJobPacked()
        {
            var encodeJob = new MortonEncodeJob_Packed()
            {
                Coordinates = m_CoordinatesPacked,
                Codes = m_Codes.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>())
            };

            var decodeJob = new MortonDecodeJob_Packed()
            {
                Codes = m_Codes.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>()),
                Coordinates = m_CoordinatesDecodedPacked,
            };

            var encodeJobHandle = encodeJob.Schedule();
            var decodeJobHandle = decodeJob.Schedule(encodeJobHandle);

            decodeJobHandle.Complete();
        }

        void DoEncodeDecodeJobPackedFor()
        {
            var encodeJob = new MortonEncodeJob_PackedFor()
            {
                Coordinates = m_CoordinatesPacked,
                Codes = m_Codes.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>())
            };

            var decodeJob = new MortonDecodeJob_PackedFor()
            {
                Codes = m_Codes.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>()),
                Coordinates = m_CoordinatesDecodedPacked,
            };

            var coordinatesLength = encodeJob.Coordinates.Length;
            var encodeJobHandle = encodeJob.Schedule(coordinatesLength, 32);
            var decodeJobHandle = decodeJob.Schedule(coordinatesLength, 32, encodeJobHandle);

            decodeJobHandle.Complete();
        }
    }
}