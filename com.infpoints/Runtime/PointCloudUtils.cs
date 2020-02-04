using InfPoints.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class PointCloudUtils
    {
        const int InnerLoopBatchCount = 128;

        public static XYZSoA<uint> PointsToCoordinates(XYZSoA<float> points, float3 offset, float cellWidth)
        {
            // Transform points from world to Octree AABB space
            var pointsWide = points.Reinterpret<float4>();
            var transformHandle = PointCloudUtils.ScheduleTransformPoints(pointsWide, offset);

            // Convert all points to node coordinates
            var coordinates = new XYZSoA<uint>(points.Length, Allocator.TempJob);
            var coordinatesWide = coordinates.Reinterpret<uint4>();
            var pointsToCoordinatesHandle = PointCloudUtils.SchedulePointsToCoordinates(pointsWide, coordinatesWide, cellWidth);
            pointsToCoordinatesHandle.Complete();
            return coordinates;
        }
        
        public static NativeArray<ulong> GetUniqueCodes(NativeArray<ulong> codes )
        {
            using (var uniqueCoordinatesMap = new NativeHashMap<ulong, uint>(codes.Length, Allocator.TempJob))
            {
                var uniqueCoordinatesHandle = new GetUniqueValuesJob<ulong>()
                {
                    Values = codes,
                    UniqueValues = uniqueCoordinatesMap
                }.Schedule(codes.Length, InnerLoopBatchCount);
                uniqueCoordinatesHandle.Complete();
                return uniqueCoordinatesMap.GetKeyArray(Allocator.TempJob);
            }
        }
        
        static JobHandle ScheduleTransformPoints(XYZSoA<float4> xyz, float3 numberToAdd)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(3, Allocator.Temp); 
            for (int index=0; index<3; index++)
            {
                // Convert points to Octree AABB space
                jobHandles[index] = new AdditionJob_float4()
                {
                    Values = xyz[index],
                    NumberToAdd = -numberToAdd[index]
                }.Schedule(xyz.Length, InnerLoopBatchCount);
            }

            var combinedHandle = JobHandle.CombineDependencies(jobHandles);
            jobHandles.Dispose();
            return combinedHandle;
        }

        static JobHandle SchedulePointsToCoordinates(XYZSoA<float4> xyz, XYZSoA<uint4> coordinates,
            float divisionAmount)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(3, Allocator.Temp); 
            for (int index=0; index<3; index++)
            {
                // Convert points to Octree AABB space
                jobHandles[index] = new IntegerDivisionJob_float4_uint4()
                {
                    Values = xyz[index],
                    Divisor = divisionAmount,
                    Quotients = coordinates[index]
                }.Schedule(xyz.Length, InnerLoopBatchCount);
            }

            var combinedHandle = JobHandle.CombineDependencies(jobHandles);
            jobHandles.Dispose();
            return combinedHandle;
        }
    }
}