using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton32EncodeJob : IJob
    {
        [ReadOnly] public NativeArray<uint3> Coordinates;
        public NativeArray<uint> Codes;

        public void Execute()
        {
            for (int i = 0; i < Coordinates.Length; i++)
            {
                Codes[i] = Morton.EncodeMorton32(Coordinates[i]);
            }
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton64EncodeJob : IJob
    {
        [ReadOnly] public NativeArray<uint3> Coordinates;
        public NativeArray<ulong> Codes;

        public void Execute()
        {
            for (int i = 0; i < Coordinates.Length; i++)
            {
                Codes[i] = Morton.EncodeMorton64(Coordinates[i]);
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton32EncodeJob_Packed : IJob
    {
        [ReadOnly] public NativeArray<uint4x3> Coordinates;
        public NativeArray<uint4> Codes;

        public void Execute()
        {
            for (int i = 0; i < Coordinates.Length; i++)
            {
                var coordinate = Coordinates[i];
                Codes[i] = Morton.EncodeMorton32(coordinate[0], coordinate[1], coordinate[2]);
            }
        }
    }

    public struct Morton32EncodeJob_PackedFor : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint4x3> Coordinates;
        public NativeArray<uint4> Codes;

        public void Execute(int index)
        {
            var coordinate = Coordinates[index];
            Codes[index] = Morton.EncodeMorton32(coordinate[0], coordinate[1], coordinate[2]);
        }
    }


    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton32DecodeJob : IJob
    {
        [ReadOnly] public NativeArray<uint> Codes;
        public NativeArray<uint3> Coordinates;

        public void Execute()
        {
            for (int i = 0; i < Codes.Length; i++)
            {
                Coordinates[i] = Morton.DecodeMorton32(Codes[i]);
            }
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton64DecodeJob : IJob
    {
        [ReadOnly] public NativeArray<ulong> Codes;
        public NativeArray<uint3> Coordinates;

        public void Execute()
        {
            for (int i = 0; i < Codes.Length; i++)
            {
                Coordinates[i] = Morton.DecodeMorton64(Codes[i]);
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton32DecodeJob_Packed : IJob
    {
        [ReadOnly] public NativeArray<uint4> Codes;
        public NativeArray<uint4x3> Coordinates;

        public void Execute()
        {
            for (int i = 0; i < Codes.Length; i++)
            {
                Coordinates[i] = Morton.DecodeMorton32(Codes[i]);
            }
        }
    }

    public struct Morton32DecodeJob_PackedFor : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint4> Codes;
        public NativeArray<uint4x3> Coordinates;

        public void Execute(int index)
        {
            Coordinates[index] = Morton.DecodeMorton32(Codes[index]);
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