using Unity.Mathematics;

namespace InfPoints
{
    public static class SparseOctreeUtils
    {
        public static int GetNodeCount(int levelIndex)
        {
            return (int) math.pow(2, levelIndex);
        }

    }
}