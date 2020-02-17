﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class XYZSoAUtils
    {
        /// <summary>
        /// Convert a <seealso cref="NativeArray{T}"/> of points to a <seealso cref="NativeArrayXYZ{T}"/>.
        /// This allocates memory for the new array.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="allocator"></param>
        /// <returns></returns>
        public static NativeArrayXYZ<float> MakeNativeArrayXYZ(NativeArray<float3> points, Allocator allocator)
        {
            var xyzPoints = new NativeArrayXYZ<float>(points.Length, allocator);
            for (int index = 0; index < points.Length; index++)
            {
                var p = points[index];
                xyzPoints.X[index] = p.x;
                xyzPoints.Y[index] = p.y;
                xyzPoints.Z[index] = p.z;
            }

            return xyzPoints;
        }
        
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct AdditionJob_NativeArrayXYZ_float4 : IJobParallelFor
        {
            [ReadOnly] public float4 NumberToAdd;
            public NativeArray<float4> ValuesX;
            public NativeArray<float4> ValuesY;
            public NativeArray<float4> ValuesZ;

            public void Execute(int index)
            {
                ValuesX[index] += NumberToAdd;
                ValuesY[index] += NumberToAdd;
                ValuesZ[index] += NumberToAdd;
            }
        }
        
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct IntegerDivisionJob_NativeArrayXYZ_float4_uint4 : IJobParallelFor
        {
            [ReadOnly] public float4 Divisor;
            [ReadOnly] public NativeArray<float4> ValuesX;
            [ReadOnly] public NativeArray<float4> ValuesY;
            [ReadOnly] public NativeArray<float4> ValuesZ;
            public NativeArray<uint4> QuotientsX;
            public NativeArray<uint4> QuotientsY;
            public NativeArray<uint4> QuotientsZ;

            public void Execute(int index)
            {
                QuotientsX[index] = (uint4) math.floor(ValuesX[index] / Divisor);
                QuotientsY[index] = (uint4) math.floor(ValuesY[index] / Divisor);
                QuotientsZ[index] = (uint4) math.floor(ValuesZ[index] / Divisor);
            }
        }
    }
}