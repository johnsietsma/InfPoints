using InfPoints.Jobs;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public class PointCloud
    {
        SparseOctree<ulong> m_Octree;

        const int InnerLoopBatchCount = 128;
        const int MaximumPointsPerNode = 1024 * 1024;

        public PointCloud(AABB aabb)
        {
            m_Octree = new SparseOctree<ulong>(aabb, MaximumPointsPerNode, Allocator.Persistent);
        }

        public void AddPoints(XYZSoA<float> points)
        {
            int levelIndex = 0;
            int cellCount = SparseOctree<int>.GetNodeCount(levelIndex);
            float cellWidth = m_Octree.AABB.Size / cellCount;

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
            var filterFullNodesHandle = new FilterFullNodesJob<ulong>()
            {
                MortonCodes = uniqueCodes,
                NodeStorage = m_Octree.GetNodeStorage(levelIndex).Storage
            }.ScheduleAppend(filteredMortonCodeIndices, mortonCodes.Length, InnerLoopBatchCount);
            filterFullNodesHandle.Complete();

            var collectedPoints = new NativeArray<float3>(mortonCodes.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var collectedPointsCount = new NativeArray<int>(1, Allocator.TempJob);
            // Filter points
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
                    CollectedPoints = collectedPoints,
                    CollectedPointsCount = collectedPointsCount
                }.ScheduleFilter(filteredMortonCodeIndices, InnerLoopBatchCount);
                
                collectJob.Complete();
            }

            collectedPointsCount.Dispose();
            collectedPoints.Dispose();
            filteredMortonCodeIndices.Dispose();
            uniqueCodes.Dispose();
            coordinates.Dispose();
            mortonCodes.Dispose();
        }
    }
}