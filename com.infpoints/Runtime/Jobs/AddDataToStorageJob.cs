using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct AddDataToStorageJob : IJob
    {
        [ReadOnly] public ulong SparseIndex;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArrayXYZ<float> Data;
        public NativeSparsePagedArrayXYZ Storage;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeInt Count;

        public AddDataToStorageJob(ulong sparseIndex, NativeArrayXYZ<float> data, NativeSparsePagedArrayXYZ storage,
            NativeInt count)
        {
            SparseIndex = sparseIndex;
            Data = data;
            Storage = storage;
            Count = count;
        }

        public void Execute()
        {
            Logger.LogFormat(LogMessage.DataCountAddedToStorage, Count.Value);
            Storage.AddData(SparseIndex, Data, Count.Value);
        }
    }
}