using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct CollectPointsJob : IJob
    {
        [ReadOnly] public ulong CodeKey;
        [ReadOnly] public NativeArray<ulong> Codes;
        [ReadOnly] public NativeArray<float> PointsX;
        [ReadOnly] public NativeArray<float> PointsY;
        [ReadOnly] public NativeArray<float> PointsZ;
        public NativeArray<float> CollectedPointsX;
        public NativeArray<float> CollectedPointsY;
        public NativeArray<float> CollectedPointsZ;
        public NativeInt CollectedPointsCount;
        
        public void Execute()
        {
            int count = 0;
            for (int index = 0; index < Codes.Length; index++)
            {
                if (Codes[index].Equals(CodeKey))
                {
                    CollectedPointsX[count] = PointsX[index];
                    CollectedPointsY[count] = PointsY[index];
                    CollectedPointsZ[count] = PointsZ[index];
                    count++;
                }
            }

            Logger.Log($"Collected {count} points");
            CollectedPointsCount.Value = count;
        }
    }
}