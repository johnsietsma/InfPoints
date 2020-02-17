using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct AddNodeToStorageJob : IJob
    {
        [ReadOnly] public ulong SparseIndex;
        public NativeSparsePagedArrayXYZ Storage;
        
        public void Execute()
        {
            if (!Storage.ContainsNode(SparseIndex))
            {
                Storage.AddNode(SparseIndex);
            }
        }
    }
}