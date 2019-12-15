using System;
using System.Diagnostics;

namespace InfPoints.Octree.Morton
{
    using System.Runtime.CompilerServices;
    using Unity.Mathematics;


    /// <summary>
    /// Morton order.
    /// See http://asgerhoedt.dk/?p=276 for an overview.
    /// Encoding and decoding functions adapted from https://fgiesen.wordpress.com/2009/12/13/decoding-morton-codes/.
    /// </summary>
    public static class Morton
    {
        public static readonly uint MaxCoordinateValue = 0b0011_1111_1111;  // 1023
        public static readonly uint MaxCoordinateValue64 = 0b0011_1111_1111_1111_1111_1111;  // 4194303
        
        public static uint EncodeMorton3(uint3 coordinate)
        {
            CheckLimits(coordinate);
            return (Part1By2(coordinate.z) << 2) + (Part1By2(coordinate.y) << 1) + Part1By2(coordinate.x);
        }

        public static ulong EncodeMorton3_64(uint3 coordinate)
        {
            CheckLimits64(coordinate);
            return (Part1By2_64(coordinate.z) << 2) + (Part1By2_64(coordinate.y) << 1) + Part1By2_64(coordinate.x);
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
            //CheckLimits(coordinates);
            return (Part1By2(coordinateX) << 2) + (Part1By2(coordinateY) << 1) + Part1By2(coordinateZ);
        }

        public static uint3 DecodeMorton3(uint code)
        {
            var x = Compact1By2(code);
            var y = Compact1By2(code >> 1);
            var z = Compact1By2(code >> 2);
            return new uint3(x, y, z);
        }
        
        public static uint3 DecodeMorton3_64(ulong code)
        {
            var x = Compact1By2_64(code);
            var y = Compact1By2_64(code >> 1);
            var z = Compact1By2_64(code >> 2);
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
            x &=                                           0b0011_1111_1111;  // x = ---- ---- ---- ---- ---- --98 7654 3210
            x = (x ^ (x << 16)) & 0b1111_1111_0000_0000_0000_0000_1111_1111;  // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x << 8)) &  0b0000_0011_0000_0000_1111_0000_0000_1111;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x << 4)) &  0b0000_0011_0000_1100_0011_0000_1100_0011;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x << 2)) &  0b0000_1001_0010_0100_1001_0010_0100_1001;  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            return x;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong Part1By2_64(uint x)
        {
            ulong x64 = x;
            //                                                                          x = --10 9876 5432 1098 7654 3210
            x64 &=                                                                        0b0011_1111_1111_1111_1111_1111;

            //                             x = ---- ---0 9876 ---- ---- ---- ---- ---- ---- ---- ---- 5432 1098 7654 3210
            x64 = (x64 ^ (x64 << 32)) &      0b0000_0001_1111_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111;
            
            //                             x = ---- ---0 9876 ---- ---- ---- ---- 5432 1098 ---- ---- ---- ---- 7654 3210
            x64 = (x64 ^ (x64 << 16)) &      0b0000_0001_1111_0000_0000_0000_0000_1111_1111_0000_0000_0000_0000_1111_1111;  
            
            //                        x = ---0 ---- ---- 9876 ---- ---- 5432 ---- ---- 1098 ---- ---- 7654 ---- ---- 3210
            x64 = (x64 ^ (x64 << 8)) &  0b0001_0000_0000_1111_0000_0000_1111_0000_0000_1111_0000_0000_1111_0000_0000_1111;  
            
            //                        x = ---0 ---- 98-- --76 ---- 54-- --32 ---- 10-- --98 ---- 76-- --54 ---- 32-- --10
            x64 = (x64 ^ (x64 << 4)) &  0b0001_0000_0000_0000_0000_0000_0000_0000_0000_0011_0000_1100_0011_0000_1100_0011;  
            
            //                        x = ---1 --0- -9-- 8--7 --6- -5-- 4--3 --1- -0-- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            x64 = (x64 ^ (x64 << 2)) &  0b0001_0010_0100_1001_0010_0100_1001_0010_0100_1001_0010_0100_1001_0010_0100_1001;  
            return x64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Compact1By2(uint x)
        {
            x &=                  0b0000_1001_0010_0100_1001_0010_0100_1001;  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            x = (x ^ (x >> 2)) &  0b0000_0011_0000_1100_0011_0000_1100_0011;  // x = ---- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x >> 4)) &  0b0000_0011_0000_0000_1111_0000_0000_1111;  // x = ---- --98 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x >> 8)) &  0b1111_1111_0000_0000_0000_0000_1111_1111;  // x = ---- --98 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x >> 16)) & 0b0000_0000_0000_0000_0000_0011_1111_1111;  // x = ---- ---- ---- ---- ---- --98 7654 3210
            return x;
        }


        // Inverse of Part1By2 - "delete" all bits not at positions divisible by 3
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Compact1By2_64(ulong x)
        {
            //                  x = ---1 --0- -9-- 8--7 --6- -5-- 4--3 --1- -0-- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
            x &=                  0b0001_0010_0100_1001_0010_0100_1001_0010_0100_1001_0010_0100_1001_0010_0100_1001;  
            
            //                  x = ---0 ---- 98-- --76 ---- 54-- --32 ---- 10-- --98 ---- 76-- --54 ---- 32-- --10
            x = (x ^ (x >> 2)) &  0b0001_0000_0000_0000_0000_0000_0000_0000_0000_0011_0000_1100_0011_0000_1100_0011;

            //                  x = ---0 ---- ---- 9876 ---- ---- 5432 ---- ---- 1098 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x >> 4)) &  0b0001_0000_0000_1111_0000_0000_1111_0000_0000_1111_0000_0000_1111_0000_0000_1111;
            
            //                  x = ---0 ---- ---- 9876 ---- ---- 5432 ---- ---- 1098 ---- ---- 7654 ---- ---- 3210
            x = (x ^ (x >> 8)) &  0b0001_0000_0000_1111_0000_0000_1111_0000_0000_1111_0000_0000_1111_0000_0000_1111;
            
            //                  x = ---- ---0 9876 ---- ---- ---- ---- 5432 1098 ---- ---- ---- ---- 7654 3210
            x = (x ^ (x >> 16)) & 0b0000_0001_1111_0000_0000_0000_0000_1111_1111_0000_0000_0000_0000_1111_1111;
            
            //                  x = ---- ---0 9876 ---- ---- ---- ---- ---- ---- ---- ---- 5432 1098 7654 3210
            x = (x ^ (x >> 32)) & 0b0000_0001_1111_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111;

            return (uint)x;
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
        
        static void CheckLimits64(uint3 coordinates)
        {
            if (math.cmax(coordinates) > MaxCoordinateValue64)
            {
                throw new OverflowException(
                    $"An element of coordinates {coordinates} is larger then the maximum {MaxCoordinateValue}");
            }
        }
    }
}