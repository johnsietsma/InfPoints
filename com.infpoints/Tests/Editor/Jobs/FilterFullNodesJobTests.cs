using InfPoints.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Tests.Editor.Jobs
{
    public class FilterFullNodesJobTests
    {
        [Test]
        public void DoesFilterFullNodes()
        {
            const int Length = 10;
            using (var nodePointsMap = new NodePointsMap(Length, Allocator.TempJob))
            using( var points = new XYZSoA<float>(Length, Allocator.TempJob))
            using( var codes = new NativeArray<ulong>(Length, Allocator.TempJob))
            using( var indices = new NativeList<int>(Length, Allocator.TempJob))
            {
                var node1 = new Node() { LevelIndex = 0, MortonCode = 0 };
                var node2 = new Node() { LevelIndex = 1, MortonCode = 1 };
                nodePointsMap.AddPoints(node1, points);
                nodePointsMap.AddPoints(node2, default);
                var isFullJob = new FilterFullNodesJob()
                {
                    LevelIndex = 0,
                    MortonCodes = codes,
                    NodePointsMap = nodePointsMap.NodePoints
                }.ScheduleAppend(indices, Length, 4);
                
                isFullJob.Complete();
                
                Assert.That(indices.Length, Is.EqualTo(1));
            }
            
        }
    }
}