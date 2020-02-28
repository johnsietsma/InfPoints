using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Get all the unique values. A count is kept of the number of values of each set.
    /// </summary>
    /// <typeparam name="T">An unmanaged and IEquatable<T> value</typeparam>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct GetUniqueValuesJob<T> : IJob where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeArray<ulong> Values;
        public NativeSparseList<ulong,int> UniqueValues;

        public GetUniqueValuesJob(NativeArray<ulong> values, NativeSparseList<ulong,int> uniqueValues)
        {
            Values = values;
            UniqueValues = uniqueValues;
        }
        
        public void Execute()
        {
            for (int index = 0; index < Values.Length; index++)
            {
                ulong key = Values[index];
                if(!UniqueValues.ContainsIndex(key)) UniqueValues.AddValue(key, 0);
                UniqueValues[key]++;
            }
            Logger.LogFormat(LogMessage.UniqueValuesCollected, UniqueValues.Length);
        }
    }
}