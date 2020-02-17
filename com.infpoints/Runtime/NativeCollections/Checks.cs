using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.NativeCollections
{
    public static class Checks
    {
        public static unsafe void CheckNullAndThrow<T>(NativeArray<T> data, string paramName) where T : unmanaged
        {
            CheckNullAndThrow(data.GetUnsafeReadOnlyPtr(), paramName);
        }
        
        public static unsafe void CheckNullAndThrow(void* data, string paramName)
        {
            if (data == null) throw new ArgumentNullException(paramName);
        }

    }
}