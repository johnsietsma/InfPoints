using System;
using JacksonDunstan.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CollectUniqueJob<T> : IJob where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeArray<T> Values;
        public NativeHashSet<T> UniqueValues;

        public void Execute()
        {
            for (int index = 0; index < Values.Length; index++)
            {
                if (!UniqueValues.Contains(Values[index]))
                {
                    UniqueValues.TryAdd(Values[index]);
                }
            }
        }
    }
}