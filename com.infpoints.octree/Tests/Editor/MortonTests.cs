namespace Infpoints.Octree.Tests.Editor
{
    using NUnit.Framework;
    using Unity.Mathematics;
    using UnityEngine;

    public class MortonTests
    {
        [Test]
        public void ConvertCoordinateToIndex()
        {
            TestEncoding(new uint3(0), 0);
            TestEncoding(new uint3(1,0,1), 5);
        }

        private void TestEncoding( uint3 coordinate, uint expectedCode )
        {
            var code = Morton.EncodeMorton3( coordinate );
            var decodedCoorindate = Morton.DecodeMorton3( code );

            Assert.AreEqual(code, expectedCode);
            Assert.AreEqual(coordinate, decodedCoorindate);
        }
    }

}