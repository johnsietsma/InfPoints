using Unity.Collections;
using Unity.Jobs;

namespace InfPoints
{
    public static class JobUtils
    {
        public static JobHandle CombineHandles(params JobHandle[] jobHandles)
        {
            var handles = new NativeArray<JobHandle>(jobHandles.Length, Allocator.Temp);
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i] = jobHandles[i];
            }
            var combinedJobHandle = JobHandle.CombineDependencies(handles);
            handles.Dispose();
            return combinedJobHandle;
        }
    }
}