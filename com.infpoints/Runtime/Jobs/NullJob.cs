using Unity.Jobs;

namespace InfPoints.Jobs
{
    public struct NullJob : IJob
    {
        public void Execute()
        {
            // no-op
        }
    }
}