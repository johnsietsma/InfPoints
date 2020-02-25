using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    [BurstCompile]
    public struct DisposeJob_NativeArray<T> : IJob where T : unmanaged
    {
        [DeallocateOnJobCompletion] public NativeArray<T> Values;

        public DisposeJob_NativeArray(NativeArray<T> values)
        {
            Values = values;
        }
        
        public void Execute()
        {
        }
    }

    [BurstCompile]
    public struct DisposeJob_NativeArrayXYZ<T> : IJob where T : unmanaged
    {
        [DeallocateOnJobCompletion] public NativeArray<T> ValuesX;
        [DeallocateOnJobCompletion] public NativeArray<T> ValuesY;
        [DeallocateOnJobCompletion] public NativeArray<T> ValuesZ;

        public DisposeJob_NativeArrayXYZ(NativeArrayXYZ<T> values)
        {
            ValuesX = values.X;
            ValuesY = values.Y;
            ValuesZ = values.Z;
        }

        public void Execute()
        {
            ValuesX.Dispose();
            ValuesY.Dispose();
            ValuesZ.Dispose();
        }
    }
}