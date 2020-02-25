using System;
using System.Diagnostics;
using InfPoints.Jobs;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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

            Logger.Log($"[PointCloud] Adding {points.Length} points to the octree with AABB {Octree.AABB}.");

            int levelIndex = Octree.AddLevel() - 1;
            int cellCount = SparseOctreeUtils.GetNodeCount(levelIndex);
            float cellWidth = Octree.AABB.Size / cellCount;

            Logger.Log($"[PointCloud]Cell count:{cellCount} Cell width:{cellWidth}");

            // The octree coordinates of each point
            var pointCoordinates =
                new NativeArrayXYZ<uint>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            // The node index of each point
            var pointNodeIndex =
                new NativeArray<ulong>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            // The node indices with no duplicates
            var uniqueNodeIndices = new NativeList<ulong>(Allocator.TempJob);
            // A map of node indices to the number of points in each node
            var uniqueNodeCodesMap = new NativeHashMap<ulong, int>(pointNodeIndex.Length, Allocator.TempJob);
            // The index into `uniqueNodeIndices` of each node that is valid, eg not full.            
            var validUniqueNodeIndices = new NativeList<int>(pointNodeIndex.Length, Allocator.TempJob);
            var nodeStorage = Octree.GetNodeStorage(levelIndex);

            // Transform points from world to Octree AABB space
            var transformJob =
                new NativeArrayXYZUtils.AdditionJob_NativeArrayXYZ_float4(points, -m_Octree.AABB.Minimum);
            var transformHandle = transformJob.Schedule(transformJob.ValuesX.Length, InnerLoopBatchCount);

            // Convert all points to node coordinates
            var coordinatesJob = new NativeArrayXYZUtils.IntegerDivisionJob_NativeArrayXYZ_float4_uint4(points,
                pointCoordinates, cellWidth);
            var coordinatesJobHandle =
                coordinatesJob.Schedule(coordinatesJob.ValuesX.Length, InnerLoopBatchCount, transformHandle);

            // Convert coordinates to morton codes
            var mortonCodesJobHandle = new Morton64SoAEncodeJob(pointCoordinates, pointNodeIndex)
                .Schedule(pointCoordinates.Length, InnerLoopBatchCount, coordinatesJobHandle);

            // Get all unique codes and a count of how many of each there are
            var uniqueCodesMapHandle = new GetUniqueValuesJob<ulong>(pointNodeIndex, uniqueNodeCodesMap)
                .Schedule(mortonCodesJobHandle);

            // Extract the codes
            var uniqueCodesHandle = new NativeHashMapGetKeysJob<ulong, int>(uniqueNodeCodesMap, uniqueNodeIndices)
                .Schedule(uniqueCodesMapHandle);
            uniqueCodesHandle.Complete();

            // Filter out full nodes
            var validNodesHandle = new FilterFullNodesJob<float>(nodeStorage, uniqueNodeIndices)
                .ScheduleAppend(validUniqueNodeIndices, uniqueNodeIndices.Length, InnerLoopBatchCount);
            validNodesHandle.Complete();

            Logger.Log("[PointCloud] Valid indices " + validUniqueNodeIndices.Length);

            // For each node
            var collectJobHandles = new JobHandle[validUniqueNodeIndices.Length];
            for (int index = 0; index < validUniqueNodeIndices.Length; index++)
            {
                int nodeCodeIndex = validUniqueNodeIndices[index];
                ulong nodeCode = uniqueNodeIndices[nodeCodeIndex];
                int pointsInNodeCount = uniqueNodeCodesMap[nodeCode];

                collectJobHandles[index] = AddPointsToNode(nodeCode, points,
                    pointNodeIndex, pointsInNodeCount, nodeStorage);
            }

            JobUtils.CombineHandles(collectJobHandles).Complete();

            pointCoordinates.Dispose();
            pointNodeIndex.Dispose();
            uniqueNodeCodesMap.Dispose();
            uniqueNodeIndices.Dispose();
            validUniqueNodeIndices.Dispose();
        }

        JobHandle AddPointsToNode(ulong mortonCode, NativeArrayXYZ<float> points,
            NativeArray<ulong> pointsMortonCodes,
            int pointsInNodeCount, NativeSparsePagedArrayXYZ storage)
        {
            var collectedPoints = new NativeArrayXYZ<float>(pointsInNodeCount, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);
            var collectPointIndices = new NativeArray<int>(pointsInNodeCount, Allocator.TempJob);

            Logger.Log($"[PointCloud] Adding {pointsInNodeCount} points to node {mortonCode}");

            // Collect point indices belonging to this node
            var collectPointIndicesJobHandle =
                new CollectPointIndicesJob(mortonCode, pointsMortonCodes, collectPointIndices)
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