using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Get all the unique values. A count is kept of the number of values of each set.
    /// The unique values can be accessed from `UniqueValues.GetKeyArray`. 
    /// </summary>
    /// <typeparam name="T">An unmanaged and IEquatable<T> value</typeparam>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct GetUniqueValuesJob<T> : IJobParallelFor where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeArray<T> Values;
        public NativeHashMap<T, uint>.ParallelWriter UniqueValues;

        public void Execute(int index)
        {
            T key = Values[index];
            AddUniqueWithCount(UniqueValues, key);
        }

        /// <summary>
        /// Either adds a new unique value, or increments the count of the value. 
        /// </summary>
        public static void AddUniqueWithCount(NativeHashMap<T, uint>.ParallelWriter uniqueValues, T key)
        {
            uniqueValues.TryAdd(key, 1);
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct GetUniqueUint3SoAJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<uint> X;
        [ReadOnly] public NativeArray<uint> Y;
        [ReadOnly] public NativeArray<uint> Z;
        public NativeHashMap<uint3, uint>.ParallelWriter UniqueValues;

        public void Execute(int index)
        {
            var key = new uint3(X[index], Y[index], Z[index]);
            GetUniqueValuesJob<uint3>.AddUniqueWithCount(UniqueValues, key);
        }
    }
}