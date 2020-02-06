using System;
using Unity.Collections;

namespace InfPoints.NativeCollections
{
    public static class Checks
    {
        public static void CheckNullAndThrow<T>(NativeArray<T> data) where T : unmanaged
        {
            if (data == default) throw new ArgumentNullException(nameof(data));
        }

    }
}