using Unity.Mathematics;

namespace InfPoints
{
    /// <summary>
    /// A square AABB. 
    /// </summary>
    public struct AABB
    {
        public static readonly AABB zero = default;

        public readonly float3 Center;
        public readonly float Size;
        public float Extents => Size / 2;
        public float3 Minimum => Center - Extents;
        public float3 Maximum => Center + Extents;

        public AABB(float3 center, float size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(float3 position)
        {
            var min = Minimum;
            var max = Maximum;
            return
                position.x >= min.x && position.x <= max.x &&
                position.y >= min.y && position.y <= max.y &&
                position.z >= min.z && position.z <= max.z;
        }

        public override string ToString()
        {
            return $"Center:{Center} Size:{Size}";
        }
    }
}