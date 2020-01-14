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
            return Contains(position.x, position.y, position.z);
        }
        
        public bool Contains(float x, float y, float z)
        {
            var min = Minimum;
            var max = Maximum;
            return
                x >= min.x && x <= max.x &&
                y >= min.y && y <= max.y &&
                z >= min.z && z <= max.z;
        }
        
        public bool Contains(float4 x, float4 y, float4 z)
        {
            var min = Minimum;
            var max = Maximum;
            return
                math.cmin(x) >= min.x && math.cmax(x) <= max.x &&
                math.cmin(y) >= min.y && math.cmax(y) <= max.y &&
                math.cmin(z) >= min.z && math.cmax(z) <= max.z;
        }

        public override string ToString()
        {
            return $"Center:{Center} Size:{Size}";
        }
    }
}