using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Octree.Morton
{
    [BurstCompile]
    public struct MortonEncodeJob : IJob
    {
        [ReadOnly]
        public NativeArray<uint3> m_Coordinates;
        public NativeArray<uint> m_Codes;

        public void Execute()
        {
            for (int i = 0; i < m_Coordinates.Length; i++)
            {
                m_Codes[i] = Morton.EncodeMorton3(m_Coordinates[i]);
            }
        }
    }
}