using Unity.Collections;

namespace InfPoints.Jobs
{
    public static class LogMessage
    {
        public static string PointsCollected = "[CollectPointsJob] Collected {0} points";
        public static string DataCountAddedToStorage = "[AddDataToStorageJob] Adding {0} points.";
        public static string UniqueValuesCollected = "[GetUniqueValuesJob] Collected {0} unique values";
    }
}