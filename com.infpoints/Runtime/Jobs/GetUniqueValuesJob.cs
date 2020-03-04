using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Get all the unique values. A count is kept of the number of values of each set.
    /// </summary>
    /// <typeparam name="T">An unmanaged and IEquatable<T> value</typeparam>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct GetUniqueValuesJob<T> : IJob where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] readonly NativeArray<ulong> m_Values;
        NativeSparseList<ulong,int> m_UniqueValues;

        /// <summary>
        /// Finds all the unique occurrences of <c>values</c>.
        /// </summary>
        /// <param name="values">The values to find unique values in</param>
        /// <param name="uniqueValues">All the unique value and the count of their occurrences</param>
        public GetUniqueValuesJob(NativeArray<ulong> values, NativeSparseList<ulong,int> uniqueValues)
        {
            m_Values = values;
            m_UniqueValues = uniqueValues;
        }
        
        public void Execute()
        {
            foreach (var key in m_Values)
            {
                if(!m_UniqueValues.ContainsIndex(key)) m_UniqueValues.AddValue(key, 0);
                m_UniqueValues[key]++;
            }

            Logger.LogFormat(LogMessage.UniqueValuesCollected, m_UniqueValues.Length);
        }
    }
}