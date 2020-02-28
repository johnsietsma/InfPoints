using InfPoints.Jobs;
using InfPoints.NativeCollections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class FilterFullNodesJobTests
    {
        [Test]
        public void DoesFilterFullNodes()
        {
            ulong[] codesArray = {1, 2};
            int[] codesCount = {1, 1}; // Doesn't matter what these values are
            using (var storage = new NativeSparsePagedArrayXYZ(1, 1, 2, Allocator.TempJob))
            using (var codes = new NativeSparseList<ulong, int>(codesArray, codesCount, Allocator.TempJob))
            {
                // Fill the fist page
                var index1 = codesArray[0];
                storage.AddNode(index1);
                storage.Add(index1, 1);

                // Don't fill the second page
                var index2 = codesArray[1];
                storage.AddNode(index2);

                var isFullJob = new FilterFullNodesJob<int>(storage, codes).Schedule();

                isFullJob.Complete();
                Assert.That(codes.Length, Is.EqualTo(1));
            }
        }
    }
}