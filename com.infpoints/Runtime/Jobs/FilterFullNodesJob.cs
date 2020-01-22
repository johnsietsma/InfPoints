using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct FilterFullNodesJob : IJobParallelForFilter
    {
        [ReadOnly] public NativeArray<NodeStorage> NodeStorage;
        
        public bool Execute(int index)
        {
            return NodeStorage[index].IsFull;
        }
    }
}