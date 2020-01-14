using NUnit.Framework;
using Unity.Mathematics;

namespace InfPoints.Tests.Editor
{
    public class AABBTests
    {
        [Test]
        public void Test_Contains()
        {
            var aabb = new AABB(new float3(25f, 25f, 25f), 50 );

            var insidePoint = new float3(49f, 1f, 3f);
            Assert.That(aabb.Contains(insidePoint), Is.True);
            
            var outsidePoint = new float3(-1f, 1f, 3f);
            Assert.That(aabb.Contains(outsidePoint), Is.False);
        }
    }
}