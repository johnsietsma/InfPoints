using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints
{
    public struct XYZNativeArray<T> : IDisposable where T : unmanaged
    {
        public int Length => X.Length;
        public NativeArray<T> X;
        public NativeArray<T> Y;
        public NativeArray<T> Z;

        public XYZNativeArray(int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            X = new NativeArray<T>(length, allocator, options);
            Y = new NativeArray<T>(length, allocator, options);
            Z = new NativeArray<T>(length, allocator, options);
        }

        public XYZNativeArray<U> Reinterpret<U>() where U : unmanaged
        {
            return new XYZNativeArray<U>()
            {
                X = X.Reinterpret<U>(UnsafeUtility.SizeOf<T>()),
                Y = Y.Reinterpret<U>(UnsafeUtility.SizeOf<T>()),
                Z = Z.Reinterpret<U>(UnsafeUtility.SizeOf<T>())
            };
        }
        
        public NativeArray<T> GetXYZ(int index)
        {
            switch (index)
            {
                case 0: return X;
                case 1: return Y;
                case 2: return Z;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        public void CopyFrom(XYZNativeArray<T> nativeArray)
        {
            X.CopyFrom(nativeArray.X);
            Y.CopyFrom(nativeArray.Y);
            Z.CopyFrom(nativeArray.Z);
        }

        public static void Copy(XYZNativeArray<T> src,
            int srcIndex,
            XYZNativeArray<T> dst,
            int dstIndex,
            int length)
        {
            NativeArray<T>.Copy(src.X, srcIndex, dst.X, dstIndex, length);
            NativeArray<T>.Copy(src.Y, srcIndex, dst.Y, dstIndex, length);
            NativeArray<T>.Copy(src.Z, srcIndex, dst.Y, dstIndex, length);
        }

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
            Z.Dispose();
        }
    }
}