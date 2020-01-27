using InfPoints.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class PointCloudUtils
    {
        const int InnerLoopBatchCount = 128;

        public static void FilterMortonCodes()
        {
            
        }

        public static JobHandle ScheduleTransformPoints(XYZSoA<float4> xyz, float3 numberToAdd)
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

        public static JobHandle SchedulePointsToCoordinates(XYZSoA<float4> xyz, XYZSoA<uint4> coordinates,
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