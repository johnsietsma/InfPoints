using InfPoints.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public class PointCloud
    {
        SparseOctree<Node> m_Octree;
        
        // A way to get from a node to its points
        NativeHashMap<Node, PointsStorage> m_PointStorage;

        const int InnerLoopBatchCount = 128;

        public PointCloud(AABB aabb)
        {
            m_Octree = new SparseOctree<Node>(aabb, Allocator.Persistent);
            m_PointStorage = new NativeHashMap<Node, PointsStorage>(1024, Allocator.Persistent);
        }

        public void AddPoints(XYZSoA<float> points)
        {
            int levelIndex = 0;
            int cellCount = SparseOctree<int>.GetCellCount(levelIndex);
            float cellWidth = m_Octree.AABB.Size / cellCount;

            // Transform points from world to Octree AABB space
            var pointsWide = points.Reinterpret<float4>();
            var transformHandle = PointCloudUtils.ScheduleTransformPoints(pointsWide, -m_Octree.AABB.Minimum);
            
            // Convert all points to node coordinates
            var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob);
            var coordinatesWide = coordinates.Reinterpret<uint4>();
            var pointsToCoordinatesHandle = PointCloudUtils.SchedulePointsToCoordinates(pointsWide, coordinatesWide, cellWidth);
            
            // Get all unique coordinates
            var mortonCodes = new NativeArray<ulong>(points.Length, Allocator.TempJob);
            var uniqueNodesIndices = new NativeList<int>();
            var uniqueCoordinatesMap = new NativeHashMap<uint3,uint>(cellCount, Allocator.TempJob);
            var uniqueCoordinatesHandle = new FilterUniqueUint3SoAJob()
            {
                X = coordinates.X,
                Y = coordinates.Y,
                Z = coordinates.Z,
                UniqueValues = uniqueCoordinatesMap
            }.ScheduleAppend(uniqueNodesIndices, mortonCodes.Length, InnerLoopBatchCount);

            var uniqueCoordinates = uniqueCoordinatesMap.GetKeyArray(Allocator.TempJob);
            
            // Convert unique coordinates to morton codes
            var mortonCodeHandle = new Morton64EncodeJob()
            {
                Coordinates = uniqueCoordinates,
                Codes = mortonCodes
            }.Schedule(uniqueCoordinates.Length, InnerLoopBatchCount);

            // Filter full nodes
            /*
            var uniqueFilteredCodeHandle = new FilterFullNodesJob()
            {
                NodePointsMap = m_PointStorage,
                MortonCodes = mortonCodes,
                LevelIndex = levelIndex
            }.ScheduleAppend(uniqueNodesIndices, mortonCodes.Length, InnerLoopBatchCount);
*/
            uniqueCoordinates.Dispose();
            uniqueCoordinatesMap.Dispose();
            coordinates.Dispose();
            mortonCodes.Dispose();
        }
    }
}