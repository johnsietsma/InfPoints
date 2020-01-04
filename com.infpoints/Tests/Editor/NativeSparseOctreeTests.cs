using System;
using System.Collections;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable HeapView.BoxingAllocation

namespace InfPoints.Tests.Editor
{
    public class NativeSparseOctreeTests
    {
        [Test]
        public void Create()
        {
            var octree = default(NativeSparseOctree<int>);
            Assert.That(octree.IsCreated, Is.False);
            octree = new NativeSparseOctree<int>(Allocator.Persistent);
            Assert.That(octree.IsCreated, Is.True);
            octree.Dispose();
        }

        [Test]
        public void Dispose()
        {
            var octree = new NativeSparseArray<int>(1, Allocator.Persistent);
            octree.Dispose();
            Assert.That(octree.IsCreated, Is.False);
            Assert.That(delegate { octree.Dispose(); }, Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.That(() => octree.AddValue(1, 2), Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void AddValueGivesCorrectValue()
        {
            using (var octree = new NativeSparseOctree<int>(Allocator.Persistent))
            {
                Assert.That(octree.LevelCount, Is.EqualTo(0));
                octree.AddLevel();
                Assert.That(octree.LevelCount, Is.EqualTo(1));
            }
            
        }
    }
}