using System;
using Unity.Collections;

namespace InfPoints
{
    /// <summary>
    /// </summary>
    public struct NodePointsMap : IDisposable
    {
        public NativeHashMap<Node, PointsStorage> NodePoints => m_NodePointsMap;
        NativeHashMap<Node, PointsStorage> m_NodePointsMap;

        public NodePointsMap(int capacity, Allocator allocator)
        {
            m_NodePointsMap = new NativeHashMap<Node, PointsStorage>(capacity, allocator);
        }

        public bool IsFull(Node node)
        {
            return m_NodePointsMap[node].IsFull;
        }
        
        
        public void AddPoints(Node node, XYZSoA<float> points)
        {
            m_NodePointsMap[node].AddPoints(points);
        }

        public void Dispose()
        {
            m_NodePointsMap.Dispose();
        }
    }
}