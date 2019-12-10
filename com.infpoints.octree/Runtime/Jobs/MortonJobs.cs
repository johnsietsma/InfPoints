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
    
    [BurstCompile]
    public struct MortonEncodeJob_Packed : IJob
    {
        [ReadOnly]
        public NativeArray<uint3x4> m_Coordinates;
        public NativeArray<uint4> m_Codes;

        public void Execute()
        {
            for (int i = 0; i < m_Coordinates.Length; i++)
            {
                m_Codes[i] = Morton.EncodeMorton3( math.transpose(m_Coordinates[i]) );
            }
        }
    }
}