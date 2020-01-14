using Unity.Mathematics;

namespace InfPoints
{
    /// <summary>
    /// A square AABB. 
    /// </summary>
    // Has to be square
    public struct AABB
    {
        public static readonly AABB zero = default;

        public readonly float3 Center;
        public readonly float Size;
        public float Extents => Size / 2;
        public float3 Minimum => Center - Size / 2;
        public float3 Maximum => Center - Size / 2;

        public AABB(float3 center, float size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(float3 position)
        {
            return
                position.x >= Center.x - Extents && position.x <= Center.x + Extents &&
                position.y >= Center.y - Extents && position.y <= Center.y + Extents &&
                position.z >= Center.z - Extents && position.z <= Center.z + Extents;
        }

        public float3 TransformPoint(float3 point)
        {
            return point - Minimum;
        }

        public override string ToString()
        {
            return $"Center:{Center} Size:{Size}";
        }
    }
}