using System;
using System.Diagnostics;

namespace InfPoints.Octree.Morton
{
    using System.Runtime.CompilerServices;
    using Unity.Mathematics;


    /// <summary>
    /// Morton order.
    /// See http://asgerhoedt.dk/?p=276 for an overview.
    /// Encoding and decoding functions from https://fgiesen.wordpress.com/2009/12/13/decoding-morton-codes/.
    /// </summary>
    public static class Morton
    {
        public static readonly uint MaxCoordinateValue = 0b0011_1111_1111;
        
        public static uint EncodeMorton3(uint3 coordinate)
        {
            CheckLimits(coordinate);
            return (Part1By2(coordinate.z) << 2) + (Part1By2(coordinate.y) << 1) + Part1By2(coordinate.x);
        }
        
        /// <summary>
        /// SIMD verison. This will take the "packed" coordinates and Burst will auto-vectorise so four encodings
        /// happen for the price of one. 
        /// </summary>
        /// <param name="coordinates">(xxxx),(yyyy),(zzzz)</param>
        /// <returns></returns>
        public static uint4 EncodeMorton3(uint4x3 coordinates)
        {
            CheckLimits(coordinates);
            return EncodeMorton3(coordinates[0], coordinates[1], coordinates[2]);
        }

        /// <summary>
        /// SIMD verison. This will take the "packed" coordinates and Burst will auto-vectorise so four encodings
        /// happen for the price of one. 
        /// </summary>
        /// <param name="coordinateX">(xxxx)</param>
        /// <param name="coordinateY">(yyyy)</param>
        /// <param name="coordinateZ">(zzzz)</param>
        /// <returns></returns>
        public static uint4 EncodeMorton3(uint4 coordinateX, uint4 coordinateY, uint4 coordinateZ)
        {
            return (Part1By2(coordinateX) << 2) + (Part1By2(coordinateY) << 1) + Part1By2(coordinateZ);
        }

        public static uint3 DecodeMorton3(uint code)
        {
            var x = Compact1By2(code);
            var y = Compact1By2(code >> 1);
            var z = Compact1By2(code >> 2);
            return new uint3(x, y, z);
        }
        
        /// <summary>
        /// SIMD version. Pass in four codes as a single unit4, it will be auto-vectorised by Burst.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static uint4x3 DecodeMorton3(uint4 code)
        {
            var z = Compact1By2(code);
            var y = Compact1By2(code >> 1);
            var x = Compact1By2(code >> 2);
            return new uint4x3(x, y, z);
        }

        // "Insert" two 0 bits after each of the 10 low bits of x
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Part1By2(uint x)
        {
            x &= 0x000003ff;                  // x = ---- ---- ---- ---- ---- --98 7654 3210
            x = (x ^ (x << 16)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x << 8)) & 0x0300f00f;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x << 4)) & 0x030c30c3;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x << 2)) & 0x09249249;  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            return x;
        }

        // Inverse of Part1By2 - "delete" all bits not at positions divisible by 3
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Compact1By2(uint x)
        {
            x &= 0x09249249;                  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            x = (x ^ (x >> 2)) & 0x030c30c3;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x >> 4)) & 0x0300f00f;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x >> 8)) & 0xff0000ff;  // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x >> 16)) & 0x000003ff; // x = ---- ---- ---- ---- ---- --98 7654 3210
            return x;
        }

        // SIMD friendly version
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint4 Part1By2(uint4 x)
        {
            x &= 0x000003ff;                  // x = ---- ---- ---- ---- ---- --98 7654 3210
            x = (x ^ (x << 16)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x << 8)) & 0x0300f00f;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x << 4)) & 0x030c30c3;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x << 2)) & 0x09249249;  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            return x;
        }

        // SIMD friendly version
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint4 Compact1By2(uint4 x)
        {
            x &= 0x09249249;                  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            x = (x ^ (x >> 2)) & 0x030c30c3;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x >> 4)) & 0x0300f00f;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x >> 8)) & 0xff0000ff;  // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x >> 16)) & 0x000003ff; // x = ---- ---- ---- ---- ---- --98 7654 3210
            return x;
        }

        [Conditional("DEBUG")]
        static void CheckLimits(uint4x3 coordinates)
        {
            var transposedCoordinates = math.transpose(coordinates);
            CheckLimits(transposedCoordinates[0]);
            CheckLimits(transposedCoordinates[1]);
            CheckLimits(transposedCoordinates[2]);
            CheckLimits(transposedCoordinates[3]);
        }
        
        static void CheckLimits(uint3 coordinates)
        {
            if (math.cmax(coordinates) > MaxCoordinateValue)
            {
                throw new OverflowException(
                    $"An element of coordinates {coordinates} is larger then the maximum {MaxCoordinateValue}");
            }
        }
        
    }
}