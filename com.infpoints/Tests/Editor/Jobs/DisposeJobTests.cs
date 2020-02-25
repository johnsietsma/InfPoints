using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using InfPoints.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class DisposeJobTests
    {
        [Test]
        public void NativeArrayDispose()
        {
            var array = new NativeArray<int>(10, Allocator.TempJob);
            new DisposeJob_NativeArray<int>(array).Schedule().Complete();
            Assert.That(()=>array.Dispose(), Throws.InvalidOperationException);
        }
    }
}