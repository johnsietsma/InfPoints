using Unity.Collections;

namespace InfPoints.Jobs
{
    public static class LogMessage
    {
        public static readonly string PointsCollected = "[CollectPointsJob] Collected {0} points";
        public static readonly string DataCountAddedToStorage = "[AddDataToStorageJob] Adding {0} points.";
        public static readonly string UniqueValuesCollected = "[GetUniqueValuesJob] Collected {0} unique values";
    }
}