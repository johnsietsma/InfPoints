using System;
using InfPoints.Jobs;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints
{
    public class PointCloud : IDisposable
    {
        SparseOctree<float> m_Octree;

        const int InnerLoopBatchCount = 128;
        const int MaximumPointsPerNode = 1024 * 1024;

        public PointCloud(AABB aabb)
        {
            m_Octree = new SparseOctree<float>(aabb, MaximumPointsPerNode, Allocator.Persistent);
        }

        public void AddPoints(XYZSoA<float> points)
        {
            int levelIndex = 0;
            int cellCount = SparseOctreeUtils.GetNodeCount(levelIndex);
            float cellWidth = m_Octree.AABB.Size / cellCount;

            const int MaximumPointsPerNode = 1024;
            int MaximumPointsPerLevel = MaximumPointsPerNode * cellCount;
            
            m_Octree.AddLevel(MaximumPointsPerLevel);
            
            var outsideCount = new NativeInterlockedInt(0, Allocator.TempJob);
            // Check points are inside AABB
            var arePointsInsideJob = new ArePointsInsideAABBJob()
            {
                aabb = m_Octree.AABB,
                Points = points,
                OutsideCount = outsideCount
            }.Schedule(points.Length, InnerLoopBatchCount);
            arePointsInsideJob.Complete();
            if (outsideCount.Value > 0)
            {
                UnityEngine.Debug.Log("Adding points outside the AABB, aborting");
            }
            
            outsideCount.Dispose();

            // Transform each point to a coordinate within the AABB
            var coordinates = PointCloudUtils.PointsToCoordinates(points, -m_Octree.AABB.Minimum, cellWidth);
            
            // Convert unique coordinates to morton codes
            var mortonCodes = new NativeArray<ulong>(points.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var mortonCodeHandle = new Morton64SoAEncodeJob()
            {
                CoordinatesX = coordinates.X,
                CoordinatesY = coordinates.Y,
                CoordinatesZ = coordinates.Z,
                Codes = mortonCodes
            }.Schedule(coordinates.Length, InnerLoopBatchCount);
            mortonCodeHandle.Complete();

            // Get all unique codes
            var uniqueCodes = PointCloudUtils.GetUniqueCodes(mortonCodes);

            // Filter out full nodes
            NativeList<int> filteredMortonCodeIndices = new NativeList<int>(mortonCodes.Length, Allocator.TempJob);
            var filterFullNodesHandle = new FilterFullNodesJob<float>()
            {
                MortonCodes = uniqueCodes,
                NodeStorage = m_Octree.GetNodeStorage(levelIndex)
            }.ScheduleAppend(filteredMortonCodeIndices, mortonCodes.Length, InnerLoopBatchCount);
            filterFullNodesHandle.Complete();

            var collectedPoints = new XYZSoA<float>(mortonCodes.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var collectedPointsCount = new NativeArray<int>(1, Allocator.TempJob);
            
            // For each node
            for (int index = 0; index < filteredMortonCodeIndices.Length; index++)
            {
                int mortonCodeIndex = filteredMortonCodeIndices[index];
                ulong mortonCode = mortonCodes[mortonCodeIndex];
                
                // Collect points
                var collectJob = new CollectPointsJob()
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
                }.ScheduleFilter(filteredMortonCodeIndices, InnerLoopBatchCount);
                collectJob.Complete();

                // Add them to the octree
                var storage = m_Octree.GetNodeStorage(0);
                if (!storage.ContainsNode(mortonCode))
                {
                    storage.AddNode(mortonCode);
                }
                
                storage.AddData(mortonCode, collectedPoints);
            }

            collectedPointsCount.Dispose();
            collectedPoints.Dispose();
            filteredMortonCodeIndices.Dispose();
            uniqueCodes.Dispose();
            coordinates.Dispose();
            mortonCodes.Dispose();
        }

        public void Dispose()
        {
            m_Octree?.Dispose();
        }
    }
}