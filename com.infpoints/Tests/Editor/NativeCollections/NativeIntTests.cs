using JacksonDunstan.NativeCollections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.NativeCollections
{
    internal struct IncrementIntJob : IJobParallelFor
    {
        public NativeInt Count;

        public void Execute(int index)
        {
            Count.Increment();
        }
    }
    
    internal struct DeallocIntJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] public NativeInt Count;

        public void Execute(int index)
        {
            Count.Increment();
        }
    }
    
    internal struct AccessIntJob : IJob
    {
        public NativeInt Count;

        public void Execute()
        {
            int value = Count.Value;
        }
    }

    public class NativeIntTests
    {
        [Test]
        public void AddingGivesTheCorrectResult()
        {
            using (var nativeInt = new NativeInt(0, Allocator.TempJob))
            {
                Assert.That(nativeInt.Value, Is.EqualTo(0));
                nativeInt.Add(5);
                Assert.That(nativeInt.Value, Is.EqualTo(5));
            }
        }

        [Test]
        public void IncrementingGivesTheCorrectResult()
        {
            using (var nativeInt = new NativeInt(10, Allocator.TempJob))
            {
                Assert.That(nativeInt.Value, Is.EqualTo(10));
                nativeInt.Increment();
                Assert.That(nativeInt.Value, Is.EqualTo(11));
            }
        }

        [Test]
        public void DecrementingGivesTheCorrectResult()
        {
            using (var nativeInt = new NativeInt(0, Allocator.TempJob))
            {
                Assert.That(nativeInt.Value, Is.EqualTo(0));
                nativeInt.Decrement();
                Assert.That(nativeInt.Value, Is.EqualTo(-1));
            }
        }

        [Test]
        public void IncrementGivesCorrectResultInJob()
        {
            const int incrementCount = 1024 * 1024;
            const int batchCount = 2;

            using (var nativeInt = new NativeInt(0, Allocator.TempJob))
            {
                var incrementJob = new IncrementIntJob()
                {
                    Count = nativeInt
                }.Schedule(incrementCount, batchCount);

                Assert.That(nativeInt.IsCreated, Is.True);
                incrementJob.Complete();

                Assert.That(incrementCount, Is.EqualTo(nativeInt.Value));
            }
        }
        
        [Test]
        public void DeallocsOnJobCompletion()
        {
            const int incrementCount = 1;
            const int batchCount = 1;

            var nativeInt = new NativeInt(0, Allocator.TempJob);
            nativeInt.Value = 5; // Legal before the job is scheduled
            
            var deallocJob = new DeallocIntJob()
            {
                Count = nativeInt
            }.Schedule(incrementCount, batchCount);
            
            deallocJob.Complete();
            
            Assert.That(()=>nativeInt.Value, Throws.InvalidOperationException); // Deallocated, illegal access
        }
        
        [Test]
        public void ReadingValueGivesTheCorrectResult()
        {
            using (var nativeInt = new NativeInt(Allocator.TempJob))
            {
                new AccessIntJob()
                {
                    Count = nativeInt
                }.Schedule().Complete();
            }
        }
    }
}