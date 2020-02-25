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
            [ReadOnly] public float3 NumberToAdd;
            public NativeArray<float4> ValuesX;
            public NativeArray<float4> ValuesY;
            public NativeArray<float4> ValuesZ;

            public AdditionJob_NativeArrayXYZ_float4(NativeArrayXYZ<float> values, float3 numberToAdd)
            {
                NumberToAdd = numberToAdd;
                ValuesX = values.X.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
                ValuesY = values.Y.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
                ValuesZ = values.Z.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
            }

            public void Execute(int index)
            {
                ValuesX[index] += NumberToAdd.x;
                ValuesY[index] += NumberToAdd.y;
                ValuesZ[index] += NumberToAdd.z;
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

            public IntegerDivisionJob_NativeArrayXYZ_float4_uint4(NativeArrayXYZ<float> values,
                NativeArrayXYZ<uint> quotients, float4 divisor)
            {
                Divisor = divisor;
                ValuesX = values.X.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
                ValuesY = values.Y.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
                ValuesZ = values.Z.Reinterpret<float4>(UnsafeUtility.SizeOf<float>());
                QuotientsX = quotients.X.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>());
                QuotientsY = quotients.Y.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>());
                QuotientsZ = quotients.Z.Reinterpret<uint4>(UnsafeUtility.SizeOf<uint>());
            }

            public void Execute(int index)
            {
                QuotientsX[index] = (uint4) math.floor(ValuesX[index] / Divisor);
                QuotientsY[index] = (uint4) math.floor(ValuesY[index] / Divisor);
                QuotientsZ[index] = (uint4) math.floor(ValuesZ[index] / Divisor);
            }
        }
    }
}