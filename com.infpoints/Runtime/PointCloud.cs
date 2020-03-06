using System;
using InfPoints.Jobs;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public class PointCloud : IDisposable
    {
        public SparseOctree Octree { get; }

        const int InnerLoopBatchCount = 128;
        const int MaximumPointsPerNode = 1024 * 1024;

        public PointCloud(AABB aabb)
        {
            Octree = new SparseOctree(aabb, MaximumPointsPerNode, Allocator.Persistent);
        }

        public JobHandle AddPoints(NativeArrayXYZ<float> points)
        {
            return AddPoints(points, points.Length);
        }

        public JobHandle AddPoints(NativeArrayXYZ<float> points, int pointCount)
        {
            // TODO: https://www.nuget.org/packages/System.Buffers

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Because of reinterpret to SIMD friendly types 
            if (points.Length % 4 != 0) throw new ArgumentException("Points must be added in multiples of 4");

            using (var outsideCount = new NativeInt(Allocator.TempJob))
            {
                // Check points are inside AABB
                var arePointsInsideJob = new CountPointsOutsideAABBJob(Octree.AABB, points, outsideCount)
                    .Schedule(points.Length, InnerLoopBatchCount);
                arePointsInsideJob.Complete();
                if (outsideCount.Value > 0)
                {
                    Logger.LogError(
                        $"[PointCloud]Trying to add {outsideCount.Value} points outside the AABB {Octree.AABB}, aborting");
                    return default;
                }
            }
#endif

            int levelIndex = Octree.AddLevel() - 1;
            Logger.Log($"[PointCloud] Adding {points.Length} points to the octree with AABB {Octree.AABB}.");

            var pointsLength = (pointCount + 3) & ~0x03; // Next multiple of 4

            // The morton code of each point
            var pointMortonCodes =
                new NativeArray<ulong>(pointsLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            // The morton codes for each node with no duplicates
            var validMortonCodes = new NativeSparseArray<ulong, int>(pointsLength, Allocator.TempJob);
            var preparePointsHandle = ConvertPointsToMortonCodes(points, pointsLength, levelIndex, pointMortonCodes);

            var validNodesHandle = GetValidNodes(levelIndex, pointMortonCodes, validMortonCodes, preparePointsHandle);
            validNodesHandle.Complete();

            Logger.Log("[PointCloud] Valid indices " + validMortonCodes.Length);

            var addPointsHandle = AddPointsToNodes(points, levelIndex, validMortonCodes, pointMortonCodes, validNodesHandle);
            var deallocatePointMortonCodesHandle = new DeallocateNativeArrayJob<ulong>(pointMortonCodes).Schedule(addPointsHandle);
            var deallocateValidMortonCodesHandle = new DeallocateNativeSparseArrayJob<ulong,int>(validMortonCodes).Schedule(deallocatePointMortonCodesHandle);
            return deallocateValidMortonCodesHandle;
        }

        JobHandle GetValidNodes(int levelIndex, NativeArray<ulong> pointMortonCodes,
            NativeSparseArray<ulong, int> validMortonCodes, JobHandle deps=default)
        {
            // Get all unique codes and a count of how many of each there are
            var uniqueCodesMapHandle = new GetUniqueValuesJob<ulong>(pointMortonCodes, validMortonCodes)
                .Schedule(deps);

            // Filter out full nodes
            var nodeStorage = Octree.GetNodeStorage(levelIndex);
            return new FilterFullNodesJob(nodeStorage, validMortonCodes).Schedule(uniqueCodesMapHandle);
        }

        JobHandle ConvertPointsToMortonCodes(NativeArrayXYZ<float> points, int pointsLength, int levelIndex,
            NativeArray<ulong> pointsMortonCodes, JobHandle deps=default)
        {
            int cellCount = SparseOctreeUtils.GetNodeCount(levelIndex);
            float cellWidth = Octree.AABB.Size / cellCount;

            Logger.Log($"[PointCloud] Prepare - Cell count:{cellCount} Cell width:{cellWidth}");

            // Transform points from world to Octree AABB space
            float3 offset = -Octree.AABB.Minimum;
            var transformJob = new NativeArrayXYZUtils.AdditionJob_NativeArrayXYZ_float4(points, pointsLength, offset);
            var transformHandle = transformJob.Schedule(transformJob.Length, InnerLoopBatchCount, deps);

            var pointCoordinates =
                new NativeArrayXYZ<uint>(pointsLength, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            // Convert all points to node coordinates (The octree coordinates of each point)
            var coordinatesJob = new NativeArrayXYZUtils.IntegerDivisionJob_NativeArrayXYZ_float4_uint4(points,
                pointsLength, pointCoordinates, cellWidth);
            var coordinatesJobHandle = coordinatesJob
                .Schedule(coordinatesJob.Length, InnerLoopBatchCount, transformHandle);

            // Convert coordinates to morton codes (deallocates pointCoordinates)
            return new Morton64SoAEncodeJob(pointCoordinates, pointsMortonCodes)
                .Schedule(pointCoordinates.Length, InnerLoopBatchCount, coordinatesJobHandle);
        }

        JobHandle AddPointsToNodes(NativeArrayXYZ<float> points, int levelIndex,
            NativeSparseArray<ulong, int> mortonCodeCounts, NativeArray<ulong> pointMortonCodes, JobHandle deps=default)
        {
            var nodeStorage = Octree.GetNodeStorage(levelIndex);
            // For each node
            var collectJobHandles = new JobHandle[mortonCodeCounts.Length];
            for (int index = 0; index < mortonCodeCounts.Length; index++)
            {
                ulong mortonCode = mortonCodeCounts.Indices[index];
                int pointsInNodeCount = mortonCodeCounts[mortonCode];

                collectJobHandles[index] =
                    AddPointsToNode(mortonCode, points, pointMortonCodes, pointsInNodeCount, nodeStorage, deps);
            }

            return JobUtils.CombineHandles(collectJobHandles);
        }

        JobHandle AddPointsToNode(ulong mortonCode, NativeArrayXYZ<float> points, NativeArray<ulong> mortonCodes,
            int pointsInNodeCount, NativeSparsePagedArrayXYZ storage, JobHandle deps=default)
        {
            Logger.Log($"[PointCloud] Adding {pointsInNodeCount} points to node {mortonCode}");

            // Add node to the Octree if it doesn't exist
            if (!storage.ContainsNode(mortonCode))
                storage.AddNode(mortonCode);

            // The index to morton code of each point we'll add
            var pointIndices = new NativeArray<int>(pointsInNodeCount, Allocator.TempJob);
            var pointCount = new NativeInt(Allocator.TempJob);
            var pointsToAddCount = new NativeInt(Allocator.TempJob);

            // Collect point indices belonging to this node
            var collectHandle = new CollectPointIndicesJob(mortonCode, mortonCodes, pointIndices, pointCount)
                .Schedule(deps);

            // Figure out how many points we can add
            var calculatePointsHandle =
                new CalculatePointsToAddCount(storage, mortonCode, pointCount, MaximumPointsPerNode, pointsToAddCount)
                    .Schedule(collectHandle);

            // Filter points that don't "fit"
            var withinDistanceJobHandle =
                IsWithinDistanceJob.BuildJobChain(points, pointsToAddCount, pointIndices, calculatePointsHandle);

            var collectedPoints = new NativeArrayXYZ<float>(pointsInNodeCount, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);

            // Collect points
            var collectPointsJobHandle =
                new CopyPointsByIndexJob(pointIndices, points, collectedPoints, pointsToAddCount)
                    .Schedule(withinDistanceJobHandle);


            // collectedPoints disposed on job completion
            return new AddDataToStorageJob(storage, mortonCode, collectedPoints, pointsToAddCount)
                .Schedule(collectPointsJobHandle);
        }

        public void Dispose()
        {
            Octree?.Dispose();
        }
    }
}