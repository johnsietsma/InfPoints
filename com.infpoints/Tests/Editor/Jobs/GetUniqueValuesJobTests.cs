using InfPoints.Jobs;
using InfPoints.NativeCollections;
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
            ulong[] values = {1, 1, 2, 3, 5, 5, 6, 6};
            using (var valuesArray = new NativeArray<ulong>(values, Allocator.TempJob))
            using (var uniqueValues = new NativeSparseArray<ulong, int>(valuesArray.Length, Allocator.TempJob))
            {
                var uniqueJob = new GetUniqueValuesJob<int>(valuesArray, uniqueValues);


                var collectUniqueJobHandle = uniqueJob.Schedule();
                collectUniqueJobHandle.Complete();

                Assert.That(uniqueValues.Length, Is.EqualTo(5));
                Assert.That(uniqueValues.ContainsIndex(5), Is.True);
            }
        }
    }
}