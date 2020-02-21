using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct AddDataToStorageJob : IJob
    {
        [ReadOnly] public ulong SparseIndex;
        [ReadOnly][DeallocateOnJobCompletion] public NativeArrayXYZ<float> Data;
        public NativeSparsePagedArrayXYZ Storage;
        [ReadOnly] public int Count;

        public AddDataToStorageJob(ulong sparseIndex, NativeArrayXYZ<float> data, NativeSparsePagedArrayXYZ storage, int count)
        {
            SparseIndex = sparseIndex;
            Data = data;
            Storage = storage;
            Count = count;
        }
        
        public void Execute()
        {
            Logger.LogFormat(LogString.DataCountAddedToStorage, Count);
            if(!Storage.ContainsNode(SparseIndex)) Storage.AddNode(SparseIndex);
            Storage.AddData(SparseIndex, Data, Count);
        }
    }
}