using JacksonDunstan.NativeCollections;
using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    public class PointCloud
    {
        SparseOctree<Node> m_Octree;
        NativeHashMap<Node, NodeStorage> m_PointStorage;


        public PointCloud(AABB aabb)
        {
            m_Octree = new SparseOctree<Node>(aabb, Allocator.Persistent);
            m_PointStorage = new NativeHashMap<Node, NodeStorage>(1024, Allocator.Persistent);
        }

        public void AddPoints(XYZSoA<float> points)
        {
            int levelIndex = 0;
            int cellCount = SparseOctree<int>.GetCellCount(levelIndex);
            float cellWidth = m_Octree.AABB.Size / cellCount;

            var pointsWide = points.Reinterpret<float4>();
            var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob);
            var coordinatesWide = coordinates.Reinterpret<uint4>();
            var codes = new NativeArray<ulong>(points.Length, Allocator.TempJob);
            var uniqueCodes = new NativeHashSet<ulong>(cellCount, Allocator.TempJob);
            var indices = new NativeList<int>();

            var transformHandle = PointCloudJobScheduler.ScheduleTransformPoints(pointsWide, -m_Octree.AABB.Minimum);
            var pointsToCoordinatesHandle = PointCloudJobScheduler.SchedulePointsToCoordinates(pointsWide, coordinatesWide, cellWidth);
            var mortonCodeHandle = PointCloudJobScheduler.ScheduleCoordinatesToMortonCode(coordinates, codes);
            var uniqueCodesHandle = PointCloudJobScheduler.ScheduleCollectUniqueMortonCodes(codes, uniqueCodes);
            var uniqueFilteredCodeHandle = PointCloudJobScheduler.ScheduleAppendNodeFullFilter(m_PointStorage.GetValueArray(Allocator.Temp), uniqueCodesHandle, indices);
            
            coordinates.Dispose();
            codes.Dispose();
        }
    }
}