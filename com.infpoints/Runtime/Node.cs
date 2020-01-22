using System;

namespace InfPoints
{
    public struct Node : IEquatable<Node>
    {
        public int LevelIndex;
        public long MortonCode;

        public bool Equals(Node other)
        {
            return LevelIndex == other.LevelIndex && MortonCode == other.MortonCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Node other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (LevelIndex * 397) ^ MortonCode.GetHashCode();
            }
        }
    }
}