using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace InfPoints
{
    public struct XYZSoA<T>: IDisposable where T : unmanaged
    {
        public int Length => X.Length;
        public NativeArray<T> X;
        public NativeArray<T> Y;
        public NativeArray<T> Z;

        public XYZSoA(int length, Allocator allocator, NativeArrayOptions options=NativeArrayOptions.ClearMemory)
        {
            X = new NativeArray<T>(length, allocator, options);
            Y = new NativeArray<T>(length, allocator, options);
            Z = new NativeArray<T>(length, allocator, options);
        }
        
        public XYZSoA<U> Reinterpret<U>() where U : unmanaged
        {
            return new XYZSoA<U>()
            {
                X = X.Reinterpret<U>(UnsafeUtility.SizeOf<T>()),
                Y = X.Reinterpret<U>(UnsafeUtility.SizeOf<T>()),
                Z = X.Reinterpret<U>(UnsafeUtility.SizeOf<T>())
            };
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            Z.Dispose();
        }
    }
}