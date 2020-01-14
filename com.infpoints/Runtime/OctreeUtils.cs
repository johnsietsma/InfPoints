using Unity.Collections;
using Unity.Mathematics;

namespace InfPoints
{
    public static class OctreeUtils
    {
        public static uint3 ConvertPointToCoord(AABB aabb, int cellCount, float3 point)
        {
            point -= aabb.Minimum; // Convert to AABB space
            var cellSize = aabb.Size / cellCount;
            return point.QuotientDivide(cellSize);
        }

        public static uint4 ConvertPointToCoord(AABB aabb, int cellCount, float4 point)
        {
            point -= aabb.Size / 2; // Convert to AABB space
            var cellSize = aabb.Size / cellCount;
            return point.QuotientDivide(cellSize);
        }
    }
}