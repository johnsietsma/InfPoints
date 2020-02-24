using System.Linq.Expressions;
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
                new float3(1, 0, 0),
                new float3(0, 0, 2),
            };

            int[] clusterPositions = new[] {1, 1, 2};

            const float epsilon = 1;
            const int innerLoopBatchCount = 4;
            BitField32 zeroBitField = new BitField32(0);

            using (var data = new NativeArray<float3>(dataValues, Allocator.TempJob))
            using (var clusters = new NativeArray<BitField32>(data.Length, Allocator.TempJob))
            {
                var firstUnusedIndex = new NativeInt(0, Allocator.TempJob);
                int clusterNumber = 1;
                var cluster = new NativeInt(Allocator.TempJob);

                do
                {
                    cluster.Value = 1 << clusterNumber++;
                    new SetClustersJob(data, clusters, epsilon, dataValues[firstUnusedIndex.Value], cluster)
                        .Schedule(data.Length, innerLoopBatchCount)
                        .Complete();

                    var clusterBits = new BitField32((uint) cluster.Value);
                    int lowestPosition = clusterBits.CountTrailingZeros();
                    //Assert.That(lowestPosition, Is.EqualTo(2));

                    new AggregateClustersJob(clusters, clusterBits, lowestPosition)
                        .Schedule(data.Length, innerLoopBatchCount)
                        .Complete();

                    new NativeArrayIndexOfJob_BitField32(clusters, zeroBitField, firstUnusedIndex).Schedule()
                        .Complete();
                    
                } while (firstUnusedIndex.Value != -1);

                for (int index = 0; index < clusters.Length; index++)
                {
                    Assert.That(clusters[index].IsSet(clusterPositions[index]), Is.True, index.ToString());
                }

                firstUnusedIndex.Dispose();
                cluster.Dispose();
            }
        }
    }
}