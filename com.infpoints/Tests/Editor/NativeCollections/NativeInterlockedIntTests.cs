using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.NativeCollections
{
    internal struct IncrementIntJob : IJobParallelFor
    {
        public NativeInterlockedInt Count;

        public void Execute(int index)
        {
            Count.Increment();
        }
    }
    
    internal struct DeallocIntJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] public NativeInterlockedInt Count;

        public void Execute(int index)
        {
            Count.Increment();
        }
    }

    public class NativeInterlockedIntTests
    {
        [Test]
        public void AddingGivesTheCorrectResult()
        {
            using (NativeInterlockedInt interlockedInt = new NativeInterlockedInt(0, Allocator.TempJob))
            {
                Assert.That(interlockedInt.Value, Is.EqualTo(0));
                interlockedInt.Add(5);
                Assert.That(interlockedInt.Value, Is.EqualTo(5));
            }
        }

        [Test]
        public void IncrementingGivesTheCorrectResult()
        {
            using (NativeInterlockedInt interlockedInt = new NativeInterlockedInt(10, Allocator.TempJob))
            {
                Assert.That(interlockedInt.Value, Is.EqualTo(10));
                interlockedInt.Increment();
                Assert.That(interlockedInt.Value, Is.EqualTo(11));
            }
        }

        [Test]
        public void DecrementingGivesTheCorrectResult()
        {
            using (NativeInterlockedInt interlockedInt = new NativeInterlockedInt(0, Allocator.TempJob))
            {
                Assert.That(interlockedInt.Value, Is.EqualTo(0));
                interlockedInt.Decrement();
                Assert.That(interlockedInt.Value, Is.EqualTo(-1));
            }
        }

        [Test]
        public void IncrementGivesCorrectResultInJob()
        {
            const int incrementCount = 1024 * 1024;
            const int batchCount = 2;

            using (var interlockedInt = new NativeInterlockedInt(0, Allocator.TempJob))
            {
                var incrementJob = new IncrementIntJob()
                {
                    Count = interlockedInt
                }.Schedule(incrementCount, batchCount);

                Assert.That(interlockedInt.IsCreated, Is.True);
                incrementJob.Complete();

                Assert.That(incrementCount, Is.EqualTo(interlockedInt.Value));
            }
        }
        
        [Test]
        public void DeallocsOnJobCompletion()
        {
            const int incrementCount = 1;
            const int batchCount = 1;

            var interlockedInt = new NativeInterlockedInt(0, Allocator.TempJob);
            interlockedInt.Value = 5; // Legal before the job is scheduled
            
            var deallocJob = new DeallocIntJob()
            {
                Count = interlockedInt
            }.Schedule(incrementCount, batchCount);
            
            deallocJob.Complete();
            
            Assert.That(()=>interlockedInt.Value, Throws.InvalidOperationException); // Deallocated
        }
    }
}