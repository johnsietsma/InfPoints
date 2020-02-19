using System.Collections;
using System.Collections.Generic;
using InfPoints;
using InfPoints.NativeCollections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace InfPoints.Tests.Editor.NativeCollections
{

    public class NativeSparseArrayJobTests
    {
        const int AddCount = 1000;

        public struct AddJob : IJob
        {
            public NativeSparseArray<int> array;
            public NativeInt addedCount;

            public void Execute()
            {
                for (int i = 0; i < AddCount; i++)
                {
                    array.AddValue(i, i);
                    addedCount.Increment();
                }
            }
        }

        [Test]
        public void AddJobAddsCorrectAmount()
        {
            using (var array = new NativeSparseArray<int>(AddCount, Allocator.TempJob))
            using (var count = new NativeInt(0, Allocator.TempJob))
            {
                var addJob = new AddJob()
                {
                    array = array,
                    addedCount = count
                };

                var jobData = addJob.Schedule();
                jobData.Complete();

                Assert.That(addJob.addedCount.Value, Is.EqualTo(AddCount));
                Assert.That(array.Length, Is.EqualTo(AddCount));
            }
        }
    }

}