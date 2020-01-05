using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class OctreeUtils
{
    /// <summary>
    /// Return the number of nodes in the given level 
    /// </summary>
    /// <param name="levelIndex"></param>
    /// <returns>The number of nodes in the level</returns>
    public static int GetNodeCount(int levelIndex)
    {
        return (int)math.pow(8, levelIndex);
    }
}
