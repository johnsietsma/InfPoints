using System;
using System.Diagnostics;
using InfPoints.Jobs;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace InfPoints
{
    public class PointCloud : IDisposable
    {
        public SparseOctree Octree => m_Octree;

        const int InnerLoopBatchCount = 128;
        const int MaximumPointsPerNode = 1024 * 1024;

        SparseOctree m_Octree;

        public PointCloud(AABB aabb)
        {
            m_Octree = new SparseOctree(aabb, MaximumPointsPerNode, Allocator.Persistent);
        }


        public void AddPoints(NativeArrayXYZ<float> points)
        {
            // TODO: https://www.nuget.org/packages/System.Buffers

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Because of reinterpret to SIMD friendly types 
            if (points.Length % 4 != 0) throw new ArgumentException("Points must be added in multiples of 4");

            using (var outsideCount = new NativeInt(0, Allocator.TempJob))
            {
                // Check points are inside AABB
                var arePointsInsideJob = new CountPointsOutsideAABBJob(Octree.AABB, points, outsideCount)
                    .Schedule(points.Length, InnerLoopBatchCount);
                arePointsInsideJob.Complete();
                if (outsideCount.Value > 0)
                {
                    Logger.LogError(
                        $"[PointCloud]Trying to add {outsideCount.Value} points outside the AABB {Octree.AABB}, aborting");
                    return;
                }
            }
#endif

            int levelIndex = Octree.AddLevel() - 1;
            Logger.Log($"[PointCloud] Adding {points.Length} points to the octree with AABB {Octree.AABB}.");

            // The morton code of each point
            var pointMortonCodes =
                new NativeArray<ulong>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            // The morton codes for each node with no duplicates
            var validMortonCodes = new NativeSparseList<ulong, int>(10, Allocator.TempJob);
            unsafe
            {
                if(validMortonCodes.Indices.GetUnsafeReadOnlyPtr()==null) 
                    Assert.IsTrue(false);
            }


            var preparePointsHandle = ConvertPointsToMortonCodes(points, levelIndex, pointMortonCodes);
            preparePointsHandle.Complete();

            var validNodesHandle = GetValidNodes(levelIndex, pointMortonCodes, validMortonCodes);
            validNodesHandle.Complete();

            Logger.Log("[PointCloud] Valid indices " + validMortonCodes.Length);

            var addPointsHandle = AddPointsToLevel(points, levelIndex, validMortonCodes, pointMortonCodes);
            addPointsHandle.Complete();

            pointMortonCodes.Dispose();
            validMortonCodes.Dispose();
        }

        JobHandle GetValidNodes(int levelIndex, NativeArray<ulong> pointMortonCodes,
            NativeSparseList<ulong, int> validMortonCodes)
        {
            // Get all unique codes and a count of how many of each there are
            var uniqueCodesMapHandle = new GetUniqueValuesJob<ulong>(pointMortonCodes, validMortonCodes)
                .Schedule();

            // Filter out full nodes
            var nodeStorage = Octree.GetNodeStorage(levelIndex);
            return new FilterFullNodesJob<float>(nodeStorage, validMortonCodes).Schedule(uniqueCodesMapHandle);
        }

        JobHandle AddPointsToLevel(NativeArrayXYZ<float> points, int levelIndex, NativeSparseList<ulong, int> mortonCodes, NativeArray<ulong> pointMortonCodes)
        {
            var nodeStorage = Octree.GetNodeStorage(levelIndex);
            // For each node
            var collectJobHandles = new JobHandle[mortonCodes.Length];
            for (int index = 0; index < mortonCodes.Length; index++)
            {
                ulong mortonCode = mortonCodes.Indices[index];
                int pointsInNodeCount = mortonCodes[mortonCode];

                collectJobHandles[index] = AddPointsToNode(mortonCode, points, pointMortonCodes, pointsInNodeCount, nodeStorage);
            }

            return JobUtils.CombineHandles(collectJobHandles);
        }

        JobHandle ConvertPointsToMortonCodes(NativeArrayXYZ<float> points, int levelIndex,
            NativeArray<ulong> pointsMortonCodes)
        {
            int cellCount = SparseOctreeUtils.GetNodeCount(levelIndex);
            float cellWidth = Octree.AABB.Size / cellCount;

            Logger.Log($"[PointCloud] Prepare - Cell count:{cellCount} Cell width:{cellWidth}");

            // Transform points from world to Octree AABB space
            float3 offset = -m_Octree.AABB.Minimum;
            var transformJob = new NativeArrayXYZUtils.AdditionJob_NativeArrayXYZ_float4(points, offset);
            var transformHandle = transformJob.Schedule(transformJob.Length, InnerLoopBatchCount);

            var pointCoordinates =
                new NativeArrayXYZ<uint>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            // Convert all points to node coordinates (The octree coordinates of each point)
            var coordinatesJob = new NativeArrayXYZUtils.IntegerDivisionJob_NativeArrayXYZ_float4_uint4(points,
                pointCoordinates, cellWidth);
            var coordinatesJobHandle = coordinatesJob
                .Schedule(coordinatesJob.Length, InnerLoopBatchCount, transformHandle);

            // Convert coordinates to morton codes (deallocates pointCoordinates)
            return new Morton64SoAEncodeJob(pointCoordinates, pointsMortonCodes)
                .Schedule(pointCoordinates.Length, InnerLoopBatchCount, coordinatesJobHandle);
        }

        JobHandle AddPointsToNode(ulong mortonCode, NativeArrayXYZ<float> points, NativeArray<ulong> mortonCodes, int pointsInNodeCount, NativeSparsePagedArrayXYZ storage)
        {
            var collectedPoints = new NativeArrayXYZ<float>(pointsInNodeCount, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);
            var collectPointIndices = new NativeArray<int>(pointsInNodeCount, Allocator.TempJob);

            Logger.Log($"[PointCloud] Adding {pointsInNodeCount} points to node {mortonCode}");

            // Collect point indices belonging to this node
            var collectPointIndicesJobHandle =
                new CollectPointIndicesJob(mortonCode, mortonCodes, collectPointIndices)
                    .Schedule();

            // Filter points that don't "fit"

            // Chop if too many points

            // Collect points
            var collectPointsJobHandle = new CollectPointsJob(collectPointIndices, points, collectedPoints)
                .Schedule(collectPointIndicesJobHandle);

            // Kick off jobs to add points to child nodes

            // Add node to the Octree if it doesn't exist
            var addNodeJobHandle = new TryAddNodeToStorageJob(mortonCode, storage)
                .Schedule(collectPointsJobHandle);

            // collectedPoints disposed on job completion
            return new AddDataToStorageJob(mortonCode, collectedPoints, storage, pointsInNodeCount)
                .Schedule(addNodeJobHandle);
        }

        public void Dispose()
        {
            Octree?.Dispose();
        }
    }
}