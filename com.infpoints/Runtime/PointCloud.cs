using System;
using System.Diagnostics;
using InfPoints.Jobs;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

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
#endif

            UnityEngine.Debug.Log($"Adding {points.Length} points to the octree with AABB {Octree.AABB}.");

            int levelIndex = 0;
            int cellCount = SparseOctreeUtils.GetNodeCount(levelIndex);
            float cellWidth = Octree.AABB.Size / cellCount;

            Logger.Log($"Cell count:{cellCount} Cell width:{cellWidth}");

            Octree.AddLevel();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            using (var outsideCount = new NativeInt(0, Allocator.TempJob))
            {
                // Check points are inside AABB
                var arePointsInsideJob = new ArePointsInsideAABBJob()
                {
                    aabb = Octree.AABB,
                    Points = points,
                    OutsideCount = outsideCount
                }.Schedule(points.Length, InnerLoopBatchCount);
                arePointsInsideJob.Complete();
                if (outsideCount.Value > 0)
                {
                    Logger.Log("Adding points outside the AABB, aborting");
                    return;
                }
            }
#endif

            // Transform each point to a coordinate within the AABB
            var coordinates =
                new NativeArrayXYZ<uint>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var coordinatesJobHandle =
                PointCloudUtils.SchedulePointsToCoordinates(points, coordinates, m_Octree.AABB.Minimum, cellWidth);

            // Convert coordinates to morton codes
            var mortonCodes =
                new NativeArray<ulong>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var mortonCodesJobHandle =
                PointCloudUtils.ScheduleEncodeMortonCodes(coordinates, mortonCodes, coordinatesJobHandle);

            // Get all unique codes
            var uniqueCoordinatesMap = new NativeHashMap<ulong, uint>(mortonCodes.Length, Allocator.TempJob);
            NativeList<ulong> uniqueCodes = new NativeList<ulong>(Allocator.TempJob);
            var uniqueCodesHandle = PointCloudUtils.ScheduleGetUniqueCodes(mortonCodes, uniqueCoordinatesMap,
                uniqueCodes, mortonCodesJobHandle);
            uniqueCodesHandle.Complete();

            // Filter out full nodes
            var nodeStorage = Octree.GetNodeStorage(levelIndex);
            NativeList<int> validNodeIndices = new NativeList<int>(mortonCodes.Length, Allocator.TempJob);
            var validNodesHandle = PointCloudUtils.FilterFullNodes(uniqueCodes, nodeStorage, validNodeIndices);
            validNodesHandle.Complete();


            // For each node
            var collectJobHandles = new JobHandle[validNodeIndices.Length];
            for (int index = 0; index < validNodeIndices.Length; index++)
            {
                int mortonCodeIndex = validNodeIndices[index];
                ulong mortonCode = mortonCodes[mortonCodeIndex];
                
                // Add node to the Octree if it doesn't exist
                var storage = Octree.GetNodeStorage(levelIndex);
                var addNodeJobHandle = new AddNodeToStorageJob()
                {
                    SparseIndex = mortonCode,
                    Storage = Octree.GetNodeStorage(levelIndex)
                }.Schedule(validNodesHandle);
                
                var collectedPoints = new NativeArrayXYZ<float>(mortonCodes.Length, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);
                var collectedPointsCount = new NativeInt(Allocator.TempJob);

                // Collect points
                var collectJobHandle = new CollectPointsJob()
                {
                    CodeKey = mortonCode,
                    Codes = uniqueCodes,
                    PointsX = points.X,
                    PointsY = points.Y,
                    PointsZ = points.Z,
                    CollectedPointsX = collectedPoints.X,
                    CollectedPointsY = collectedPoints.Y,
                    CollectedPointsZ = collectedPoints.Z,
                    CollectedPointsCount = collectedPointsCount
                }.ScheduleFilter(validNodeIndices, InnerLoopBatchCount, addNodeJobHandle);

                var addDataToStorageHandle = new AddDataToStorageJob()
                {
                    SparseIndex = mortonCode,
                    Data = collectedPoints, // Deallocate on job completion
                    Count = collectedPointsCount, // Deallocate on job completion
                    Storage = storage
                }.Schedule(collectJobHandle);

                collectJobHandles[index] = addDataToStorageHandle;
            }

            JobUtils.CombineHandles(collectJobHandles).Complete();

            validNodeIndices.Dispose();
            uniqueCodes.Dispose();
            uniqueCoordinatesMap.Dispose();
            coordinates.Dispose();
            mortonCodes.Dispose();
        }

        public void Dispose()
        {
            Octree?.Dispose();
        }
    }
}
