using InfPoints.Jobs;
using JacksonDunstan.NativeCollections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class PointCloudJobScheduler
    {
        const int InnerLoopBatchCount = 128;

        public static JobHandle ScheduleTransformPoints(Float3SoA<float4> float3, float3 numberToAdd)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(3, Allocator.Temp); 
            for (int index=0; index<3; index++)
            {
                // Convert points to Octree AABB space
                jobHandles[index] = new AdditionJob_float4()
                {
                    Values = float3[index],
                    NumberToAdd = -numberToAdd[index]
                }.Schedule(float3.Length, InnerLoopBatchCount);
            }

            var combinedHandle = JobHandle.CombineDependencies(jobHandles);
            jobHandles.Dispose();
            return combinedHandle;
        }

        public static JobHandle SchedulePointsToCoordinates(Float3SoA<float4> float3, Float3SoA<uint4> coordinates,
            float divisionAmount)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(3, Allocator.Temp); 
            for (int index=0; index<3; index++)
            {
                // Convert points to Octree AABB space
                jobHandles[index] = new IntegerDivisionJob_float4_uint4()
                {
                    Values = float3[index],
                    Divisor = divisionAmount,
                    Quotients = coordinates[index]
                }.Schedule(float3.Length, InnerLoopBatchCount);
            }

            var combinedHandle = JobHandle.CombineDependencies(jobHandles);
            jobHandles.Dispose();
            return combinedHandle;
        }
    }
}