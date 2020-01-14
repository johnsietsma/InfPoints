using System;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable HeapView.BoxingAllocation

namespace InfPoints.Tests.Editor
{
    public class SparseOctreeTests
    {
        [Test]
        public void CreateAndDispose()
        {
            var octree = new SparseOctree<int>(AABB.zero, 1, Allocator.Persistent);
            Assert.That(octree.IsCreated, Is.True);
            octree.Dispose();
            Assert.That(octree.IsCreated, Is.False);
            Assert.That(delegate { octree.Dispose(); }, Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void AddValueGivesCorrectValue()
        {
            using (var octree = new SparseOctree<int>(AABB.zero, 1,Allocator.Persistent))
            {
                Assert.That(octree.LevelCount, Is.EqualTo(0));
                octree.AddLevel(1);
                Assert.That(octree.LevelCount, Is.EqualTo(1));
            }
        }
    }
}