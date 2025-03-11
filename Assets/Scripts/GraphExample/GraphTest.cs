using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    public UiNode nodePrefab;

    public SearchMethod searchMethod;

    public List<UiNode> uiNodes;

    public int start;
    public int end;

    private void Start()
    {
        Pathfind();
    }

    private void InitUiNodes(Graph graph)
    {
        foreach(var uiNode in uiNodes)
        {
            Destroy(uiNode.gameObject);
        }
        uiNodes.Clear();


        foreach (var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab, transform);

            uiNode.SetNode(node);
            uiNodes.Add(uiNode);
        }
    }

    [ContextMenu("Path Find")]
    private void Pathfind()
    {
        int[,] map = new int[5, 5]
        {
            {1, -1, 1, 1, 1},
            {1, -1, 10, 5, 1},
            {1, -1, 10, 5, 1},
            {1, -1, 5, 1, 1},
            {1, 1, 1, 1, 1}
        };

        var graph = new Graph();

        graph.Init(map);
        InitUiNodes(graph);

        var search = new GraphSearch();

        search.Init(graph);

        search.Search(searchMethod, graph.nodes[start], graph.nodes[end]);

        for (int i = 0; i < search.path.Count; ++i)
        {
            var node = search.path[i];
            var color = Color.Lerp(Color.red, Color.green, (float)i / search.path.Count);
            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetString($"ID: {node.id}\nWeight: {node.weight}\nPath:{i}");
        }
    }
}
