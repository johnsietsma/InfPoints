using System;
using System.Diagnostics;
using InfPoints.Jobs;
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

            var coordinates =
                new NativeArrayXYZ<uint>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var pointsWide = points.Reinterpret<float4>();
            var coordinatesWide = coordinates.Reinterpret<uint4>();
            var mortonCodes =
                new NativeArray<ulong>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var uniqueCodesMap = new NativeHashMap<ulong, int>(mortonCodes.Length, Allocator.TempJob);
            var uniqueCodes = new NativeList<ulong>(Allocator.TempJob);
            var nodeStorage = Octree.GetNodeStorage(levelIndex);
            var validNodeIndices = new NativeList<int>(mortonCodes.Length, Allocator.TempJob);

            // Transform points from world to Octree AABB space
            var transformHandle =
                new NativeArrayXYZUtils.AdditionJob_NativeArrayXYZ_float4(pointsWide, -m_Octree.AABB.Minimum[0])
                    .Schedule(pointsWide.Length, InnerLoopBatchCount);

            // Convert all points to node coordinates
            var coordinatesJobHandle =
                new NativeArrayXYZUtils.IntegerDivisionJob_NativeArrayXYZ_float4_uint4(pointsWide, coordinatesWide,
                        cellWidth)
                    .Schedule(pointsWide.Length, InnerLoopBatchCount, transformHandle);

            // Convert coordinates to morton codes
            var mortonCodesJobHandle = new Morton64SoAEncodeJob(coordinates, mortonCodes)
                .Schedule(coordinates.Length, InnerLoopBatchCount, coordinatesJobHandle);

            // Get all unique codes and a count of how many of each there are
            var uniqueCodesMapHandle = new GetUniqueValuesJob<ulong>(mortonCodes, uniqueCodesMap)
                .Schedule(mortonCodesJobHandle);

            // Extract the codes
            var uniqueCodesHandle = new NativeHashMapGetKeysJob<ulong, int>(uniqueCodesMap, uniqueCodes)
                .Schedule(uniqueCodesMapHandle);
            uniqueCodesHandle.Complete();

            Logger.Log("[PointCloud] Unique codes " + uniqueCodes.Length);

            // Filter out full nodes
            var validNodesHandle = new FilterFullNodesJob<float>(nodeStorage, mortonCodes)
                .ScheduleAppend(validNodeIndices, uniqueCodes.Length, InnerLoopBatchCount);
            validNodesHandle.Complete();


            Logger.Log("[PointCloud] Valid indices " + validNodeIndices.Length);

            // For each node
            var collectJobHandles = new JobHandle[validNodeIndices.Length];
            for (int index = 0; index < validNodeIndices.Length; index++)
            {
                int mortonCodeIndex = validNodeIndices[index];
                ulong mortonCode = mortonCodes[mortonCodeIndex];
                int pointsInNodeCount = uniqueCodesMap[mortonCode];
                var collectedPoints = new NativeArrayXYZ<float>(pointsInNodeCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

                Logger.Log($"[PointCloud] Adding {pointsInNodeCount} points to node {mortonCode}");

                // Add node to the Octree if it doesn't exist
                var storage = Octree.GetNodeStorage(levelIndex);
                var addNodeJobHandle = new TryAddNodeToStorageJob(mortonCode, Octree.GetNodeStorage(levelIndex))
                    .Schedule();

                // Collect points
                var collectJobHandle = new CollectPointsJob(mortonCode, mortonCodes, points, collectedPoints)
                {
                    CodeKey = mortonCode,
                    Codes = mortonCodes,
                    PointsX = points.X,
                    PointsY = points.Y,
                    PointsZ = points.Z,
                    CollectedPointsX = collectedPoints.X,
                    CollectedPointsY = collectedPoints.Y,
                    CollectedPointsZ = collectedPoints.Z,
                }.Schedule(addNodeJobHandle);

                // collectedPoints disposed on job completion
                var addDataToStorageHandle =
                    new AddDataToStorageJob(mortonCode, collectedPoints, storage, pointsInNodeCount)
                        .Schedule(collectJobHandle);

                collectJobHandles[index] = addDataToStorageHandle;
            }

            JobUtils.CombineHandles(collectJobHandles).Complete();

            coordinates.Dispose();
            mortonCodes.Dispose();
            uniqueCodesMap.Dispose();
            uniqueCodes.Dispose();
            validNodeIndices.Dispose();
        }

        public void Dispose()
        {
            Octree?.Dispose();
        }
    }
}