using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Octree.Morton
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MortonEncodeJob : IJob
    {
        [ReadOnly] public NativeArray<uint3> Coordinates;
        public NativeArray<uint> Codes;

        public void Execute()
        {
            for (int i = 0; i < Coordinates.Length; i++)
            {
                Codes[i] = Morton.EncodeMorton3(Coordinates[i]);
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MortonEncodeJob_Packed : IJob
    {
        [ReadOnly] public NativeArray<uint4x3> Coordinates;
        public NativeArray<uint4> Codes;

        public void Execute()
        {
            for (int i = 0; i < Coordinates.Length; i++)
            {
                Codes[i] = Morton.EncodeMorton3(Coordinates[i]);
            }
        }
    }

    public struct MortonEncodeJob_PackedFor : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint4x3> Coordinates;
        public NativeArray<uint4> Codes;

        public void Execute(int index)
        {
            Codes[index] = Morton.EncodeMorton3(Coordinates[index]);
        }
    }


    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MortonDecodeJob : IJob
    {
        [ReadOnly] public NativeArray<uint> Codes;
        public NativeArray<uint3> Coordinates;

        public void Execute()
        {
            for (int i = 0; i < Codes.Length; i++)
            {
                Coordinates[i] = Morton.DecodeMorton3(Codes[i]);
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MortonDecodeJob_Packed : IJob
    {
        [ReadOnly] public NativeArray<uint4> Codes;
        public NativeArray<uint4x3> Coordinates;

        public void Execute()
        {
            for (int i = 0; i < Codes.Length; i++)
            {
                Coordinates[i] = Morton.DecodeMorton3(Codes[i]);
            }
        }
    }

    public struct MortonDecodeJob_PackedFor : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint4> Codes;
        public NativeArray<uint4x3> Coordinates;

        public void Execute(int index)
        {
            Coordinates[index] = Morton.DecodeMorton3(Codes[index]);
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
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

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
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
}