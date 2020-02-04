using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    public struct CollectPointsJob : IJobParallelForFilter
    {
        [ReadOnly] public ulong CodeKey;
        [ReadOnly] public NativeArray<ulong> Codes;
        [ReadOnly] public NativeArray<float> PointsX;
        [ReadOnly] public NativeArray<float> PointsY;
        [ReadOnly] public NativeArray<float> PointsZ;
        public NativeArray<float3> CollectedPoints;
        public NativeArray<int> CollectedPointsCount; // TODO Make NativeValue
        
        public bool Execute(int index)
        {
            if (Codes[index].Equals(CodeKey))
            {
                CollectedPoints[CollectedPointsCount[0]] = new float3(PointsX[index], PointsY[index], PointsZ[index]);
                CollectedPointsCount[0]++;
            }

            return true;
        }
    }
}