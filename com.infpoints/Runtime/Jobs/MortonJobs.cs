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
    public struct Morton64DecodeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ulong> Codes;
        public NativeArray<uint3> Coordinates;

        public void Execute(int index)
        {
            Coordinates[index] = Morton.DecodeMorton64(Codes[index]);
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Morton64SoAEncodeJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion][ReadOnly] NativeArray<uint> m_CoordinatesX;
        [DeallocateOnJobCompletion][ReadOnly] NativeArray<uint> m_CoordinatesY;
        [DeallocateOnJobCompletion][ReadOnly] NativeArray<uint> m_CoordinatesZ;
        NativeArray<ulong> m_Codes;

        public Morton64SoAEncodeJob(NativeArrayXYZ<uint> coordinates, NativeArray<ulong> codes)
        {
            m_CoordinatesX = coordinates.X;
            m_CoordinatesY = coordinates.Y;
            m_CoordinatesZ = coordinates.Z;
            m_Codes = codes;
        }

        public void Execute(int index)
        {
            m_Codes[index] = Morton.EncodeMorton64( new uint3(m_CoordinatesX[index], m_CoordinatesY[index], m_CoordinatesZ[index]));
        }
    }
}