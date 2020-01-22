using Unity.Collections;

namespace InfPoints
{
    public struct NodeStorage
    {
        XYZSoA<float> m_Points;
        public bool IsFull => Length == m_Points.Length;
        public int Length { get; private set; }

        public NodeStorage(int capacity, Allocator allocator, NativeArrayOptions options=NativeArrayOptions.ClearMemory)
        {
            m_Points = new XYZSoA<float>(capacity, allocator, options);
            Length = 0;
        }
        
        public void AddPoints(XYZSoA<float> points)
        {
            m_Points.CopyFrom(points);
            Length = points.Length;
        }
    }
}