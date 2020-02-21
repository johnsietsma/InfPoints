using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Copy(collect) all the points matching the same code. 
    /// </summary>
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
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

        public CollectPointsJob(ulong codeKey, NativeArray<ulong> codes, NativeArrayXYZ<float> points, NativeArrayXYZ<float> collectedPoints)
        {
            CodeKey = codeKey;
            Codes = codes;
            PointsX = points.X;
            PointsY = points.Y;
            PointsZ = points.Z;
            CollectedPointsX = collectedPoints.X;
            CollectedPointsY = collectedPoints.Y;
            CollectedPointsZ = collectedPoints.Z;
        }
        
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

            //Logger.Log($"[CollectPointsJob] Collected {count} points");
        }
    }
}