using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class GetUniqueValuesJobTests
    {
        [Test]
        public void DoesFilterUniqueValues()
        {
            int[] values = {1, 1, 2, 3, 5, 5, 6, 6};
            using (var valuesArray = new NativeArray<int>(values, Allocator.TempJob))
            using (var uniqueMap = new NativeHashMap<int,uint>(valuesArray.Length, Allocator.TempJob))
            using(var indices = new NativeList<int>(valuesArray.Length, Allocator.TempJob) )
            {
                var uniqueJob = new GetUniqueValuesJob<int>()
                {
                    Values = valuesArray,
                    UniqueValues = uniqueMap
                };


                var collectUniqueJobHandle = uniqueJob.Schedule(valuesArray.Length, 128);
                collectUniqueJobHandle.Complete();

                Assert.That(uniqueMap.Length, Is.EqualTo(5));
                Assert.That(uniqueMap.ContainsKey(5), Is.True);
                Assert.That(indices.Length, Is.EqualTo(5));
                Assert.That(indices[0], Is.EqualTo(0));
                Assert.That(indices[1], Is.EqualTo(2));
            }
        }
    }
}