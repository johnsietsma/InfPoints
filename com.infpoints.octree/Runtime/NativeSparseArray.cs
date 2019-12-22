using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

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

        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
        static readonly int DisposeSentinelStackDepth = 2;
#endif

        public int Length => m_Data.Length;
        public NativeArray<T> Data => m_Data;

        NativeArray<T> m_Data;
        NativeArray<int> m_Indices;

        public NativeSparseArray(int length, Allocator allocator,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            var totalSize = UnsafeUtility.SizeOf<T>() * (long) length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent.
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be >= 0");
            if (totalSize > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(length),
                    $"Capacity * sizeof(T) cannot exceed {int.MaxValue} bytes");

            CollectionHelper.CheckIsUnmanaged<T>();
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, DisposeSentinelStackDepth, allocator);
#endif

            m_Indices = new NativeArray<int>(length, allocator, NativeArrayOptions.ClearMemory);
            m_Data = new NativeArray<T>(length, allocator, NativeArrayOptions.ClearMemory);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
        }

        public T this[int index]
        {
            get => FindData(index);
            set => SetData(index, value);
        }

        public bool ContainsIndex(int sparseIndex)
        {
            return m_Indices.BinarySearch(sparseIndex) >= 0;
        }

        public int SetData(int sparseIndex, T data)
        {
            int dataIndex = m_Indices.BinarySearch(sparseIndex);
            if (dataIndex < 0)
            {
                // Doesn't exist yet, insert it
                dataIndex = ~dataIndex; // 1s complement is the insertion point
                m_Indices.Insert(dataIndex, sparseIndex);
                m_Data.Insert(dataIndex, data);
            }
            else
            {
                // Update the data
                m_Data[dataIndex] = data;
            }

            return dataIndex;
        }

        public void RemoveAt(int sparseIndex)
        {
            int dataIndex = m_Indices.BinarySearch(sparseIndex);
            if (dataIndex < 0) throw new ArgumentOutOfRangeException();
            m_Indices.RemoveAt(dataIndex);
            m_Data.RemoveAt(dataIndex);
        }

        T FindData(int sparseIndex)
        {
            int dataIndex = m_Indices.BinarySearch(sparseIndex);
            if (dataIndex < 0) return default(T);
            return m_Data[dataIndex];
        }


        void Deallocate()
        {
            m_Indices.Dispose();
            m_Data.Dispose();
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            Deallocate();
        }

        /// <summary>
        /// Safely disposes of this container and deallocate its memory when the jobs that use it have completed.
        /// </summary>
        /// <remarks>You can call this function dispose of the container immediately after scheduling the job. Pass
        /// the [JobHandle](https://docs.unity3d.com/ScriptReference/Unity.Jobs.JobHandle.html) returned by
        /// the [Job.Schedule](https://docs.unity3d.com/ScriptReference/Unity.Jobs.IJobExtensions.Schedule.html)
        /// method using the `jobHandle` parameter so the job scheduler can dispose the container after all jobs
        /// using it have run.</remarks>
        /// <param name="jobHandle">The job handle or handles for any scheduled jobs that use this container.</param>
        /// <returns>A new job handle containing the prior handles as well as the handle for the job that deletes
        /// the container.</returns>
        public JobHandle Dispose(JobHandle inputDeps)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // [DeallocateOnJobCompletion] is not supported, but we want the deallocation
            // to happen in a thread. DisposeSentinel needs to be cleared on main thread.
            // AtomicSafetyHandle can be destroyed after the job was scheduled (Job scheduling
            // will check that no jobs are writing to the container).
            DisposeSentinel.Clear(ref m_DisposeSentinel);
#endif
            var jobHandle = new DisposeJob {Container = this}.Schedule(inputDeps);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_Safety);
#endif
            return jobHandle;
        }

        [BurstCompile]
        struct DisposeJob : IJob
        {
            public NativeSparseArray<T> Container;

            public void Execute()
            {
                Container.Deallocate();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Data.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_Data.GetEnumerator();
        }
    }

    sealed class NativeSparseArrayDebugView<T> where T : struct
    {
        NativeSparseArray<T> m_Array;

        public NativeSparseArrayDebugView(NativeSparseArray<T> array)
        {
            m_Array = array;
        }

        public T[] Items => m_Array.Data.ToArray();
    }
}