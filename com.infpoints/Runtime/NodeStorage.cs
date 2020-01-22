using Unity.Collections;

namespace InfPoints
{
    public struct NodeStorage
    {
        Float3SoA<float> m_Float3;
        public bool IsFull => Length == m_Float3.Length;
        public int Length { get; private set; }

        public NodeStorage(int capacity, Allocator allocator, NativeArrayOptions options=NativeArrayOptions.ClearMemory)
        {
            m_Float3 = new Float3SoA<float>(capacity, allocator, options);
            Length = 0;
        }
        
        public void AddPoints(Float3SoA<float> float3)
        {
            m_Float3.CopyFrom(float3);
            Length = float3.Length;
        }
    }
}