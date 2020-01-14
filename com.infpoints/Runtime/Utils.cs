using System;
using Unity.Mathematics;

namespace InfPoints
{
    public class Utils
    {
        public static uint3 PointToCoords(float3 point, int cellCount, AABB aabb)
        {
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            if(!aabb.Contains(point)) throw new ArgumentOutOfRangeException(nameof(point));
            #endif
            
            float cellSize = aabb.Size / cellCount;

            uint3 coord = new uint3(
                (uint)math.floor(point.x / cellSize),
                (uint)math.floor(point.y / cellSize),
                (uint)math.floor(point.z / cellSize)
            );
            
            return coord;
        }
        
        public static void PointToCoords(XYZSoA<float> pointsSoA, int cellCount, AABB aabb, XYZSoA<uint> coordsSoA)
        {
            var pointsX = pointsSoA.X.Reinterpret<float4>();
            var pointsY = pointsSoA.X.Reinterpret<float4>();
            var pointsZ = pointsSoA.X.Reinterpret<float4>();
            
            var coordsX = coordsSoA.X.Reinterpret<uint4>();
            var coordsY = coordsSoA.X.Reinterpret<uint4>();
            var coordsZ = coordsSoA.X.Reinterpret<uint4>();
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if(pointsX.Length!=pointsY.Length && pointsY.Length!=pointsZ.Length) throw new ArgumentException("X,Y and Z arrays are not equal length");
#endif
            
            for (int i = 0; i < pointsX.Length; i++)
            {
                float4 x = pointsX[i];
                float4 y = pointsY[i];
                float4 z = pointsZ[i];
                
                
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (!aabb.Contains(x,y,z)) throw new ArgumentOutOfRangeException();
#endif

                float cellSize = aabb.Size / cellCount;

                uint4 coordX = (uint4) math.floor(x / cellSize);
                uint4 coordY = (uint4) math.floor(y / cellSize);
                uint4 coordZ = (uint4) math.floor(z / cellSize);

                coordsX[i] = coordX;
                coordsY[i] = coordY;
                coordsZ[i] = coordZ;
            }
        }
    }
}