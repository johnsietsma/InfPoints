using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct FilterFullNodesJob : IJobParallelForFilter
    {
        [ReadOnly] public NativeSparseArray<PageAllocation> PageAllocations;
        [ReadOnly] public NativeArray<ulong> MortonCodes;
        
        public bool Execute(int index)
        {
            ulong code = MortonCodes[index];
            return PageAllocations[code].IsFull;
        }
    }
}