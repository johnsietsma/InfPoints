
namespace Infpoints.Octree
{
    using Infpoints.Common;
    using Unity.Mathematics;

    /// <summary>
    /// Morton order.
    /// See http://asgerhoedt.dk/?p=276 for an overview.
    /// Encoding and decoding functions from https://fgiesen.wordpress.com/2009/12/13/decoding-morton-codes/.
    /// </summary>
    public static class Morton
    {
        public static uint EncodeMorton3(uint3 coordinate)
        {
            return (Part1By2(coordinate.z) << 2) + (Part1By2(coordinate.y) << 1) + Part1By2(coordinate.x);
        }

        public static uint3 DecodeMorton3(uint code)
        {
            var x = Compact1By2(code);
            var y = Compact1By2(code >> 1);
            var z = Compact1By2(code >> 2);
            return new uint3(x, y, z);
        }

        // "Insert" two 0 bits after each of the 10 low bits of x
        static uint Part1By2(uint x)
        {
            x &= 0x000003ff;                  // x = ---- ---- ---- ---- ---- --98 7654 3210
            x = (x ^ (x << 16)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x << 8)) & 0x0300f00f; // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x << 4)) & 0x030c30c3; // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x << 2)) & 0x09249249; // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            return x;
        }

        // Inverse of Part1By2 - "delete" all bits not at positions divisible by 3
        static uint Compact1By2(uint x)
        {
            x &= 0x09249249;                  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            x = (x ^ (x >> 2)) & 0x030c30c3; // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x >> 4)) & 0x0300f00f; // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x >> 8)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x >> 16)) & 0x000003ff; // x = ---- ---- ---- ---- ---- --98 7654 3210
            return x;
        }
    }
}