using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct AddNodeToStorageJob : IJob
    {
        [ReadOnly] public ulong SparseIndex;
        public NativeNodeStorage Storage;
        
        public void Execute()
        {
            if (!Storage.ContainsNode(SparseIndex))
            {
                Storage.AddNode(SparseIndex);
            }
        }
    }
}