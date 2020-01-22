using Unity.Collections;

namespace InfPoints
{
    public struct NodeStorage
    {
        XYZSoA<float> m_Points;

        public NodeStorage(int length, Allocator allocator, NativeArrayOptions options=NativeArrayOptions.ClearMemory)
        {
            m_Points = new XYZSoA<float>(length, allocator, options);
        }
        
        public void AddPoints(XYZSoA<float> points)
        {
            m_Points.CopyFrom(points);
        }
    }
}