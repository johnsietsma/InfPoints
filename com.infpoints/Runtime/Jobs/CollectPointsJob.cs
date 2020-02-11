using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    public struct CollectPointsJob : IJobParallelForFilter
    {
        [ReadOnly] public ulong CodeKey;
        [ReadOnly] public NativeList<ulong> Codes;
        [ReadOnly] public NativeArray<float> PointsX;
        [ReadOnly] public NativeArray<float> PointsY;
        [ReadOnly] public NativeArray<float> PointsZ;
        public NativeArray<float> CollectedPointsX;
        public NativeArray<float> CollectedPointsY;
        public NativeArray<float> CollectedPointsZ;
        public NativeArray<int> CollectedPointsCount; // TODO Make NativeValue
        
        public bool Execute(int index)
        {
            if (Codes[index].Equals(CodeKey))
            {
                int collectedPointIndex = CollectedPointsCount[0];
                CollectedPointsX[collectedPointIndex] = PointsX[index];
                CollectedPointsY[collectedPointIndex] = PointsY[index];
                CollectedPointsZ[collectedPointIndex] = PointsZ[index];
                CollectedPointsCount[0]++;
            }

            return true;
        }
    }
}