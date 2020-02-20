using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

// ReSharper disable HeapView.BoxingAllocation

namespace InfPoints.Tests.Editor
{
    public class SparseOctreeTests
    {
        [Test]
        public void CreateAndDispose()
        {
            var aabb = new AABB(float3.zero, 1);
            var octree = new SparseOctree(aabb, 1, Allocator.Persistent);
            Assert.That(octree.IsCreated, Is.True);
            octree.Dispose();
            Assert.That(octree.IsCreated, Is.False);
            Assert.That(delegate { octree.Dispose(); }, Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void AddValueGivesCorrectValue()
        {
            var aabb = new AABB(float3.zero, 1);
            using (var octree = new SparseOctree(aabb, 1, Allocator.Persistent))
            {
                Assert.That(octree.LevelCount, Is.EqualTo(0));
                octree.AddLevel();
                Assert.That(octree.LevelCount, Is.EqualTo(1));
            }
        }
    }
}