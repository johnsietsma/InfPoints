using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CollectUniqueJob<T> : IJob where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeArray<T> Values;
        public NativeHashMap<T, int> UniqueValues;

        public void Execute()
        {
            for (int index = 0; index < Values.Length; index++)
            {
                if (!UniqueValues.ContainsKey(Values[index]))
                {
                    UniqueValues.Add(Values[index], 1);
                }
            }
        }
    }
}