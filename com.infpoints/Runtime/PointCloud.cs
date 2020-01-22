using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    public class PointCloud
    {
        SparseOctree<int> m_Octree;
        NativeHashMap<int, int> m_PointStorage;


        public PointCloud(AABB aabb)
        {
            m_Octree = new SparseOctree<int>(aabb, Allocator.Persistent);
            m_PointStorage = new NativeHashMap<int, int>(1024, Allocator.Persistent);
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
            var uniqueCodes = new NativeHashMap<ulong,int>(cellCount, Allocator.TempJob);


            var transformHandle = PointCloudJobs.ScheduleTransformPoints(pointsWide, -m_Octree.AABB.Minimum);
            var pointsToCoordinatesHandle = PointCloudJobs.SchedulePointsToCoordinates(pointsWide, coordinatesWide, cellWidth);
            var mortonCodeHandle = PointCloudJobs.ScheduleCoordinatesToMortonCode(coordinates, codes);
            var uniqueCodesHandle = PointCloudJobs.ScheduleCollectUniqueMortonCode(codes, uniqueCodes);
            
            coordinates.Dispose();
            codes.Dispose();
        }
        
    }
}