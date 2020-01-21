using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public class PointCloud
    {
        SparseOctree<int> m_Octree;
        NativeHashMap<int, int> m_PointStorage;

        const int InnerLoopBatchCount = 1024;

        public PointCloud(AABB aabb)
        {
            m_Octree = new SparseOctree<int>(aabb, Allocator.Persistent);
            m_PointStorage = new NativeHashMap<int, int>(1024, Allocator.Persistent);
        }

        public void AddPoints(XYZSoA<float> points)
        {
            int levelIndex = 0;
            var pointsWide = points.Reinterpret<float4>();
            
            var transformHandle = TransformPoints(pointsWide, -m_Octree.AABB.Minimum);

            int cellCount = SparseOctree<int>.GetCellCount(levelIndex);
            float cellWidth = m_Octree.AABB.Size / cellCount;
            var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob);
            var coordinatesWide = coordinates.Reinterpret<uint4>();
            var pointsToCoordinatesHandle = PointsToCoordinates(pointsWide, coordinatesWide, cellCount, transformHandle);
            
            coordinates.Dispose();
        }

        static JobHandle TransformPoints(XYZSoA<float4> pointsWide, float3 numberToAdd)
        {
            // Convert points to Octree AABB space
            var spaceConvertJobX = new AdditionJob_float4()
            {
                Values = pointsWide.X,
                NumberToAdd = -numberToAdd.x
            };
            var spaceConvertJobY = new AdditionJob_float4()
            {
                Values = pointsWide.Y,
                NumberToAdd = -numberToAdd.y
            };
            var spaceConvertJobZ = new AdditionJob_float4()
            {
                Values = pointsWide.Z,
                NumberToAdd = -numberToAdd.z
            };

            return JobUtils.ScheduleMultiple(pointsWide.X.Length, InnerLoopBatchCount, spaceConvertJobX, spaceConvertJobY, spaceConvertJobZ);
        }

        static JobHandle PointsToCoordinates(XYZSoA<float4> points, XYZSoA<uint4> coordinates, float divisionAmount, JobHandle jobDeps)
        {
            var convertJobX = new IntegerDivisionJob_float4_uint4()
            {
                Values = points.X,
                Divisor = divisionAmount,
                Quotients = coordinates.X
            };
            
            var convertJobY = new IntegerDivisionJob_float4_uint4()
            {
                Values = points.Y,
                Divisor = divisionAmount,
                Quotients = coordinates.Y
            };
            
            var convertJobZ = new IntegerDivisionJob_float4_uint4()
            {
                Values = points.Z,
                Divisor = divisionAmount,
                Quotients = coordinates.Z
            };

            return JobUtils.ScheduleMultiple(points.Length, InnerLoopBatchCount, jobDeps, convertJobX, convertJobY,
                convertJobZ);
        }
    }
}