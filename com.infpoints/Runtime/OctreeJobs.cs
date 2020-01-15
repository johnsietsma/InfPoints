﻿using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class OctreeJobs
    {
        public static JobHandle ScheduleConvertPointsToCoordsJobs(NativeArray<float3> points, NativeArray<uint3> coords, AABB aabb,
            int cellCount, int batchCount)
        {
            var addJob = new AddJob_float3()
            {
                Add = -aabb.Minimum,
                Data = points
            };

            var convertJob = new QuotientDivideJob()
            {
                CellSize = aabb.Size / cellCount,
                Points = points,
                Coords = coords
            };

            var addJobData = addJob.Schedule(points.Length, batchCount);
            return convertJob.Schedule(points.Length, batchCount, addJobData);
        }
        
        public static JobHandle ScheduleConvertPointsToCoordsJobs(NativeArray<float4x3> points, NativeArray<uint4x3> coords, AABB aabb,
            int cellCount, int batchCount)
        {
            float3 min = -aabb.Minimum;
            var addJob = new AddJob_float4x3()
            {
                Add = new float4x3(min.x, min.y, min.z), 
                Data = points
            };
            
            var convertJob = new QuotientDivideWideJob()
            {
                CellSize = aabb.Size / cellCount,
                Points = points,
                Coords = coords
            };

            var addJobData = addJob.Schedule(points.Length, batchCount);
            return convertJob.Schedule(points.Length, batchCount, addJobData);
        }
    }


    public struct AddJob_float3 : IJobParallelFor
    {
        [ReadOnly] public float3 Add;
        public NativeArray<float3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + Add;
        }
    }
    
    public struct AddJob_float4x3 : IJobParallelFor
    {
        [ReadOnly] public float4x3 Add;
        public NativeArray<float4x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + Add;
        }
    }
    
    public struct QuotientDivideJob : IJobParallelFor
    {
        [ReadOnly] public float CellSize;
        [ReadOnly] public NativeArray<float3> Points;

        public NativeArray<uint3> Coords;

        public void Execute(int index)
        {
            Coords[index] = Points[index].QuotientDivide(CellSize);
        }
    }
    
    public struct QuotientDivideWideJob : IJobParallelFor
    {
        [ReadOnly] public float CellSize;
        [ReadOnly] public NativeArray<float4x3> Points;

        public NativeArray<uint4x3> Coords;

        public void Execute(int index)
        {
            Coords[index] = Points[index].QuotientDivide(CellSize);
        }
    }
}
