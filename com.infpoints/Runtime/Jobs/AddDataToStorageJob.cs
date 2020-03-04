using InfPoints.NativeCollections;
using Unity.Collections;
using Unity.Jobs;

namespace InfPoints.Jobs
{
    /// <summary>
    /// Add points to storage. Deallocate the data once it's added.
    /// </summary>
    public struct AddDataToStorageJob : IJob
    {
        [DeallocateOnJobCompletion] [ReadOnly] readonly NativeArrayXYZ<float> m_Data;
        [DeallocateOnJobCompletion] [ReadOnly] NativeInt m_DataLength;
        [ReadOnly] readonly ulong m_SparseIndex;
        NativeSparsePagedArrayXYZ m_Storage;

        /// <summary>
        /// Create the job for adding data to storage.
        /// </summary>
        /// <param name="storage">The storage that will receive the data.</param>
        /// <param name="sparseIndex">The sparse index in which to add the data.</param>
        /// <param name="data">The data to add. Will be deallocated once added.</param>
        /// <param name="dataLength">The length of the data to be added. Will be deallocated once it's added.</param>
        public AddDataToStorageJob(NativeSparsePagedArrayXYZ storage, ulong sparseIndex, NativeArrayXYZ<float> data,
            NativeInt dataLength)
        {
            m_Storage = storage;
            m_SparseIndex = sparseIndex;
            m_Data = data;
            m_DataLength = dataLength;
        }

        public void Execute()
        {
            Logger.LogFormat(LogMessage.DataCountAddedToStorage, m_DataLength.Value);
            m_Storage.AddData(m_SparseIndex, m_Data, m_DataLength.Value);
        }
    }
}