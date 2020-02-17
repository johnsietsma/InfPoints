using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct AddDataToStorageJob : IJob
    {
        [ReadOnly] public ulong SparseIndex;
        [ReadOnly][DeallocateOnJobCompletion] public XYZSoA<float> Data;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeInt Count;
        public NativeNodeStorage Storage;

        public void Execute()
        {
            Storage.AddData(SparseIndex, Data, Count.Value);
        }
    }
}