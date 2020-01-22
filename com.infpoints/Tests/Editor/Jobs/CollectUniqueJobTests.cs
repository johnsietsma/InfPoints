using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class CollectUniqueJobTests
    {
        [Test]
        public void DoesCollectUniqueValues()
        {
            int[] values = {1, 1, 2, 3, 5, 5, 6};
            using (var valuesArray = new NativeArray<int>(values, Allocator.TempJob))
            using (var uniqueMap = new NativeHashMap<int, int>(valuesArray.Length, Allocator.TempJob))
            {
                var uniqueJob = new CollectUniqueJob<int>()
                {
                    Values = valuesArray,
                    UniqueValues = uniqueMap
                };


                var uniqueValues = new NativeList<int>(uniqueMap.Length, Allocator.Persistent);

                var toArrayJob = new NativeHashMapGetValuesJob<int>();
                toArrayJob.UniqueHash = uniqueMap;
                toArrayJob.UniqueValues = uniqueValues;


                var collectUniqueJobHandle = uniqueJob.Schedule();
                collectUniqueJobHandle.Complete();

                // Must complete the collect job before scheduling so we can use the results
                var toArrayJobHandle = toArrayJob.Schedule();
                toArrayJobHandle.Complete();
        
                Assert.That(uniqueMap.Length, Is.EqualTo(5));
                Assert.That(uniqueMap.ContainsKey(5), Is.True);
                Assert.That(uniqueValues.Length, Is.EqualTo(5));
                Assert.That(uniqueValues[3], Is.EqualTo(5));

                uniqueValues.Dispose();
            }
        }
    }
}