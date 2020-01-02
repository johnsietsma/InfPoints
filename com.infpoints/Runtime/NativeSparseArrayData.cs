using System;
using Unity.Collections;

/// <summary>
/// A simple container for the data used by <see cref="NativeSparseArray<T>"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct NativeSparseArrayData<T> : IDisposable
#if CSHARP_7_3_OR_NEWER
    where T : unmanaged
#else
        where T : struct
#endif
{
    public int UsedElementCount;
    public NativeArray<int> Indices;
    public NativeArray<T> Data;

    public NativeSparseArrayData(int length, Allocator allocator)
    {
        Indices = new NativeArray<int>(length, allocator, NativeArrayOptions.UninitializedMemory);
        Data = new NativeArray<T>(length, allocator, NativeArrayOptions.UninitializedMemory);
        UsedElementCount = 0;
    }

    public void Dispose()
    {
        Indices.Dispose();
        Data.Dispose();
    }
}
