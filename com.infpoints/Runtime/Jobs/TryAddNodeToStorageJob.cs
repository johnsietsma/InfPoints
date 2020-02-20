using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Add the node at <c>SparseIndex</c> to <c>Storage</c>, if it hasn't already been added.
    /// </summary>
    public struct TryAddNodeToStorageJob : IJob
    {
        [ReadOnly] public ulong SparseIndex;
        public NativeSparsePagedArrayXYZ Storage;

        public TryAddNodeToStorageJob( ulong sparseIndex, NativeSparsePagedArrayXYZ storage)
        {
            SparseIndex = sparseIndex;
            Storage = storage;
        }
        
        public void Execute()
        {
            if (!Storage.ContainsNode(SparseIndex))
            {
                Storage.AddNode(SparseIndex);
            }
        }
    }
}