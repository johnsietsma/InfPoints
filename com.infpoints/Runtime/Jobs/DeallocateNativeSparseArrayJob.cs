using System;
using InfPoints.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Deallocate a NativeSpraseArray in a Job.
    /// </summary>
    [BurstCompile]
    public struct DeallocateNativeSparseArrayJob<TIndex,TData> : IJob
        where TIndex : unmanaged, IComparable<TIndex>
        where TData : unmanaged
    {
        [DeallocateOnJobCompletion] NativeArray<TIndex> m_Indices;
        [DeallocateOnJobCompletion] NativeArray<TData> m_Data;
        [DeallocateOnJobCompletion] NativeInt m_Length;

        public DeallocateNativeSparseArrayJob(NativeSparseArray<TIndex,TData> values)
        {
            m_Indices = values.Indices;
            m_Data = values.Data;
            m_Length = values.NativeLength;
        }
        
        public void Execute()
        {
        }
    }
}