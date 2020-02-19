﻿using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct FilterFullNodesJob<T> : IJobParallelForFilter where T :unmanaged
    {
        [ReadOnly] public NativeSparsePagedArrayXYZ SparsePagedArray;
        [ReadOnly] public NativeArray<ulong> MortonCodes;

        public FilterFullNodesJob(NativeSparsePagedArrayXYZ sparsePagedArray, NativeArray<ulong> mortonCodes)
        {
            SparsePagedArray = sparsePagedArray;
            MortonCodes = mortonCodes;
        }
        
        
        public bool Execute(int index)
        {
            ulong code = MortonCodes[index];
            return !SparsePagedArray.ContainsNode(code) || !SparsePagedArray.IsFull(code);
        }
    }
}