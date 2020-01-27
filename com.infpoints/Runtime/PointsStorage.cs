using System;
using Unity.Collections;

namespace InfPoints
{
    /// <summary>
    /// Simple storage for points.
    /// </summary>
    public struct PointsStorage : IDisposable
    {
        XYZSoA<float> m_Points;
        public bool IsFull => Length == m_Points.Length;
        public int Capacity => m_Points.Length;
        public int Length { get; private set; }

        public PointsStorage(int capacity, Allocator allocator)
        {
            m_Points = new XYZSoA<float>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
            Length = 0;
        }
        
        public void AddPoints(XYZSoA<float> xyz)
        {
            XYZSoA<float>.Copy(xyz, 0, m_Points, Length, xyz.Length);
            Length += xyz.Length;
        }

        public void Dispose()
        {
            m_Points.Dispose();
        }
    }
}