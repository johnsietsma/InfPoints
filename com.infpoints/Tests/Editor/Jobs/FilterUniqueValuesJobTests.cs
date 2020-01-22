using InfPoints.Jobs;
using JacksonDunstan.NativeCollections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class FilterUniqueValuesJobTests
    {
        struct IsEvenNumberFilter : IJobParallelForFilter
        {
            [ReadOnly] public NativeArray<int> Values;
            
            public bool Execute(int index)
            {
                return (Values[index] % 2)==0;
            }
        }
        
        
        [Test]
        public void DoesFilterUniqueValues()
        {
            int[] values = {1, 1, 2, 3, 5, 5, 6, 6};
            using (var valuesArray = new NativeArray<int>(values, Allocator.TempJob))
            using (var uniqueMap = new NativeHashSet<int>(valuesArray.Length, Allocator.TempJob))
            using(var indices = new NativeList<int>(valuesArray.Length, Allocator.TempJob) )
            {
                var uniqueJob = new FilterUniqueValuesJob<int>()
                {
                    Values = valuesArray,
                    UniqueValues = uniqueMap
                };


                var collectUniqueJobHandle = uniqueJob.ScheduleAppend(indices, valuesArray.Length, 128);
                collectUniqueJobHandle.Complete();

                Assert.That(uniqueMap.Length, Is.EqualTo(5));
                Assert.That(uniqueMap.Contains(5), Is.True);
                Assert.That(indices.Length, Is.EqualTo(5));
                Assert.That(indices[0], Is.EqualTo(0));
                Assert.That(indices[1], Is.EqualTo(2));

                var evenFilterJob = new IsEvenNumberFilter()
                {
                    Values = valuesArray
                };
                var evenFilterHandle = evenFilterJob.ScheduleAppend(indices, valuesArray.Length, 128);
                evenFilterHandle.Complete();
                
                Assert.That(indices.Length, Is.EqualTo(8));

            }
        }
    }
}