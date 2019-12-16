using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints.Octree
{
    /// <summary>
    /// Store array data contiguously, while allowing indices that are far apart.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}")]
    [DebuggerTypeProxy(typeof(NativeSparseArrayDebugView<>))]
    public struct NativeSparseArray<T> : IEnumerable<T>, IDisposable
        where T : struct
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule]
        DisposeSentinel m_DisposeSentinel;
#endif

        NativeArray<T> m_Data;
        NativeArray<int> m_Indices;
        

        public NativeSparseArray<T>()
        {
#if !CSHARP_7_3_OR_NEWER
            if (!UnsafeUtility.IsUnmanaged<T>())
            {
                throw new InvalidOperationException(
                    "Only unmanaged types are supported");
            }
#endif
            
            int length = length0 * length1;
            if (length <= 0)
            {
                throw new InvalidOperationException(
                    "Total number of elements must be greater than zero");
            }


        }
    }
}