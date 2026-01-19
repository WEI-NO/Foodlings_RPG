using System.Collections.Generic;
using UnityEngine;

public static class DLGBuilder
{
    public static Dictionary<int, LevelNode> Build(
        DirectedLevelGraph graphData,
        Dictionary<int, Vector3> levelPositions)
    {
        var nodes = new Dictionary<int, LevelNode>();

        // 1) Create all nodes
        foreach (var data in graphData.nodes)
        {
            if (!levelPositions.TryGetValue(data.levelIndex, out var pos))
            {
                Debug.LogError($"No world position provided for levelIndex {data.levelIndex}.");
                pos = Vector3.zero;
            }

            nodes[data.levelIndex] = new LevelNode
            {
                levelIndex = data.levelIndex,
                worldPosition = pos
            };
        }

        // 2) Link edges (parents -> children)
        foreach (var data in graphData.nodes)
        {
            var node = nodes[data.levelIndex];

            foreach (int parentIndex in data.parentIndices)
            {
                if (!nodes.TryGetValue(parentIndex, out var parent))
                {
                    Debug.LogError($"GraphData error: level {data.levelIndex} references missing parent {parentIndex}.");
                    continue;
                }

                node.parents.Add(parent);
                parent.children.Add(node);
            }
        }

        return nodes;
    }
}
