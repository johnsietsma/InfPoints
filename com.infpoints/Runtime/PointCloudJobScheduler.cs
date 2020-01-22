using InfPoints.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class PointCloudJobScheduler
    {
        const int InnerLoopBatchCount = 128;

        public static JobHandle ScheduleTransformPoints(XYZSoA<float4> pointsWide, float3 numberToAdd)
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

            return JobUtils.ScheduleMultiple(pointsWide.X.Length, InnerLoopBatchCount, spaceConvertJobX,
                spaceConvertJobY, spaceConvertJobZ);
        }

        public static JobHandle SchedulePointsToCoordinates(XYZSoA<float4> points, XYZSoA<uint4> coordinates,
            float divisionAmount)
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

            return JobUtils.ScheduleMultiple(points.Length, InnerLoopBatchCount, convertJobX, convertJobY,
                convertJobZ);
        }

        public static JobHandle ScheduleCoordinatesToMortonCode(XYZSoA<uint> coordinates, NativeArray<ulong> codes)
        {
            var mortonEncodeJob = new Morton64SoAEncodeJob()
            {
                CoordinatesX = coordinates.X,
                CoordinatesY = coordinates.Y,
                CoordinatesZ = coordinates.Z,
                Codes = codes
            };

            return mortonEncodeJob.Schedule(coordinates.Length, InnerLoopBatchCount);
        }

        public static JobHandle ScheduleCollectUniqueMortonCodes(NativeArray<ulong> codes, NativeHashMap<ulong, int> uniqueHash)
        {
            return new CollectUniqueJob<ulong>()
            {
                Values = codes,
                UniqueValues = uniqueHash
            }.Schedule();
        }

        public static JobHandle ScheduleAppendNodeFullFilter(NativeArray<NodeStorage> nodeStorage, JobHandle uniqueCodesHandle, NativeList<int> indices)
        {
            return new FilterFullNodesJob()
            {
                NodeStorage = nodeStorage
            }.ScheduleAppend(indices, nodeStorage.Length, InnerLoopBatchCount, uniqueCodesHandle);
        }
    }
}