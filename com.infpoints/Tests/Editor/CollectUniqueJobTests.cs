using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor
{
    public class CollectUniqueJobTests
    {
        [Test]
        public void DoesCollectUniqueValues()
        {
            int[] values = {1, 1, 2, 3, 5, 5, 6};
            using (var valuesArray = new NativeArray<int>(values, Allocator.TempJob))
                using( var uniqueMap = new NativeHashMap<int,int>(valuesArray.Length, Allocator.TempJob))
                {
                    var uniqueJob = new CollectUniqueJob<int>()
                    {
                        Values = valuesArray,
                        UniqueValues = uniqueMap
                    };

                    var jobHandle = uniqueJob.Schedule();
                    jobHandle.Complete();

                    Assert.That(uniqueMap.Length, Is.EqualTo(5));
                    Assert.That(uniqueMap.ContainsKey(5), Is.True);
                }
        }
        
    }
}