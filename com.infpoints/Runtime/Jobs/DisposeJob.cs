using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Deallocate a NativeArray in a Job.
    /// </summary>
    [BurstCompile]
    public struct DeallocateNativeArrayJob<T> : IJob where T : unmanaged
    {
        [DeallocateOnJobCompletion] NativeArray<T> m_Values;

        public DeallocateNativeArrayJob(NativeArray<T> values)
        {
            m_Values = values;
        }
        
        public void Execute()
        {
        }
    }
}