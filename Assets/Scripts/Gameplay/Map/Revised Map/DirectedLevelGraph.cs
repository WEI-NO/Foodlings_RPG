using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Overworld/Directed Graph")]
public class DirectedLevelGraph : ScriptableObject
{
    public List<NodeData> nodes;
}

[System.Serializable]
public class NodeData
{
    public int levelIndex;
    public List<int> parentIndices;
}

public class LevelNode
{
    public int levelIndex;
    public Vector3 worldPosition;

    public List<LevelNode> parents = new();
    public List<LevelNode> children = new();

    public bool IsUnlocked(Func<int, bool> isCompleted)
    {
        if (parents.Count == 0) return true;
        return parents.All(p => isCompleted(p.levelIndex));
    }
}