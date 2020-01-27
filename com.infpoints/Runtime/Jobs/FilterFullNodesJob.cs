using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct FilterFullNodesJob : IJobParallelForFilter
    {
        [ReadOnly] public NativeHashMap<Node, PointsStorage> NodePointsMap;
        [ReadOnly] public NativeArray<ulong> MortonCodes;
        [ReadOnly] public int LevelIndex;
        
        public bool Execute(int index)
        {
            var node = new Node() {LevelIndex = LevelIndex, MortonCode = MortonCodes[index]};
            return NodePointsMap[node].IsFull;
        }
    }
}