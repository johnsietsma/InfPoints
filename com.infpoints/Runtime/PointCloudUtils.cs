using InfPoints.Jobs;
using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class PointCloudUtils
    {
        const int InnerLoopBatchCount = 128;

        public static JobHandle SchedulePointsToCoordinates(XYZSoA<float> points, XYZSoA<uint> coordinates,
            float3 offset, float cellWidth, JobHandle deps = default)
        {
            // Transform points from world to Octree AABB space
            var pointsWide = points.Reinterpret<float4>();
            var transformHandle = ScheduleTransformPoints(pointsWide, offset, deps);

            // Convert all points to node coordinates
            var coordinatesWide = coordinates.Reinterpret<uint4>();
            var pointsToCoordinatesHandle =
                SchedulePointsToCoordinates(pointsWide, coordinatesWide, cellWidth, transformHandle);
            return pointsToCoordinatesHandle;
        }

        public static JobHandle ScheduleEncodeMortonCodes(XYZSoA<uint> coordinates, NativeArray<ulong> mortonCodes,
            JobHandle deps = default)
        {
            return new Morton64SoAEncodeJob()
            {
                CoordinatesX = coordinates.X,
                CoordinatesY = coordinates.Y,
                CoordinatesZ = coordinates.Z,
                Codes = mortonCodes
            }.Schedule(coordinates.Length, InnerLoopBatchCount, deps);
        }

        public static JobHandle ScheduleGetUniqueCodes(NativeArray<ulong> codes,
            NativeHashMap<ulong, uint> uniqueCodesMap, NativeList<ulong> uniqueCodes, JobHandle deps = default)
        {
            var uniqueCodesMapHandle = new GetUniqueValuesJob<ulong>()
            {
                Values = codes,
                UniqueValues = uniqueCodesMap.AsParallelWriter()
            }.Schedule(codes.Length, InnerLoopBatchCount, deps);

            return new NativeHashMapGetKeysJob<ulong, uint>()
            {
                NativeHashMap = uniqueCodesMap,
                Keys = uniqueCodes
            }.Schedule(uniqueCodesMapHandle);
        }

        public static JobHandle FilterFullNodes(NativeArray<ulong> mortonCodes, NativeNodeStorage nodeStorage,
            NativeList<int> notFullNodeIndices, JobHandle deps = default)
        {
            return new FilterFullNodesJob<float>()
            {
                MortonCodes = mortonCodes,
                NodeStorage = nodeStorage
            }.ScheduleAppend(notFullNodeIndices, mortonCodes.Length, InnerLoopBatchCount, deps);
        }

        static JobHandle ScheduleTransformPoints(XYZSoA<float4> xyz, float3 numberToAdd, JobHandle deps = default)
        {
            // Convert points to Octree AABB space
            return new XYZSoAUtils.AdditionJob_XYZSoA_float4()
            {
                ValuesX = xyz.X,
                ValuesY = xyz.Y,
                ValuesZ = xyz.Z,
                NumberToAdd = -numberToAdd[0]
            }.Schedule(xyz.Length, InnerLoopBatchCount, deps);
        }

        static JobHandle SchedulePointsToCoordinates(XYZSoA<float4> xyz, XYZSoA<uint4> coordinates,
            float divisionAmount, JobHandle deps)
        {
            // Convert points to Octree AABB space
            return new XYZSoAUtils.IntegerDivisionJob_XYZSoA_float4_uint4()
            {
                ValuesX = xyz.X,
                ValuesY = xyz.Y,
                ValuesZ = xyz.Z,
                Divisor = divisionAmount,
                QuotientsX = coordinates.X,
                QuotientsY = coordinates.Y,
                QuotientsZ = coordinates.Z
            }.Schedule(xyz.Length, InnerLoopBatchCount, deps);
        }
    }
}