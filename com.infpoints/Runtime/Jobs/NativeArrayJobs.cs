using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct NativeArrayIndexOfJob<T> : IJob where T :unmanaged, IEquatable<T>
    {
        [ReadOnly] NativeArray<T> Values;
        [ReadOnly] T TargetValue;
        NativeInt Index;

        public NativeArrayIndexOfJob(NativeArray<T> values, T targetValue, NativeInt index)
        {
            Values = values;
            TargetValue = targetValue;
            Index = index;
            Index.Value = -1;
        }
        
        public void Execute()
        {
            for (int index = 0; index < Values.Length; index++)
            {
                if (Values[index].Equals(TargetValue))
                {
                    Index.Value = index;
                    break;
                }
            }
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct NativeArrayIndexOfJob_BitField32 : IJob
    {
        [ReadOnly] NativeArray<BitField32> Values;
        [ReadOnly] BitField32 TargetValue;
        NativeInt Index;

        public NativeArrayIndexOfJob_BitField32(NativeArray<BitField32> values, BitField32 targetValue, NativeInt index)
        {
            Values = values;
            TargetValue = targetValue;
            Index = index;
            Index.Value = -1;
        }
        
        public void Execute()
        {
            for (int index = 0; index < Values.Length; index++)
            {
                if (Values[index].Value.Equals(TargetValue.Value))
                {
                    Index.Value = index;
                    break;
                }
            }
        }
    }
}