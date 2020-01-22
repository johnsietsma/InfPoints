﻿using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

 namespace InfPoints
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct NativeHashMapGetValuesJob<T> : IJob where T : unmanaged, IEquatable<T>
    {
        [ReadOnly] public NativeHashMap<T, int> UniqueHash;
        public NativeList<T> UniqueValues;

        public void Execute()
        {
            var uniqueValuesTemp = UniqueHash.GetKeyArray(Allocator.Temp);
            UniqueValues.AddRange(uniqueValuesTemp);
        }
    }   
}