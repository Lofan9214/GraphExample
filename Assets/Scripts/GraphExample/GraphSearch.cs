using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SearchMethod
{
    DFS,
    RecursiveDFS,
    BFS,
    PathFindBFS,
    PathFindDijkstra,
    PathFindAStar
}

public class GraphSearch
{
    private Graph graph;
    public List<Node> path = new List<Node>();

    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    public void Search(SearchMethod method, Node start, Node goal = null)
    {
        switch (method)
        {
            case SearchMethod.DFS:
                DFS(start);
                break;
            case SearchMethod.RecursiveDFS:
                RecursiveDFS(start);
                break;
            case SearchMethod.BFS:
                BFS(start);
                break;
            case SearchMethod.PathFindBFS:
                PathFindingBFS(start, goal);
                break;
            case SearchMethod.PathFindDijkstra:
                PathFindingDijkstra(start, goal);
                break;
            case SearchMethod.PathFindAStar:
                PathFindingAstar(start, goal);
                break;
        }
    }

    public void DFS(Node node)
    {
        path.Clear();
        var visited = new HashSet<Node>();
        var stack = new Stack<Node>();

        stack.Push(node);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            path.Add(currentNode);
            visited.Add(currentNode);

            foreach (var adjecent in currentNode.adjacents)
            {
                if (!adjecent.CanVisit || visited.Contains(adjecent) || stack.Contains(adjecent))
                {
                    continue;
                }

                stack.Push(adjecent);
            }
        }
    }

    public void RecursiveDFS(Node node)
    {
        path.Clear();
        var visited = new HashSet<Node>();

        RecursiveDFS(node, visited);
    }

    private void RecursiveDFS(Node currentNode, HashSet<Node> visited)
    {
        path.Add(currentNode);

        foreach (var adjecent in currentNode.adjacents)
        {
            if (!adjecent.CanVisit || path.Contains(adjecent))
            {
                continue;
            }

            RecursiveDFS(adjecent, visited);
        }
        return;
    }

    public void BFS(Node node)
    {
        path.Clear();

        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(node);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            path.Add(currentNode);
            visited.Add(currentNode);

            foreach (var adjecent in currentNode.adjacents)
            {
                if (!adjecent.CanVisit || visited.Contains(adjecent) || queue.Contains(adjecent))
                {
                    continue;
                }
                queue.Enqueue(adjecent);
            }
        }
    }


    public void PathFinding(Node start, Node goal)
    {
        path.Clear();

        var dict = new Dictionary<Node, int>();
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(start);
        dict.Add(start, 0);

        Node lastNode = null;

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            visited.Add(currentNode);

            if (currentNode == goal)
            {
                lastNode = currentNode;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent) || queue.Contains(adjacent))
                {
                    continue;
                }
                dict.Add(adjacent, dict[currentNode] + currentNode.weight);
                queue.Enqueue(adjacent);
            }
        }

        int distance = dict[lastNode];

        while (distance > 0)
        {
            foreach (var adjacent in lastNode.adjacents)
            {
                if (dict.ContainsKey(adjacent) && dict[adjacent] < distance)
                {
                    path.Insert(0, adjacent);
                    distance = dict[adjacent];
                    lastNode = adjacent;
                }
            }
        }
    }

    public void Dijkstra(Node start, Node goal)
    {
        path.Clear();

        Dictionary<Node, int> distances = new Dictionary<Node, int>();
        PriorityQueue<Node, int> queue = new PriorityQueue<Node, int>();
        Node lastNode = null;

        foreach (var node in graph.nodes)
        {
            distances.Add(node, int.MaxValue);
        }

        distances[start] = 0;

        queue.Enqueue(start, 0);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            if (currentNode == goal)
            {
                lastNode = currentNode;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                int nextDistance = distances[currentNode] + adjacent.weight;

                if (distances[adjacent] > nextDistance)
                {
                    distances[adjacent] = nextDistance;
                    queue.Enqueue(adjacent, nextDistance);
                }
            }
        }

        while (distances[lastNode] > 0f)
        {
            path.Insert(0, lastNode);

            float distance = distances[lastNode];
            Node minNode = null;
            foreach (var adjacent in lastNode.adjacents)
            {
                if (distance > distances[adjacent])
                {
                    distance = distances[adjacent];
                    minNode = adjacent;
                }
            }
            if (distance != distances[lastNode])
            {
                lastNode = minNode;
            }
        }
    }

    public bool PathFindingBFS(Node start, Node goal)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();
        bool found = false;

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            if (currentNode == goal)
            {
                found = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjecent in currentNode.adjacents)
            {
                if (!adjecent.CanVisit || visited.Contains(adjecent) || queue.Contains(adjecent))
                {
                    continue;
                }
                queue.Enqueue(adjecent);
                adjecent.previous = currentNode;
            }
        }

        if (!found)
        {
            return false;
        }

        Node step = goal;

        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return true;
    }

    public bool PathFindingDijkstra(Node start, Node goal)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var pQueue = new PriorityQueue<Node, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        int[] distances = new int[graph.nodes.Length];
        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
        }
        distances[start.id] = 0;

        pQueue.Enqueue(start, distances[start.id]);

        bool found = false;

        while (pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();

            if (visited.Contains(currentNode))
            {
                continue;
            }

            if (currentNode == goal)
            {
                found = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit)
                {
                    continue;
                }
                int newDistance = distances[currentNode.id] + adjacent.weight;
                if (newDistance < distances[adjacent.id])
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;
                    pQueue.Enqueue(adjacent, distances[adjacent.id]);
                }
            }
        }

        if (!found)
        {
            return false;
        }

        Node step = goal;

        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return true;
    }

    private int Heuristic(Node a, Node b)
    {
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool PathFindingAstar(Node start, Node goal)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var pQueue = new PriorityQueue<Node, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        int[] distances = new int[graph.nodes.Length];
        int[] scores = new int[graph.nodes.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
            scores[i] = int.MaxValue;
        }
        distances[start.id] = 0;
        scores[start.id] = Heuristic(start, goal);

        pQueue.Enqueue(start, distances[start.id]);

        bool found = false;

        while (pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();

            if (visited.Contains(currentNode))
            {
                continue;
            }

            if (currentNode == goal)
            {
                found = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit)
                {
                    continue;
                }
                int newDistance = distances[currentNode.id] + adjacent.weight;
                if (newDistance < distances[adjacent.id])
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = newDistance + Heuristic(adjacent, goal);
                    adjacent.previous = currentNode;
                    pQueue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        if (!found)
        {
            return false;
        }

        Node step = goal;

        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return true;
    }
}
