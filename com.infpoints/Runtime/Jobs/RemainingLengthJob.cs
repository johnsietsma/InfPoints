using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    public struct RemainingLengthJob : IJob
    {
        [ReadOnly] NativeSparsePagedArrayXYZ Storage;
        [ReadOnly] ulong MortonCode;
        [ReadOnly] int MaximumPointsPerNode;
        [DeallocateOnJobCompletion] [ReadOnly] NativeInt PointCount;
        public NativeInt RemainingStoragePerNode;

        public RemainingLengthJob(NativeSparsePagedArrayXYZ storage, ulong mortonCode, NativeInt pointCount, NativeInt remainingStoragePerNode,
            int maximumPointsPerNode)
        {
            Storage = storage;
            MortonCode = mortonCode;
            RemainingStoragePerNode = remainingStoragePerNode;
            MaximumPointsPerNode = maximumPointsPerNode;
            PointCount = pointCount;
        }
        
        public void Execute()
        {
            int maximumRemaining = MaximumPointsPerNode - Storage.GetLength(MortonCode);
            RemainingStoragePerNode.Value = math.min(PointCount.Value, maximumRemaining);
        }
    }
}