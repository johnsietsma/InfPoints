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
            const int length = 2;
            ulong[] codesArray = {1, 2};
            using (var pageAllocations = new NativeSparseArray<PageAllocation>(length, Allocator.TempJob))
            using( var codes = new NativeArray<ulong>(codesArray, Allocator.TempJob))
            using( var indices = new NativeList<int>(length, Allocator.TempJob))
            {
                const int pageIndex = 0;
                const int startIndex = 0;

                pageAllocations.AddValue( new PageAllocation()
                {
                    PageIndex = pageIndex,
                    Capacity = 1,
                    Length = 1,
                    StartIndex = startIndex
                } , codesArray[0] );
                
                pageAllocations.AddValue( new PageAllocation()
                {
                    PageIndex = pageIndex,
                    Capacity = 2,
                    Length = 1,
                    StartIndex = startIndex
                } , codesArray[1] );
                
                var isFullJob = new FilterFullNodesJob()
                {
                    MortonCodes = codes,
                    PageAllocations = pageAllocations
                }.ScheduleAppend(indices, length, 4);
                
                isFullJob.Complete();
                
                Assert.That(indices.Length, Is.EqualTo(1));
            }
            
        }
    }
}