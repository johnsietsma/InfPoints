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

/// ParallelFor Jobs for simple addition of numbers to elements of NativeArrays.
/// Each numeric type of Unity.Mathematics is supported; double, float, int and uint.
/// Each dimension is supported 2,3,4,2x2,2x3,2x4, etc.
namespace InfPoints 
{
    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double : IJobParallelFor
    {
        [ReadOnly] public double NumberToAdd;
        public NativeArray<double> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double2 : IJobParallelFor
    {
        [ReadOnly] public double2 NumberToAdd;
        public NativeArray<double2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double2x2 : IJobParallelFor
    {
        [ReadOnly] public double2x2 NumberToAdd;
        public NativeArray<double2x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double2x3 : IJobParallelFor
    {
        [ReadOnly] public double2x3 NumberToAdd;
        public NativeArray<double2x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double2x4 : IJobParallelFor
    {
        [ReadOnly] public double2x4 NumberToAdd;
        public NativeArray<double2x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double3 : IJobParallelFor
    {
        [ReadOnly] public double3 NumberToAdd;
        public NativeArray<double3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double3x2 : IJobParallelFor
    {
        [ReadOnly] public double3x2 NumberToAdd;
        public NativeArray<double3x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double3x3 : IJobParallelFor
    {
        [ReadOnly] public double3x3 NumberToAdd;
        public NativeArray<double3x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double3x4 : IJobParallelFor
    {
        [ReadOnly] public double3x4 NumberToAdd;
        public NativeArray<double3x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double4 : IJobParallelFor
    {
        [ReadOnly] public double4 NumberToAdd;
        public NativeArray<double4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double4x2 : IJobParallelFor
    {
        [ReadOnly] public double4x2 NumberToAdd;
        public NativeArray<double4x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double4x3 : IJobParallelFor
    {
        [ReadOnly] public double4x3 NumberToAdd;
        public NativeArray<double4x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_double4x4 : IJobParallelFor
    {
        [ReadOnly] public double4x4 NumberToAdd;
        public NativeArray<double4x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float : IJobParallelFor
    {
        [ReadOnly] public float NumberToAdd;
        public NativeArray<float> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float2 : IJobParallelFor
    {
        [ReadOnly] public float2 NumberToAdd;
        public NativeArray<float2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float2x2 : IJobParallelFor
    {
        [ReadOnly] public float2x2 NumberToAdd;
        public NativeArray<float2x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float2x3 : IJobParallelFor
    {
        [ReadOnly] public float2x3 NumberToAdd;
        public NativeArray<float2x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float2x4 : IJobParallelFor
    {
        [ReadOnly] public float2x4 NumberToAdd;
        public NativeArray<float2x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float3 : IJobParallelFor
    {
        [ReadOnly] public float3 NumberToAdd;
        public NativeArray<float3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float3x2 : IJobParallelFor
    {
        [ReadOnly] public float3x2 NumberToAdd;
        public NativeArray<float3x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float3x3 : IJobParallelFor
    {
        [ReadOnly] public float3x3 NumberToAdd;
        public NativeArray<float3x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float3x4 : IJobParallelFor
    {
        [ReadOnly] public float3x4 NumberToAdd;
        public NativeArray<float3x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float4 : IJobParallelFor
    {
        [ReadOnly] public float4 NumberToAdd;
        public NativeArray<float4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float4x2 : IJobParallelFor
    {
        [ReadOnly] public float4x2 NumberToAdd;
        public NativeArray<float4x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float4x3 : IJobParallelFor
    {
        [ReadOnly] public float4x3 NumberToAdd;
        public NativeArray<float4x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_float4x4 : IJobParallelFor
    {
        [ReadOnly] public float4x4 NumberToAdd;
        public NativeArray<float4x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int : IJobParallelFor
    {
        [ReadOnly] public int NumberToAdd;
        public NativeArray<int> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int2 : IJobParallelFor
    {
        [ReadOnly] public int2 NumberToAdd;
        public NativeArray<int2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int2x2 : IJobParallelFor
    {
        [ReadOnly] public int2x2 NumberToAdd;
        public NativeArray<int2x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int2x3 : IJobParallelFor
    {
        [ReadOnly] public int2x3 NumberToAdd;
        public NativeArray<int2x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int2x4 : IJobParallelFor
    {
        [ReadOnly] public int2x4 NumberToAdd;
        public NativeArray<int2x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int3 : IJobParallelFor
    {
        [ReadOnly] public int3 NumberToAdd;
        public NativeArray<int3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int3x2 : IJobParallelFor
    {
        [ReadOnly] public int3x2 NumberToAdd;
        public NativeArray<int3x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int3x3 : IJobParallelFor
    {
        [ReadOnly] public int3x3 NumberToAdd;
        public NativeArray<int3x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int3x4 : IJobParallelFor
    {
        [ReadOnly] public int3x4 NumberToAdd;
        public NativeArray<int3x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int4 : IJobParallelFor
    {
        [ReadOnly] public int4 NumberToAdd;
        public NativeArray<int4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int4x2 : IJobParallelFor
    {
        [ReadOnly] public int4x2 NumberToAdd;
        public NativeArray<int4x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int4x3 : IJobParallelFor
    {
        [ReadOnly] public int4x3 NumberToAdd;
        public NativeArray<int4x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_int4x4 : IJobParallelFor
    {
        [ReadOnly] public int4x4 NumberToAdd;
        public NativeArray<int4x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint : IJobParallelFor
    {
        [ReadOnly] public uint NumberToAdd;
        public NativeArray<uint> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint2 : IJobParallelFor
    {
        [ReadOnly] public uint2 NumberToAdd;
        public NativeArray<uint2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint2x2 : IJobParallelFor
    {
        [ReadOnly] public uint2x2 NumberToAdd;
        public NativeArray<uint2x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint2x3 : IJobParallelFor
    {
        [ReadOnly] public uint2x3 NumberToAdd;
        public NativeArray<uint2x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint2x4 : IJobParallelFor
    {
        [ReadOnly] public uint2x4 NumberToAdd;
        public NativeArray<uint2x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint3 : IJobParallelFor
    {
        [ReadOnly] public uint3 NumberToAdd;
        public NativeArray<uint3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint3x2 : IJobParallelFor
    {
        [ReadOnly] public uint3x2 NumberToAdd;
        public NativeArray<uint3x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint3x3 : IJobParallelFor
    {
        [ReadOnly] public uint3x3 NumberToAdd;
        public NativeArray<uint3x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint3x4 : IJobParallelFor
    {
        [ReadOnly] public uint3x4 NumberToAdd;
        public NativeArray<uint3x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint4 : IJobParallelFor
    {
        [ReadOnly] public uint4 NumberToAdd;
        public NativeArray<uint4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint4x2 : IJobParallelFor
    {
        [ReadOnly] public uint4x2 NumberToAdd;
        public NativeArray<uint4x2> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint4x3 : IJobParallelFor
    {
        [ReadOnly] public uint4x3 NumberToAdd;
        public NativeArray<uint4x3> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

    /// Add a number to every element of a NativeArray.
    /// The addition is done in place.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AddJob_uint4x4 : IJobParallelFor
    {
        [ReadOnly] public uint4x4 NumberToAdd;
        public NativeArray<uint4x4> Data;

        public void Execute(int index)
        {
            Data[index] = Data[index] + NumberToAdd;
        }
    }

}