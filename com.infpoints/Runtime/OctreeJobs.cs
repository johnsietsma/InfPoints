using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    /*
    public static class OctreeJobs
    {
        /// <summary>
        /// Schedule jobs for converting points to coordinates.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="coords"></param>
        /// <param name="aabb"></param>
        /// <param name="cellCount"></param>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        public static JobHandle ScheduleConvertPointsToCoordsJobs(NativeArray<float3> points, NativeArray<uint3> coords, AABB aabb,
            int cellCount, int batchCount)
        {
            // Move the points relative to the AABB
            var addJob = new AdditionJob_float3()
            {
                NumberToAdd = -aabb.Minimum,
                Data = points
            };

            // Find out which cell the point is in
            var convertJob = new QuotientDivideJob()
            {
                CellSize = aabb.Size / cellCount,
                Points = points,
                Coords = coords
            };

            return addJob.Schedule(points.Length, batchCount);
            //return convertJob.Schedule(points.Length, batchCount, addJobData);
        }
        
        public static JobHandle ScheduleConvertPointsToCoordsJobs(NativeArray<float4x3> points, NativeArray<uint4x3> coords, AABB aabb,
            int cellCount, int batchCount)
        {
            float3 min = -aabb.Minimum;
            var addJob = new AdditionJob_float4x3()
            {
                NumberToAdd = new float4x3(min.x, min.y, min.z), 
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
*/
}
