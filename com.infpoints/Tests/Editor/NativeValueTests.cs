using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor
{
    public class NativeValueTests
    {
        private struct WriteValueJob<T> : IJob where T : unmanaged
        {
            public T valueToWrite;
            public NativeValue<T> value;
            
            public void Execute()
            {
                value.Value = valueToWrite;
            }
        }

        [Test]
        public void WriteValueInJob()
        {
            var job = new WriteValueJob<int>()
            {
                valueToWrite = 2,
                value = new NativeValue<int>(1, Allocator.Persistent)
            };

            var jobData = job.Schedule();
            jobData.Complete();
            Assert.That(job.value.Value, Is.EqualTo(2));
            job.value.Dispose();
        }

        [Test]
        public void DisposeValueInAJob()
        {
            var value = new NativeValue<int>(1, Allocator.Persistent);    

            var writeJob = new WriteValueJob<int>()
            {
                value = value
            };
            var writeJobHandle = writeJob.Schedule();
            var jobHandle = value.Dispose(writeJobHandle);
            Assert.That(value.IsCreated, Is.True);
            jobHandle.Complete();
            Assert.That(value.IsCreated, Is.False);
        }
        
    }
}