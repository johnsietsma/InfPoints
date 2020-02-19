using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton32EncodeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint3> Coordinates;
        public NativeArray<uint> Codes;

        public void Execute(int index)
        {
            Codes[index] = Morton.EncodeMorton32(Coordinates[index]);
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton64EncodeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint3> Coordinates;
        public NativeArray<ulong> Codes;

        public void Execute(int index)
        {
            Codes[index] = Morton.EncodeMorton64(Coordinates[index]);
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton64SoAEncodeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint> CoordinatesX;
        [ReadOnly] public NativeArray<uint> CoordinatesY;
        [ReadOnly] public NativeArray<uint> CoordinatesZ;
        public NativeArray<ulong> Codes;

        public Morton64SoAEncodeJob(NativeArrayXYZ<uint> coordinates, NativeArray<ulong> codes)
        {
            CoordinatesX = coordinates.X;
            CoordinatesY = coordinates.Y;
            CoordinatesZ = coordinates.Z;
            Codes = codes;
        }

        public void Execute(int index)
        {
            Codes[index] = Morton.EncodeMorton64( new uint3(CoordinatesX[index], CoordinatesY[index], CoordinatesZ[index]));
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton32DecodeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint> Codes;
        public NativeArray<uint3> Coordinates;

        public void Execute(int index)
        {
            Coordinates[index] = Morton.DecodeMorton32(Codes[index]);
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton64DecodeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ulong> Codes;
        public NativeArray<uint3> Coordinates;

        public void Execute(int index)
        {
            Coordinates[index] = Morton.DecodeMorton64(Codes[index]);
        }
    }
}