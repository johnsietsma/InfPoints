using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints
{
    public static class NativeArrayXYZUtils
    {
        /// <summary>
        /// Convert a <seealso cref="NativeArray{T}"/> of points to a <seealso cref="NativeArrayXYZ{T}"/>.
        /// This allocates memory for the new array.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="allocator"></param>
        /// <returns></returns>
        public static NativeArrayXYZ<float> MakeNativeArrayXYZ(float3[] points, Allocator allocator)
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
            public readonly int Length; // The calculated Length, use when scheduling the job
            [ReadOnly] readonly float3 m_NumberToAdd;
            NativeArray<float4> m_ValuesX;
            NativeArray<float4> m_ValuesY;
            NativeArray<float4> m_ValuesZ;

            public AdditionJob_NativeArrayXYZ_float4(NativeArrayXYZ<float> values, float3 numberToAdd)
                : this(values, values.Length, numberToAdd)
            {
            }

            public AdditionJob_NativeArrayXYZ_float4(NativeArrayXYZ<float> values, int valuesLength, float3 numberToAdd)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                // Because of reinterpret to SIMD friendly types 
                if (values.Length % 4 != 0) throw new ArgumentException("Values must be added in multiples of 4");
#endif
                m_NumberToAdd = numberToAdd;
                int sizeOfFloat = UnsafeUtility.SizeOf<float>();
                m_ValuesX = values.X.Reinterpret<float4>(sizeOfFloat);
                m_ValuesY = values.Y.Reinterpret<float4>(sizeOfFloat);
                m_ValuesZ = values.Z.Reinterpret<float4>(sizeOfFloat);
                Length = valuesLength / sizeOfFloat;
            }

            public void Execute(int index)
            {
                m_ValuesX[index] += m_NumberToAdd.x;
                m_ValuesY[index] += m_NumberToAdd.y;
                m_ValuesZ[index] += m_NumberToAdd.z;
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct IntegerDivisionJob_NativeArrayXYZ_float4_uint4 : IJobParallelFor
        {
            public readonly int Length; // The calculated Length, use when scheduling the job
            [ReadOnly] readonly float4 m_Divisor;
            [ReadOnly] NativeArray<float4> m_ValuesX;
            [ReadOnly] NativeArray<float4> m_ValuesY;
            [ReadOnly] NativeArray<float4> m_ValuesZ;
            NativeArray<uint4> m_QuotientsX;
            NativeArray<uint4> m_QuotientsY;
            NativeArray<uint4> m_QuotientsZ;

            public IntegerDivisionJob_NativeArrayXYZ_float4_uint4(NativeArrayXYZ<float> values,
                NativeArrayXYZ<uint> quotients, float4 divisor)
                : this(values, values.Length, quotients, divisor)
            {
            }

            public IntegerDivisionJob_NativeArrayXYZ_float4_uint4(NativeArrayXYZ<float> values, int valuesLength,
                NativeArrayXYZ<uint> quotients, float4 divisor)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                // Because of reinterpret to SIMD friendly types 
                if (values.Length % 4 != 0) throw new ArgumentException("Values must be added in multiples of 4");
#endif

                int sizeOfFloat = UnsafeUtility.SizeOf<float>();
                m_Divisor = divisor;
                m_ValuesX = values.X.Reinterpret<float4>(sizeOfFloat);
                m_ValuesY = values.Y.Reinterpret<float4>(sizeOfFloat);
                m_ValuesZ = values.Z.Reinterpret<float4>(sizeOfFloat);
                m_QuotientsX = quotients.X.Reinterpret<uint4>(sizeOfFloat);
                m_QuotientsY = quotients.Y.Reinterpret<uint4>(sizeOfFloat);
                m_QuotientsZ = quotients.Z.Reinterpret<uint4>(sizeOfFloat);
                Length = valuesLength / sizeOfFloat;
            }

            public void Execute(int index)
            {
                m_QuotientsX[index] = (uint4) math.floor(m_ValuesX[index] / m_Divisor);
                m_QuotientsY[index] = (uint4) math.floor(m_ValuesY[index] / m_Divisor);
                m_QuotientsZ[index] = (uint4) math.floor(m_ValuesZ[index] / m_Divisor);
            }
        }
    }
}