using Unity.Collections;
using Unity.Jobs;
using UnityEditor;

namespace InfPoints
{
    public static class JobUtils
    {
        public static JobHandle ScheduleMultiple<T>(int length, int innerLoopBatchCount, params T[] jobs)
            where T : struct, IJobParallelFor
        {
            return ScheduleMultiple(length, innerLoopBatchCount, default, jobs);
        }
        
        public static JobHandle ScheduleMultiple<T>(int length, int innerLoopBatchCount, JobHandle dependsOn, params T[] jobs) where T : struct, IJobParallelFor
        {
            var handles = new NativeArray<JobHandle>(jobs.Length, Allocator.TempJob);
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i] = jobs[i].Schedule(length, innerLoopBatchCount, dependsOn);
            }
            var combinedJobHandle = JobHandle.CombineDependencies(handles);
            handles.Dispose();
            return combinedJobHandle;
        }
    }
}