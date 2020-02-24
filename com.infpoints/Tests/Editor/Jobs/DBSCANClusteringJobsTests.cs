using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Tests.Editor.Jobs
{
    public class DBSCANClusteringJobsTests
    {
        [Test]
        public void SetClustersReturnsTheCorrectResult()
        {
            float3[] dataValues = new[]
            {
                float3.zero, 
                new float3(1,0,0), 
                new float3(0,0,2),
            };
            
            BitField32[] clusterValues = new[]
            {
                new BitField32(1<<2),  
                new BitField32(1<<3),  
                new BitField32(1<<6),  
            };

            using (var data = new NativeArray<float3>(dataValues, Allocator.TempJob))
            using (var clusters = new NativeArray<BitField32>(clusterValues, Allocator.TempJob))
            using(var cluster = new NativeInt(1<<5, Allocator.TempJob))
            {
                new SetClustersJob(data, clusters, 1, float3.zero, cluster).Schedule(data.Length, 4).Complete();
                
                var clusterBits = new BitField32((uint)cluster.Value);
                
                Assert.That(clusterBits.IsSet(2), Is.True);
                Assert.That(clusterBits.IsSet(3), Is.True);
                Assert.That(clusterBits.IsSet(5), Is.True);
                
                Assert.That(clusters[0].IsSet(5), Is.True);
                Assert.That(clusters[0].IsSet(2), Is.True);
                Assert.That(clusters[1].IsSet(5), Is.True);
                Assert.That(clusters[2].IsSet(5), Is.False);

                int lowestPosition = clusterBits.CountTrailingZeros();
                Assert.That(lowestPosition, Is.EqualTo(2));
            }
        }
        
        
    }
}