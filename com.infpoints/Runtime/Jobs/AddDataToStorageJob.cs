using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct AddDataToStorageJob : IJob
    {
        [ReadOnly] public ulong SparseIndex;
        [ReadOnly][DeallocateOnJobCompletion] public NativeArrayXYZ<float> Data;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeInt Count;
        public NativeSparsePagedArrayXYZ Storage;

        public void Execute()
        {
            Logger.Log($"Adding {Count.Value} points.");
            if(!Storage.ContainsNode(SparseIndex)) Storage.AddNode(SparseIndex);
            Storage.AddData(SparseIndex, Data, Count.Value);
        }
    }
}