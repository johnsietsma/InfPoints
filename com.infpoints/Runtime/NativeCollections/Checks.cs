using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    public static class Checks
    {
        public static void CheckNullAndThrow<T>(NativeSlice<T> data, string paramName) where T : unmanaged
        {
            if (data == default) throw new ArgumentNullException(paramName);
        }
        
        public static void CheckNullAndThrow<T>(NativeArray<T> data, string paramName) where T : unmanaged
        {
            if (data == default) throw new ArgumentNullException(paramName);
        }
        
        public static unsafe void CheckNullAndThrow(void* data, string paramName)
        {
            if (data == null) throw new ArgumentNullException(paramName);
        }
    }
}