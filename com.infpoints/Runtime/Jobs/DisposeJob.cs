using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    [BurstCompile]
    struct DisposeJob_NativeArray<T> : IJob where T : unmanaged
    {
        public NativeArray<T> Values;

        public DisposeJob_NativeArray(NativeArray<T> values)
        {
            Values = values;
        }

        public void Execute()
        {
            Values.Dispose();
        }
    }
    
    [BurstCompile]
    struct DisposeJob_NativeList<T> : IJob where T : unmanaged
    {
        public NativeList<T> Values;

        public DisposeJob_NativeList(NativeList<T> values)
        {
            Values = values;
        }

        public void Execute()
        {
            Values.Dispose();
        }
    }
    
    [BurstCompile]
    struct DisposeJob_NativeArrayXYZ<T> : IJob where T : unmanaged
    {
        public NativeArray<T> ValuesX;
        public NativeArray<T> ValuesY;
        public NativeArray<T> ValuesZ;

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