using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace InfPoints
{
    public struct Float3SoA<T>: IDisposable where T : unmanaged
    {
        public int Length => X.Length;
        public NativeArray<T> X;
        public NativeArray<T> Y;
        public NativeArray<T> Z;

        public Float3SoA(int length, Allocator allocator, NativeArrayOptions options=NativeArrayOptions.ClearMemory)
        {
            X = new NativeArray<T>(length, allocator, options);
            Y = new NativeArray<T>(length, allocator, options);
            Z = new NativeArray<T>(length, allocator, options);
        }
        
        public Float3SoA<U> Reinterpret<U>() where U : unmanaged
        {
            return new Float3SoA<U>()
            {
                X = X.Reinterpret<U>(UnsafeUtility.SizeOf<T>()),
                Y = Y.Reinterpret<U>(UnsafeUtility.SizeOf<T>()),
                Z = Z.Reinterpret<U>(UnsafeUtility.SizeOf<T>())
            };
        }

        public NativeArray<T> this[int index]
        {
            get
            {
                switch (index)
                {
                    case 1: return X;
                    case 2: return Y;
                    case 3: return Z;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void CopyFrom(Float3SoA<T> array)
        {
            X.CopyFrom(array.X);
            Y.CopyFrom(array.Y);
            Z.CopyFrom(array.Z);
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            Z.Dispose();
        }
    }
}