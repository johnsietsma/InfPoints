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
    public struct GetUniqueValuesJob<T> : IJob where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeArray<T> Values;
        public NativeHashMap<T, int> UniqueValues;

        public GetUniqueValuesJob(NativeArray<T> values, NativeHashMap<T, int> uniqueValues)
        {
            Values = values;
            UniqueValues = uniqueValues;
        }
        
        public void Execute()
        {
            for (int index = 0; index < Values.Length; index++)
            {
                T key = Values[index];
                if(!UniqueValues.ContainsKey(key)) UniqueValues.Add(key, 0);
                UniqueValues[key]++;
            }
            Logger.LogFormat(LogMessage.UniqueValuesCollected, UniqueValues.Length);
        }
    }
}