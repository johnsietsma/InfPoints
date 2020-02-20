using System;
using System.Diagnostics;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace InfPoints
{
    /// <summary>
    /// Use a simple int as a NativeContainer. This allows data to be passed out of a Job without having to create
    /// a new NativeArray to hold a single value.
    /// Use <c>NativeInt.Concurrent</c> in IJobParallelFor jobs. 
    /// Supports the [DeallocateOnJobCompletion] attribute.
    /// </summary>
    [NativeContainer]
    [NativeContainerSupportsDeallocateOnJobCompletion]
    [DebuggerTypeProxy(typeof(NativeIntDebugView))]
    [DebuggerDisplay("Value = {Value}")]
    public unsafe partial struct NativeInt : IDisposable
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        /// <summary>
        /// Has the value been created and not yet destroyed.
        /// Be aware that this will fail if a copy of the struct is taken, say for example by a job.
        /// A copy of the pointer is made and wont be set to null when the NativeInt is Disposed.
        /// </summary>
        public bool IsCreated => m_Buffer != null;

        [NativeDisableUnsafePtrRestriction] int* m_Buffer;
        readonly Allocator m_AllocatorLabel;
        
        /// <summary>
        /// Create a new NativeInt with an initial value of 0.
        /// </summary>
        /// <param name="allocatorLabel"></param>
        public NativeInt(Allocator allocatorLabel) : this(0, allocatorLabel)
        {
        }

        /// <summary>
        /// Create a new NativeValue with an initial value.
        /// </summary>
        public NativeInt(int initialValue, Allocator allocatorLabel)
        {
            m_Buffer = (int*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), UnsafeUtility.AlignOf<int>(),
                allocatorLabel);
            m_AllocatorLabel = allocatorLabel;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocatorLabel);
#endif
            *m_Buffer = initialValue;
        }

        public int Value
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                //TODO: Why does this trigger? AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return *m_Buffer;
            }
            [WriteAccessRequired]
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                Interlocked.Exchange(ref *m_Buffer, value);
            }
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

        public Concurrent ToConcurrent()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var concurrent = new Concurrent(m_Buffer, m_Safety);
            AtomicSafetyHandle.UseSecondaryVersion(ref concurrent.m_Safety);
#else
            var concurrent = new Concurrent(m_Buffer);
#endif
            return concurrent;
        }

        [WriteAccessRequired]
        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            if (m_Buffer != null)
                Deallocate();
        }

        void Deallocate()
        {
            UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);
            m_Buffer = null;
        }

        [BurstCompile]
        struct DisposeJob : IJob
        {
            public NativeInt Value;

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
            DisposeSentinel.Clear(ref m_DisposeSentinel);
#endif

            var jobHandle = new DisposeJob {Value = this}.Schedule(dependency);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_Safety);
#endif

            return jobHandle;
        }
    }

    internal sealed class NativeIntDebugView
    {
        private NativeInt m_NativeInt;

        public NativeIntDebugView(NativeInt nativeInt)
        {
            m_NativeInt = nativeInt;
        }

        public int Value => m_NativeInt.Value;
    }
}