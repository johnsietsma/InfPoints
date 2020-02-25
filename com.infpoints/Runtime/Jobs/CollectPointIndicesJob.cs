using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
   
    /// <summary>
    /// Copy(collect) all the indices of points matching the code. 
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CollectPointIndicesJob : IJob
    {
        [ReadOnly] public ulong CodeKey;
        [ReadOnly] public NativeArray<ulong> PointCodes;
        public NativeArray<int> CollectedPointsIndices;

        public CollectPointIndicesJob(ulong codeKey, NativeArray<ulong> pointCodes, NativeArray<int> collectedPointIndices)
        {
            CodeKey = codeKey;
            PointCodes = pointCodes;
            CollectedPointsIndices = collectedPointIndices;
        }
        
        public void Execute()
        {
            int count = 0;
            for (int index = 0; index < PointCodes.Length; index++)
            {
                if (PointCodes[index].Equals(CodeKey))
                {
                    CollectedPointsIndices[count] = index;
                    count++;
                }
            }

            Logger.LogFormat(LogString.PointsCollected, count);
        }
    }
}