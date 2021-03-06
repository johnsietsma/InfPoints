﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine.TestTools.Constraints;
using Is = NUnit.Framework.Is;
using Random = Unity.Mathematics.Random;

namespace InfPoints.Tests.Editor
{
    // ----
    // Code for packing and unpacking points into 4x3 arrays

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct TransposePackedJob : IJob
    {
        [ReadOnly] public NativeArray<uint3x4> SourceArray;
        public NativeArray<uint4x3> TransposedArray;

        public void Execute()
        {
            for (int i = 0; i < SourceArray.Length; i++)
            {
                TransposedArray[i] = math.transpose(SourceArray[i]);
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct TransposeUnpackedJob : IJob
    {
        [ReadOnly] public NativeArray<uint4x3> SourceArray;
        public NativeArray<uint3x4> TransposedArray;

        public void Execute()
        {
            for (int i = 0; i < SourceArray.Length; i++)
            {
                TransposedArray[i] = math.transpose(SourceArray[i]);
            }
        }
    }

    [BurstCompile()]//FloatPrecision.Standard, FloatMode.Fast)]
    public struct Morton32EncodeJob_Packed : IJob
    {
        [ReadOnly] public NativeArray<uint4x3> Coordinates;
        public NativeArray<uint4> Codes;

        public void Execute()
        {
            for (int index = 0; index < Coordinates.Length; index++)
            {
                var coordinate = Coordinates[index];
                Codes[index] = Morton.EncodeMorton32(coordinate[0], coordinate[1], coordinate[2]);
            }
        }
    }

    [BurstCompile()]//FloatPrecision.Standard, FloatMode.Fast)]
    public struct Morton32DecodeJob_Packed : IJob
    {
        [ReadOnly] public NativeArray<uint4> Codes;
        public NativeArray<uint4x3> Coordinates;

        public void Execute()
        {
            for (int index = 0; index < Codes.Length; index++)
            {
                Coordinates[index] = Morton.DecodeMorton32(Codes[index]);
            }
        }
    }

    // ----


    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
    public class MortonTests
    {
        static readonly int NUM_COORDS = 1000000;
        const int InnerLoopBatchCont = 64;

        private static uint3[] WellKnownCoordinates32 { get; } =
        {
            new uint3(0, 0, 0),
            new uint3(1, 0, 0),
            new uint3(1, 0, 1),
            new uint3(0b0101, 0b1001, 0b0001)
        };

        static readonly uint[] WellKnownCodes32 =
        {
            0,
            1,
            5,
            0b010_001_000_111
        };

        private static uint3[] WellKnownCoordinates64 { get; } =
        {
            //  299593, 599186, 1198372
            new uint3(0b001_001_001_001_001_001_001, 0b010_010_010_010_010_010_010, 0b100_100_100_100_100_100_100)
        };


        static readonly ulong[] WellKnownCodes64 =
        {
            0b100_010_001_100_010_001_100_010_001_100_010_001_100_010_001_100_010_001_100_010_001
        };


        NativeArray<uint3> m_Coordinates32;
        NativeArray<uint4x3> m_Coordinates32Packed;
        NativeArray<uint3> m_Coordinates32Decoded;
        NativeArray<uint4x3> m_Coordinates32DecodedPacked;
        NativeArray<uint> m_Codes32;
        NativeArray<uint4> m_Codes32_4;

        NativeArray<uint3> m_Coordinates64;
        NativeArray<uint3> m_Coordinates64Decoded;
        NativeArray<ulong> m_Codes64;


        [SetUp]
        public void SetupTests()
        {
            uint3[] randomCoordinates32 = new uint3[NUM_COORDS];
            uint3[] randomCoordinates64 = new uint3[NUM_COORDS];
            uint[] randomCodes32 = new uint[NUM_COORDS];

            var r = new Random(1);
            for (int i = 0; i < randomCoordinates32.Length; i++)
            {
                randomCoordinates32[i] = r.NextUInt3(Morton.MaxCoordinateValue32);
            }

            for (int i = 0; i < randomCoordinates64.Length; i++)
            {
                randomCoordinates64[i] = r.NextUInt3(Morton.MaxCoordinateValue64);
            }

            for (int i = 0; i < randomCoordinates32.Length; i++)
            {
                randomCodes32[i] = Morton.EncodeMorton32(randomCoordinates32[i]);
            }

            int coordinatesLength = randomCoordinates32.Length;
            int packedLength = coordinatesLength / 4;
            m_Coordinates32 = new NativeArray<uint3>(randomCoordinates32, Allocator.TempJob);
            m_Coordinates64 = new NativeArray<uint3>(randomCoordinates64, Allocator.TempJob);
            m_Coordinates32Packed = new NativeArray<uint4x3>(packedLength, Allocator.TempJob);
            m_Coordinates32Decoded = new NativeArray<uint3>(coordinatesLength, Allocator.TempJob);
            m_Coordinates64Decoded = new NativeArray<uint3>(coordinatesLength, Allocator.TempJob);
            m_Coordinates32DecodedPacked = new NativeArray<uint4x3>(packedLength, Allocator.TempJob);
            m_Codes32 = new NativeArray<uint>(coordinatesLength, Allocator.TempJob);
            m_Codes64 = new NativeArray<ulong>(coordinatesLength, Allocator.TempJob);
        }

        [TearDown]
        public void TearDown()
        {
            m_Coordinates32.Dispose();
            m_Coordinates64.Dispose();
            m_Coordinates32Packed.Dispose();
            m_Coordinates32Decoded.Dispose();
            m_Coordinates64Decoded.Dispose();
            m_Coordinates32DecodedPacked.Dispose();
            m_Codes32.Dispose();
            m_Codes64.Dispose();
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
        public void WellKnownNumbers32()
        {
            for (int i = 0; i < WellKnownCodes32.Length; i++)
            {
                var code = Morton.EncodeMorton32(WellKnownCoordinates32[i]);
                var decodedCoordinate = Morton.DecodeMorton32(code);

                Assert.AreEqual(code, WellKnownCodes32[i]);
                Assert.AreEqual(WellKnownCoordinates32[i], decodedCoordinate);
            }
        }

        [Test]
        public void WellKnownNumbers64()
        {
            for (int i = 0; i < WellKnownCodes64.Length; i++)
            {
                // 64 bits functions work on 32 bit codes
                var code = Morton.EncodeMorton64(WellKnownCoordinates32[i]);
                var decodedCoordinate = Morton.DecodeMorton64(code);

                Assert.AreEqual(code, WellKnownCodes32[i]);
                Assert.AreEqual(WellKnownCoordinates32[i], decodedCoordinate);
            }

            for (int i = 0; i < WellKnownCodes64.Length; i++)
            {
                var code = Morton.EncodeMorton64(WellKnownCoordinates64[i]);
                var decodedCoordinate = Morton.DecodeMorton64(code);

                Assert.AreEqual(code, WellKnownCodes64[i]);
                Assert.AreEqual(WellKnownCoordinates64[i], decodedCoordinate);
            }
        }

        [Test]
        public void WellKnownNumbers_Packed()
        {
            var packedCoordinatesIn = new uint3x4(WellKnownCoordinates32[0], WellKnownCoordinates32[1],
                WellKnownCoordinates32[2], WellKnownCoordinates32[3]);
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
                }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        [Conditional("DEBUG")]
        public void Limits()
        {
            Assert.DoesNotThrow(() => Morton.EncodeMorton32(Morton.MaxCoordinateValue32));
            Assert.Throws<OverflowException>(() => Morton.EncodeMorton32(new uint3(Morton.MaxCoordinateValue32 + 1)));

            Assert.DoesNotThrow(() => Morton.EncodeMorton64(Morton.MaxCoordinateValue64));
            Assert.Throws<OverflowException>(() => Morton.EncodeMorton64(new uint3(Morton.MaxCoordinateValue64 + 1)));
        }

        [Test]
        public void Morton32_EncodeDecode()
        {
            DoEncodeDecode32();
            Assert.IsTrue(m_Coordinates32.ArraysEqual(m_Coordinates32Decoded));
        }

        [Test]
        public void Morton64_EncodeDecode()
        {
            DoEncodeDecode64();
            for (int i = 0; i < m_Coordinates64.Length; i++)
            {
                Assert.AreEqual(m_Coordinates64Decoded[i], m_Coordinates64[i], $"i={i}");
            }

            Assert.IsTrue(m_Coordinates64.ArraysEqual(m_Coordinates64Decoded));
        }

        [Test]
        public void MortonJob32_EncodeDecode()
        {
            DoEncodeDecodeJob32();
            Assert.IsTrue(m_Coordinates32.ArraysEqual(m_Coordinates32Decoded));
        }

        [Test]
        public void Morton32Job_EncodeDecodePacked()
        {
            DoTransposePacked32();
            DoEncodeDecodeJobPacked32();
            DoTransposeUnpacked32();

            Assert.IsTrue(m_Coordinates32.ArraysEqual(m_Coordinates32Decoded));
            Assert.IsTrue(m_Coordinates32Packed.ArraysEqual(m_Coordinates32DecodedPacked));
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecode32()
        {
            Measure.Method(DoEncodeDecode32).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecode64()
        {
            Measure.Method(DoEncodeDecode64).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecodeJob32()
        {
            Measure.Method(DoEncodeDecodeJob32).WarmupCount(2).Run();
        }

        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecodeJob64()
        {
            Measure.Method(DoEncodeDecodeJob64).WarmupCount(2).Run();
        }


        [Test, Performance]
        [Version("1")]
        public void Performance_EncodeDecodeJobPacked32()
        {
            DoTransposePacked32();
            Measure.Method(DoEncodeDecodeJobPacked32).WarmupCount(2).Run();
        }

        void DoEncodeDecode32()
        {
            for (int i = 0; i < m_Coordinates32.Length; i++)
            {
                m_Codes32[i] = Morton.EncodeMorton32(m_Coordinates32[i]);
            }

            for (int i = 0; i < m_Codes32.Length; i++)
            {
                m_Coordinates32Decoded[i] = Morton.DecodeMorton32(m_Codes32[i]);
            }
        }

        void DoEncodeDecode64()
        {
            for (int i = 0; i < m_Coordinates64.Length; i++)
            {
                m_Codes64[i] = Morton.EncodeMorton64(m_Coordinates64[i]);
            }

            for (int i = 0; i < m_Codes64.Length; i++)
            {
                m_Coordinates64Decoded[i] = Morton.DecodeMorton64(m_Codes64[i]);
            }
        }

        void DoEncodeDecodeJob32()
        {
            var encodeJob = new Morton32EncodeJob()
            {
                Coordinates = m_Coordinates32,
                Codes = m_Codes32
            };

            var decodeJob = new Morton32DecodeJob()
            {
                Codes = m_Codes32,
                Coordinates = m_Coordinates32Decoded
            };

            var encodeJobHandle = encodeJob.Schedule(m_Coordinates32.Length, InnerLoopBatchCont);
            var decodeJobHandle = decodeJob.Schedule(m_Codes32.Length, InnerLoopBatchCont, encodeJobHandle);

            decodeJobHandle.Complete();
        }

        void DoEncodeDecodeJob64()
        {
            var encodeJob = new Morton64EncodeJob()
            {
                Coordinates = m_Coordinates64,
                Codes = m_Codes64
            };

            var decodeJob = new Morton64DecodeJob()
            {
                Codes = m_Codes64,
                Coordinates = m_Coordinates64Decoded
            };

            var encodeJobHandle = encodeJob.Schedule(m_Coordinates64.Length, InnerLoopBatchCont);
            var decodeJobHandle = decodeJob.Schedule(m_Codes64.Length, InnerLoopBatchCont, encodeJobHandle);

            decodeJobHandle.Complete();
        }

        void DoTransposePacked32()
        {
            var transposePackedJob = new TransposePackedJob()
            {
                SourceArray = m_Coordinates32.Reinterpret<uint3x4>(UnsafeUtility.SizeOf<uint3>()),
                TransposedArray = m_Coordinates32Packed
            };

            transposePackedJob.Execute();
        }

        void DoTransposeUnpacked32()
        {
            var transposeUnpackedJob = new TransposeUnpackedJob()
            {
                SourceArray = m_Coordinates32DecodedPacked,
                TransposedArray = m_Coordinates32Decoded.Reinterpret<uint3x4>(UnsafeUtility.SizeOf<uint3>())
            };

            transposeUnpackedJob.Execute();
        }


        void DoEncodeDecodeJobPacked32()
        {
            var encodeJob = new Morton32EncodeJob_Packed()
            {
                Coordinates = m_Coordinates32Packed,
                Codes = m_Codes32.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>())
            };

            var decodeJob = new Morton32DecodeJob_Packed()
            {
                Codes = m_Codes32.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>()),
                Coordinates = m_Coordinates32DecodedPacked,
            };

            var encodeJobHandle = encodeJob.Schedule();
            var decodeJobHandle = decodeJob.Schedule(encodeJobHandle);

            decodeJobHandle.Complete();
        }
    }
}