using System;
using Unity.Collections;

namespace InfPoints
{
    public struct NativeArrayXYZ<T> : IDisposable where T : unmanaged
    {
        public int Length => X.Length;
        public NativeArray<T> X;
        public NativeArray<T> Y;
        public NativeArray<T> Z;

        public NativeArrayXYZ(int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            X = new NativeArray<T>(length, allocator, options);
            Y = new NativeArray<T>(length, allocator, options);
            Z = new NativeArray<T>(length, allocator, options);
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            Z.Dispose();
        }
    }
}