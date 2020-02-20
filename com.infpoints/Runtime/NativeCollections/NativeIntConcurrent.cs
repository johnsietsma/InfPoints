using System;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;

namespace InfPoints
{
    public unsafe partial struct NativeInt : IDisposable
    {
        /// <summary>
        /// A concurrent version of <c>NativeInt</c>.
        /// It contains only write methods and as usable in IJobParallelFor jobs.
        /// </summary>
        [NativeContainer]
        [NativeContainerIsAtomicWriteOnly]
        public struct Concurrent
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            public AtomicSafetyHandle m_Safety;
#endif
            
            [NativeDisableUnsafePtrRestriction]
            readonly int* m_Buffer;

            /// <summary>
            /// Create a new NativeInt with an initial value of 0.
            /// </summary>
            /// <param name="allocatorLabel"></param>
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            public Concurrent(int* buffer, AtomicSafetyHandle safety)
            {
                m_Buffer = buffer;
                m_Safety = safety;
            }
#endif
            
            public Concurrent(int* buffer)
            {
                m_Buffer = buffer;
                m_Safety = default;
            }
            
            [WriteAccessRequired]
            public void Add(int value)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                Interlocked.Add(ref *m_Buffer, value);
            }

            [WriteAccessRequired]
            public void Increment()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                Interlocked.Increment(ref *m_Buffer);
            }

            [WriteAccessRequired]
            public void Decrement()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                Interlocked.Decrement(ref *m_Buffer);
            }
        }
    }
}