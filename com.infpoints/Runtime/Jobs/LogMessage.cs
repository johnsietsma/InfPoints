using Unity.Collections;

namespace InfPoints.Jobs
{
    public static class LogMessage
    {
        public static FixedString128 PointsCollected = "[CollectPointsJob] Collected {0} points";
        public static FixedString128 DataCountAddedToStorage = "[AddDataToStorageJob] Adding {0} points.";
        public static FixedString128 UniqueValuesCollected = "[GetUniqueValuesJob] Collected {0} unique values";
        public static FixedString128 KeysCount = "[NativeHashMapGetKeysJob] Getting {0} keys";
    }
}