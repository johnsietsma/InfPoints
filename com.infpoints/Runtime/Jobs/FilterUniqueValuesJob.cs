using System;
using JacksonDunstan.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct FilterUniqueValuesJob<T> : IJobParallelForFilter where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeArray<T> Values;
        public NativeHashSet<T> UniqueValues;

        public bool Execute(int index)
        {
            if (!UniqueValues.Contains(Values[index]))
            {
                UniqueValues.TryAdd(Values[index]);
                return true;
            }

            return false;
        }
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct FilterUniqueUint3SoAJob : IJobParallelForFilter
    {
        [ReadOnly] public NativeArray<uint> X;
        [ReadOnly] public NativeArray<uint> Y;
        [ReadOnly] public NativeArray<uint> Z;
        public NativeHashSet<uint3> UniqueValues;

        public bool Execute(int index)
        {
            var value = new uint3(X[index], Y[index], Z[index]);
            if (!UniqueValues.Contains(value))
            {
                UniqueValues.TryAdd(value);
                return true;
            }

            return false;
        }
    }
}