using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct FilterFullNodesJob : IJobParallelForFilter
    {
        [ReadOnly] public NativeHashMap<Node,NodeStorage> PointsStorage;
        [ReadOnly] public NativeArray<ulong> MortonCodes;
        [ReadOnly] public int LevelIndex;
        
        public void Execute(int index)
        {
            var node = new Node() {LevelIndex = LevelIndex, MortonCode = MortonCodes[index]};
            PointsStorage[index].IsFull;
        }
    }
}