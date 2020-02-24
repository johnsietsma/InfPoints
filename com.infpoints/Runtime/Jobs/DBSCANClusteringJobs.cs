using System;
using System.Security.Cryptography.X509Certificates;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace InfPoints.Jobs
{
    //[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct SetClustersJob : IJobParallelFor
    {
        [ReadOnly] NativeArray<float3> Points;
        NativeArray<BitField32> Clusters;
        [ReadOnly] float EpsilonSquared;
        float3 ClusterPoint;
        [NativeDisableParallelForRestriction]
        NativeInt Cluster;

        public SetClustersJob(NativeArray<float3> points, NativeArray<BitField32> clusters, float epsilon, float3 clusterPoint,
            NativeInt cluster)
        {
            Points = points;
            Clusters = clusters;
            EpsilonSquared = math.pow(epsilon, 2);
            ClusterPoint = clusterPoint;
            Cluster = cluster;
        }

        public void Execute(int index)
        {
            if (math.distancesq(Points[index], ClusterPoint) <= EpsilonSquared)
            {
                var clusters = Clusters[index];
                clusters.Value |= (uint)Cluster.Value;
                
                // Both belong to the same clusters
                Clusters[index] = clusters;
                Cluster.Value = (int)clusters.Value;
            }
        }
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct AggregateClustersJob : IJobParallelFor
    {
        NativeArray<BitField32> DataClusters;
        [ReadOnly] BitField32 Cluster;
        [ReadOnly] int ClusterPosition;

        public AggregateClustersJob(NativeArray<BitField32> clusters, BitField32 cluster, int clusterPosition)
        {
            DataClusters = clusters;
            Cluster = cluster;
            ClusterPosition = clusterPosition;
        }

        public void Execute(int index)
        {
            // The point is in our cluster
            if (DataClusters[index].TestAll(ClusterPosition))
            {
                // Nuke the all the clusters but ours
                var clusters = DataClusters[index];
                clusters.Value &= ~(Cluster.Value | Cluster.Value);
            }
        }
    }


}