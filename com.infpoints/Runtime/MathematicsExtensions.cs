using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace InfPoints
{
    public static class MathematicsExtensionMethods
    {
        public static bool ApproximatelyEquals(this float f1, float f2, float epsilon = float.Epsilon)
        {
            return math.abs(f2 - f1) < epsilon;
        }

        public static bool ApproximatelyEquals(this float3 f1, float3 f2, float epsilon = float.Epsilon)
        {
            return
                ApproximatelyEquals(f1.x, f2.x, epsilon) &&
                ApproximatelyEquals(f1.y, f2.y, epsilon) &&
                ApproximatelyEquals(f1.z, f2.z, epsilon);
        }

        /// <summary>
        /// Componentwise division of a by b, returning the whole number quotient.
        /// </summary>
        /// <param name="a">numerator</param>
        /// <param name="b">denominator</param>
        /// <returns>The quotient of the division</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint3 QuotientDivide(this float3 a, float b)
        {
            return (uint3) math.floor(a / b);
        }

        /// <summary>
        /// Componentwise division of a by b, returning the whole number quotient.
        /// </summary>
        /// <param name="a">numerator</param>
        /// <param name="b">denominator</param>
        /// <returns>The quotient of the division</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4x3 QuotientDivide(this float4x3 a, float b)
        {
            return new uint4x3(
                (uint4) math.floor(a[0] / b),
                (uint4) math.floor(a[1] / b),
                (uint4) math.floor(a[2] / b));
        }
    }
}