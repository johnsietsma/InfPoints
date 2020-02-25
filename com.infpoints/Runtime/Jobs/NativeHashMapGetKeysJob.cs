using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct NativeHashMapGetKeysJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [ReadOnly] public NativeHashMap<TKey, TValue> NativeHashMap;
        public NativeList<TKey> Keys;

        public NativeHashMapGetKeysJob(NativeHashMap<TKey, TValue> nativeHashMap, NativeList<TKey> keys)
        {
            NativeHashMap = nativeHashMap;
            Keys = keys;
        }

        public void Execute()
        {
            var keyArray = NativeHashMap.GetKeyArray(Allocator.Temp);
            Logger.LogFormat( LogMessage.KeysCount, keyArray.Length);
            Keys.AddRange(keyArray);
        }
    }
}