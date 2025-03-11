using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    public int rows = 0;
    public int cols = 0;

    public Node[] nodes;

    public void ResetNodePrevious()
    {
        foreach (Node node in nodes)
        {
            if (node != null)
            {
                node.previous = null;
            }
        }
    }

    public void Init(int[,] grid)
    {
        rows = grid.GetLength(0);
        cols = grid.GetLength(1);

        nodes = new Node[grid.Length];
        for (int i = 0; i < nodes.Length; ++i)
        {
            nodes[i] = new Node();
            nodes[i].id = i;
        }

        for (int j = 0; j < rows; ++j)
        {
            for (int i = 0; i < cols; ++i)
            {
                int index = j * cols + i;
                nodes[index].weight = grid[j, i];

                if (grid[j, i] < 0)
                {
                    continue;
                }

                if (j > 0 && grid[j - 1, i] >= 0)
                {
                    nodes[index].adjacents.Add(nodes[index - cols]);
                }
                if (j + 1 < rows && grid[j + 1, i] >= 0)
                {
                    nodes[index].adjacents.Add(nodes[index + cols]);
                }
                if (i > 0 && grid[j, i - 1] >= 0)
                {
                    nodes[index].adjacents.Add(nodes[index - 1]);
                }
                if (i + 1 < cols && grid[j, i + 1] >= 0)
                {
                    nodes[index].adjacents.Add(nodes[index + 1]);
                }
            }
        }
    }
}
