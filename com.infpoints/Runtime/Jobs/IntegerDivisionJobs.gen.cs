﻿
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

// Disable warnings due to naming with numeric types and generated members not being used
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedType.Global

namespace InfPoints.Jobs
{
    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double_int : IJobParallelFor
    {
        [ReadOnly] public double Divisor;
        [ReadOnly] public NativeArray<double> Values;
        public NativeArray<int> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double_uint : IJobParallelFor
    {
        [ReadOnly] public double Divisor;
        [ReadOnly] public NativeArray<double> Values;
        public NativeArray<uint> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double2_int2 : IJobParallelFor
    {
        [ReadOnly] public double2 Divisor;
        [ReadOnly] public NativeArray<double2> Values;
        public NativeArray<int2> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int2) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double2_uint2 : IJobParallelFor
    {
        [ReadOnly] public double2 Divisor;
        [ReadOnly] public NativeArray<double2> Values;
        public NativeArray<uint2> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint2) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double3_int3 : IJobParallelFor
    {
        [ReadOnly] public double3 Divisor;
        [ReadOnly] public NativeArray<double3> Values;
        public NativeArray<int3> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int3) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double3_uint3 : IJobParallelFor
    {
        [ReadOnly] public double3 Divisor;
        [ReadOnly] public NativeArray<double3> Values;
        public NativeArray<uint3> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint3) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double4_int4 : IJobParallelFor
    {
        [ReadOnly] public double4 Divisor;
        [ReadOnly] public NativeArray<double4> Values;
        public NativeArray<int4> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int4) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_double4_uint4 : IJobParallelFor
    {
        [ReadOnly] public double4 Divisor;
        [ReadOnly] public NativeArray<double4> Values;
        public NativeArray<uint4> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint4) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float_int : IJobParallelFor
    {
        [ReadOnly] public float Divisor;
        [ReadOnly] public NativeArray<float> Values;
        public NativeArray<int> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float_uint : IJobParallelFor
    {
        [ReadOnly] public float Divisor;
        [ReadOnly] public NativeArray<float> Values;
        public NativeArray<uint> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float2_int2 : IJobParallelFor
    {
        [ReadOnly] public float2 Divisor;
        [ReadOnly] public NativeArray<float2> Values;
        public NativeArray<int2> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int2) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float2_uint2 : IJobParallelFor
    {
        [ReadOnly] public float2 Divisor;
        [ReadOnly] public NativeArray<float2> Values;
        public NativeArray<uint2> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint2) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float3_int3 : IJobParallelFor
    {
        [ReadOnly] public float3 Divisor;
        [ReadOnly] public NativeArray<float3> Values;
        public NativeArray<int3> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int3) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float3_uint3 : IJobParallelFor
    {
        [ReadOnly] public float3 Divisor;
        [ReadOnly] public NativeArray<float3> Values;
        public NativeArray<uint3> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint3) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float4_int4 : IJobParallelFor
    {
        [ReadOnly] public float4 Divisor;
        [ReadOnly] public NativeArray<float4> Values;
        public NativeArray<int4> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (int4) math.floor(Values[index] / Divisor);
        }
    }

    /// Divide all the values in the array by a divisor, storing the quotient.
    /// Jobs have a name format of `IntegerDivision_<ValuesType>_<QuotientType>`.
    /// For example `IntegerDivision_float3_uint3`.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct IntegerDivisionJob_float4_uint4 : IJobParallelFor
    {
        [ReadOnly] public float4 Divisor;
        [ReadOnly] public NativeArray<float4> Values;
        public NativeArray<uint4> Quotients;

        public void Execute(int index)
        {
            Quotients[index] = (uint4) math.floor(Values[index] / Divisor);
        }
    }

}