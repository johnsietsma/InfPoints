using System;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct NativeHashMapGetKeysJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [ReadOnly] public NativeHashMap<TKey, TValue> NativeHashMap;
        public NativeList<TKey> Keys;

        public void Execute()
        {
            var keyArray = NativeHashMap.GetKeyArray(Allocator.Temp);
            Logger.Log($"Getting {keyArray.Length} keys");
            Keys.AddRange(keyArray);
        }
    }
}