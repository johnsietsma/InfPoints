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
}