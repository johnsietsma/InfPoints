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
    }
}