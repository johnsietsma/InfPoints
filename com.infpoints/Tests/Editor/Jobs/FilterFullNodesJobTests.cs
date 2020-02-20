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
            using (var pagedArray = new NativeSparsePagedArrayXYZ(1,1, 2, Allocator.TempJob))
            using( var codes = new NativeArray<ulong>(codesArray, Allocator.TempJob))
            using( var indices = new NativeList<int>(codes.Length, Allocator.TempJob))
            {
                // Fill the fist page
                var index1 = codesArray[0];
                pagedArray.AddNode(index1);
                pagedArray.Add(index1, 1);
                
                // Don't fill the second page
                var index2 = codesArray[1];
                pagedArray.AddNode(index2);
                
                var isFullJob = new FilterFullNodesJob<int>()
                {
                    MortonCodes = codes,
                    SparsePagedArray = pagedArray
                }.ScheduleAppend(indices, codes.Length, 4);
                
                isFullJob.Complete();
                
                Assert.That(indices.Length, Is.EqualTo(1));
            }
        }
    }
}