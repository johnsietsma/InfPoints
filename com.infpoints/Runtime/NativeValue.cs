using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace InfPoints
{
    /// <summary>
    /// Use a simple value as a NativeContainer. This allows data to be passed out of a Job without having to create
    /// a new NativeArray to hold a single value.s
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [NativeContainer]
    public unsafe struct NativeValue<T> : IDisposable
        where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public bool IsCreated => m_Buffer != null;

        public T Value
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return *(T*) (m_Buffer);
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                *(T*) m_Buffer = value;
            }
        }

        [NativeDisableUnsafePtrRestriction] void* m_Buffer;

        int m_Length;
        readonly Allocator m_AllocatorLabel;

        /// <summary>
        /// </summary>
        public NativeValue(T initialValue, Allocator allocatorLabel)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocatorLabel);
#endif
            m_Buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocatorLabel);
            m_AllocatorLabel = allocatorLabel;
            *(T*) m_Buffer = initialValue;
            m_Length = 0;
        }

        void Deallocate()
        {
            UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);
            m_Buffer = null;
        }


        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            if (m_Buffer != null)
                Deallocate();
        }

        [BurstCompile]
        struct DisposeJob : IJob
        {
            public NativeValue<T> Value;

            public void Execute()
            {
                Value.Deallocate();
            }
        }

        /// <summary>
        /// Safely disposes of this container and deallocates its memory when the jobs that use it have completed.
        /// </summary>
        /// <remarks>You can call this function dispose of the container immediately after scheduling the job. Pass
        /// the [JobHandle](https://docs.unity3d.com/ScriptReference/Unity.Jobs.JobHandle.html) returned by
        /// the [Job.Schedule](https://docs.unity3d.com/ScriptReference/Unity.Jobs.IJobExtensions.Schedule.html)
        /// method using the `jobHandle` parameter so the job scheduler can dispose the container after all jobs
        /// using it have run.</remarks>
        /// <param name="dependency">All jobs spawned will depend on this JobHandle.</param>
        /// <returns>A new job handle containing the prior handles as well as the handle for the job that deletes
        /// the container.</returns>
        public JobHandle Dispose(JobHandle dependency)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // [DeallocateOnJobCompletion] is not supported, but we want the deallocation
            // to happen in a thread. DisposeSentinel needs to be cleared on main thread.
            // AtomicSafetyHandle can be destroyed after the job was scheduled (Job scheduling
            // will check that no jobs are writing to the container).
            DisposeSentinel.Clear(ref m_DisposeSentinel);
#endif

            var jobHandle = new DisposeJob {Value = this}.Schedule(dependency);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_Safety);
#endif

            return jobHandle;
        }
    }
}