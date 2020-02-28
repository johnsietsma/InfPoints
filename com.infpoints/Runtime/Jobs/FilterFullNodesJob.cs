using InfPoints.NativeCollections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct FilterFullNodesJob<T> : IJob where T :unmanaged
    {
        [ReadOnly] public NativeSparsePagedArrayXYZ Storage;
        [ReadOnly] public NativeSparseList<ulong,int> MortonCodes;

        public FilterFullNodesJob(NativeSparsePagedArrayXYZ storage, NativeSparseList<ulong,int> mortonCodes)
        {
            Storage = storage;
            MortonCodes = mortonCodes;
        }
        
        
        public void Execute()
        {
            var indices = MortonCodes.Indices;
            for (int index = 0; index < MortonCodes.Length; index++)
            {
                ulong code = indices[index];
                if (Storage.ContainsNode(code) && Storage.IsFull(code))
                    MortonCodes.RemoveAtSwapBack(code);
            }
        }
    }
}