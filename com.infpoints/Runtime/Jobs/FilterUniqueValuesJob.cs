using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Filter all the values, keeping the first unique value of every set.
    /// A count is kept of the number of values of each set.
    /// To get the unique values you can use the indices obtained from `ScheduleAppend` or `ScheduleFilter`.
    /// Or alternatively, the unique values can be accessed from `UniqueValues.GetKeyArray`. 
    /// </summary>
    /// <typeparam name="T">An unmanaged and IEquatable<T> value</typeparam>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct FilterUniqueValuesJob<T> : IJobParallelForFilter where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeArray<T> Values;
        public NativeHashMap<T, uint> UniqueValues;

        public bool Execute(int index)
        {
            T key = Values[index];
            return AddUniqueWithCount(UniqueValues, key);
        }

        /// <summary>
        /// Either adds a new unique value, or increments the count of the value. 
        /// </summary>
        /// <returns>Whether the value is unique</returns>
        public static bool AddUniqueWithCount(NativeHashMap<T, uint> uniqueValues, T key)
        {
            if (uniqueValues.TryGetValue(key, out var count))
            {
                uniqueValues[key] = count + 1;
                return false;
            }
            else
            {
                // First time we've encountered this, it is unique
                uniqueValues.TryAdd(key, 1);
                return true;
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct FilterUniqueUint3SoAJob : IJobParallelForFilter
    {
        [ReadOnly] public NativeArray<uint> X;
        [ReadOnly] public NativeArray<uint> Y;
        [ReadOnly] public NativeArray<uint> Z;
        public NativeHashMap<uint3, uint> UniqueValues;

        public bool Execute(int index)
        {
            var key = new uint3(X[index], Y[index], Z[index]);
            return FilterUniqueValuesJob<uint3>.AddUniqueWithCount(UniqueValues, key);
        }
    }
}