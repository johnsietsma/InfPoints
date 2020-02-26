using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IsWithinDistanceJob : IJobParallelFor
    {
        [ReadOnly] NativeArray<float3> Points;
        [ReadOnly] float3 Point;
        [ReadOnly] float DistanceSquared;
        public NativeInt.Concurrent WithinDistanceCount;

        public IsWithinDistanceJob(NativeArray<float3> points, float3 point, float distance,
            NativeInt withinDistanceCount)
        {
            Points = points;
            Point = point;
            DistanceSquared = math.pow(distance, 2);
            WithinDistanceCount = withinDistanceCount.ToConcurrent();
        }
        
        public void Execute(int index)
        {
            if(math.distancesq(Points[index], Point) <= DistanceSquared) WithinDistanceCount.Increment();
        }
    }
    
    public struct IsWithinDistanceJob_NativeArrayXYZ : IJobParallelFor
    {
        public int Length => PointsX.Length;
        [ReadOnly] NativeArray<float4> PointsX;
        [ReadOnly] NativeArray<float4> PointsY;
        [ReadOnly] NativeArray<float4> PointsZ;
        [ReadOnly] float3 Point;
        [ReadOnly] float DistanceSquared;
        public NativeInt.Concurrent WithinDistanceCount;

        public IsWithinDistanceJob_NativeArrayXYZ(NativeArrayXYZ<float> points, float3 point, float distance,
            NativeInt withinDistanceCount)
        {
            PointsX = points.X.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
            PointsY = points.Y.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
            PointsZ = points.Z.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
            Point = point;
            DistanceSquared = math.pow(distance, 2);
            WithinDistanceCount = withinDistanceCount.ToConcurrent();
        }
        
        public void Execute(int index)
        {
            float4 xDelta = PointsX[index] - Point.x;
            float4 yDelta = PointsY[index] - Point.y;
            float4 zDelta = PointsZ[index] - Point.z;
            float4 dotResult = xDelta * xDelta + yDelta * yDelta + zDelta * zDelta;
            dotResult -= DistanceSquared;
            dotResult = math.clamp(dotResult, -1, 0);
            dotResult = math.sign(dotResult);
            WithinDistanceCount.Add(-(int)math.csum(dotResult));
        }
    }
}
