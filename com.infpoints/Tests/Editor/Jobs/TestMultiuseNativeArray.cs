using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public struct TestMultiUse : IJob
    {
        [ReadOnly] public NativeArray<float> DataX;
        [ReadOnly] public NativeArray<float> DataY;
        [ReadOnly] public NativeArray<float> DataZ;

        public TestMultiUse(NativeArrayXYZ<float> data)
        {
            DataX = data.X;
            DataY = data.Y;
            DataZ = data.Z;
        }

        public void Execute()
        {
        }
    }


    public class TestMultiuseNativeArray
    {
        [Test]
        public void TestMultiUse()
        {
            using (var data = new NativeArrayXYZ<float>(10, Allocator.TempJob))
            {
                var jobHandles = new NativeArray<JobHandle>(100, Allocator.TempJob);
                for (int i = 0; i < 10; i++)
                {
                    jobHandles[i] = new TestMultiUse(data).Schedule();
                }

                JobHandle.CombineDependencies(jobHandles).Complete();
                jobHandles.Dispose();
            }
        }
    }
}