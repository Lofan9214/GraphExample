using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map
{
    private static readonly float sqrt2 = Mathf.Sqrt(2f);
    public Tile[] tiles;
    public int columns;
    public int rows;
    public Tile playerStartTile;
    public Tile castleTile;
    public List<Tile> path;

    public Tile[] CoastTiles => tiles.Where(p => p.autoTileId > (int)TileTypes.Empty && p.autoTileId < (int)TileTypes.Grass).ToArray();

    public Tile[] LandTiles => tiles.Where(p => p.autoTileId >= (int)TileTypes.Grass).ToArray();

    public Tile[] TownTiles => tiles.Where(p => p.autoTileId == (int)TileTypes.Town).ToArray();

    public Map()
    {
        path = new List<Tile>();
    }

    public void NewMap(int width, int height)
    {
        columns = width;
        rows = height;

        tiles = new Tile[columns * rows];

        for (int i = 0; i < tiles.Length; ++i)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        for (int j = 0; j < rows; ++j)
        {
            for (int i = 0; i < columns; ++i)
            {
                int index = j * columns + i;
                bool[] dirs = new bool[4];

                dirs[(int)Sides.Up] = j > 0;
                dirs[(int)Sides.Down] = j + 1 < rows;
                dirs[(int)Sides.Left] = i > 0;
                dirs[(int)Sides.Right] = i + 1 < columns;

                if (dirs[(int)Sides.Up])
                {
                    tiles[index].SetNeighbor(Sides.Up, tiles[index - columns]);
                }
                if (dirs[(int)Sides.Down])
                {
                    tiles[index].SetNeighbor(Sides.Down, tiles[index + columns]);
                }
                if (dirs[(int)Sides.Left])
                {
                    tiles[index].SetNeighbor(Sides.Left, tiles[index - 1]);
                }
                if (dirs[(int)Sides.Right])
                {
                    tiles[index].SetNeighbor(Sides.Right, tiles[index + 1]);
                }
                if (dirs[(int)Sides.Up] && dirs[(int)Sides.Left])
                {
                    tiles[index].SetNeighbor(Sides.UL, tiles[index - columns - 1]);
                }
                if (dirs[(int)Sides.Up] && dirs[(int)Sides.Right])
                {
                    tiles[index].SetNeighbor(Sides.UR, tiles[index - columns + 1]);
                }
                if (dirs[(int)Sides.Down] && dirs[(int)Sides.Left])
                {
                    tiles[index].SetNeighbor(Sides.DL, tiles[index + columns - 1]);
                }
                if (dirs[(int)Sides.Down] && dirs[(int)Sides.Right])
                {
                    tiles[index].SetNeighbor(Sides.DR, tiles[index + columns + 1]);
                }

                //tiles[index].UpdateAutoTileId();

                //Debug.Log(tiles[index].autoTileId);
            }
        }

        for (int i = 0; i < tiles.Length; ++i)
        {
            tiles[i].UpdateAutoTileId();
        }
    }

    public bool CreateIsland
        (int erodeIterations,
        float erodePercent,
        float lakePercent,
        float treePercent,
        float hillPercent,
        float mountainPercent,
        float townPercent,
        float dungeonPercent)
    {
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        for (int i = 0; i < erodeIterations; ++i)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hill);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountain);
        DecorateTiles(LandTiles, dungeonPercent, TileTypes.Dungeon);

        while (TownTiles.Length < 2)
        {
            DecorateTiles(LandTiles, townPercent, TileTypes.Town);
        }

        SetCastlePlayer();
#if false
        var landTiles = LandTiles;
        castleTile = landTiles[Random.Range(0, landTiles.Length)];
        castleTile.autoTileId = (int)TileTypes.Castle;


        var townTiles = TownTiles;
        playerStartTile = townTiles[Random.Range(0, townTiles.Length)];

        var path = PathFindingBFS(castleTile, playerStartTile);

#endif

        return path.Count > 0;
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);
        int total = Mathf.FloorToInt(tiles.Length * percent);

        for (int i = 0; i < total; ++i)
        {
            switch (tileType)
            {
                case TileTypes.Empty:
                    tiles[i].ClearNeighbor();
                    break;
            }

            tiles[i].autoTileId = (int)tileType;
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i >= 0; --i)
        {
            int rand = Random.Range(0, i + 1);

            Tile tile = tiles[i];
            tiles[i] = tiles[rand];
            tiles[rand] = tile;
        }
    }

    public void SetCastlePlayer()
    {
        Tile[] towns = TownTiles;

        bool notSet = true;

        while (notSet)
        {
            ShuffleTiles(towns);
            notSet = !PathFind(towns[0], towns[1]);
        }
        playerStartTile = towns[0];

        towns[1].autoTileId = (int)TileTypes.Castle;
    }

    public bool IsPathAvailableDijkstra(Tile start, Tile goal)
    {
        path.Clear();

        var parent = new Dictionary<int, int>();
        var visited = new HashSet<Tile>();
        var pQueue = new PriorityQueue<Tile, float>();
        var distances = new float[tiles.Length];
        var pathFound = false;

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = Mathf.Infinity;
        }
        distances[start.id] = 0;
        parent[start.id] = -1;

        pQueue.Enqueue(start, distances[start.id]);

        while (pQueue.Count > 0)
        {
            var currentTile = pQueue.Dequeue();

            if (visited.Contains(currentTile))
            {
                continue;
            }

            if (currentTile == goal)
            {
                return true;
            }

            visited.Add(currentTile);

            foreach (var neighbor in currentTile.neighbors)
            {
                if (neighbor == null)
                {
                    continue;
                }
                float newDistance = distances[currentTile.id] + neighbor.Weight;
                if (newDistance < distances[neighbor.id])
                {
                    distances[neighbor.id] = newDistance;
                    parent[neighbor.id] = currentTile.id;
                    pQueue.Enqueue(neighbor, distances[neighbor.id]);
                }
            }
        }

        if (!pathFound)
        {
            return false;
        }

        int stepId = goal.id;

        while (stepId >= 0)
        {
            path.Add(tiles[stepId]);
            stepId = parent[stepId];
        }

        return true;
    }

    private float Heuristic(Tile a, Tile b)
    {
        int ax = a.id % columns;
        int ay = a.id / columns;

        int bx = b.id % columns;
        int by = b.id / columns;

        int dx = Mathf.Abs(ax - bx);
        int dy = Mathf.Abs(ay - by);

        return Mathf.Min(dx, dy) * sqrt2 + Mathf.Abs(dx - dy);
    }

    public bool PathFind(Tile start, Tile goal)
    {
        return PathFindAstar(start, goal, path);
    }

    public bool PathFind(Tile start, Tile goal, out List<Tile> path)
    {
        path = new List<Tile>();
        return PathFindAstar(start, goal, path);
    }

    public bool PathFind(int start, int goal, out List<Tile> path)
    {
        return PathFind(tiles[start], tiles[goal], out path);
    }

    private bool PathFindAstar(Tile start, Tile goal, List<Tile> path)
    {
        path.Clear();
        ResetNodePrevious();

        var pQueue = new PriorityQueue<Tile, float>();

        float[] distances = new float[tiles.Length];
        float[] scores = new float[tiles.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = Mathf.Infinity;
            scores[i] = Mathf.Infinity;
        }

        distances[start.id] = 0;
        scores[start.id] = Heuristic(start, goal);

        pQueue.Enqueue(start, distances[start.id]);

        bool found = false;

        while (pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();

            if (currentNode == goal)
            {
                found = true;
                break;
            }

            bool[] dirs = new bool[4];

            dirs[(int)Sides.Up] = currentNode.neighbors[(int)Sides.Up] != null;
            dirs[(int)Sides.Down] = currentNode.neighbors[(int)Sides.Down] != null;
            dirs[(int)Sides.Left] = currentNode.neighbors[(int)Sides.Left] != null;
            dirs[(int)Sides.Right] = currentNode.neighbors[(int)Sides.Right] != null;

            for (int i = 0; i < currentNode.neighbors.Length; ++i)
            {
                var neighbor = currentNode.neighbors[i];
                if (neighbor == null || neighbor.Weight == Mathf.Infinity)
                {
                    continue;
                }
                float newDistance = Mathf.Infinity;
                switch ((Sides)i)
                {
                    case Sides.DR:
                        if (dirs[(int)Sides.Down] || dirs[(int)Sides.Right])
                        {
                            newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        }
                        break;
                    case Sides.DL:
                        if (dirs[(int)Sides.Down] || dirs[(int)Sides.Left])
                        {
                            newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        }
                        break;
                    case Sides.UR:
                        if (dirs[(int)Sides.Up] || dirs[(int)Sides.Right])
                        {
                            newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        }
                        break;
                    case Sides.UL:
                        if (dirs[(int)Sides.Up] || dirs[(int)Sides.Left])
                        {
                            newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        }
                        break;
                    default:
                        newDistance = distances[currentNode.id] + neighbor.Weight;
                        break;
                }

                if (newDistance < distances[neighbor.id])
                {
                    distances[neighbor.id] = newDistance;
                    scores[neighbor.id] = newDistance + Heuristic(neighbor, goal);
                    neighbor.previous = currentNode;
                    pQueue.Enqueue(neighbor, scores[neighbor.id]);
                }
            }
        }

        if (!found)
        {
            return false;
        }

        Tile step = goal;

        while (step != null)
        {
            path.Add(step);

            step = step.previous;
        }
        path.Reverse();

        return true;
    }

    public void ResetNodePrevious()
    {
        foreach (Tile node in tiles)
        {
            if (node != null)
            {
                node.previous = null;
            }
        }
    }

    public List<Tile> PathFindingBFS(Tile start, Tile goal)
    {
        var path = new List<Tile>();
        ResetNodePrevious();

        var visited = new HashSet<Tile>();
        var queue = new Queue<Tile>();
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

            foreach (var adjacent in currentNode.neighbors)
            {
                if (adjacent == null || visited.Contains(adjacent) || queue.Contains(adjacent))
                {
                    continue;
                }
                queue.Enqueue(adjacent);
                adjacent.previous = currentNode;
            }
        }

        if (!found)
        {
            return path;
        }

        Tile step = goal;

        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return path;
    }

    public List<Tile> PathFindingAstar(Tile start, Tile goal)
    {
        var path = new List<Tile>();
        ResetNodePrevious();

        var visited = new HashSet<Tile>();
        var pQueue = new PriorityQueue<Tile, float>();

        float[] distances = new float[tiles.Length];
        float[] scores = new float[tiles.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = Mathf.Infinity;
            scores[i] = Mathf.Infinity;
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

            for (int i = 0; i < currentNode.neighbors.Length; ++i)
            {
                var neighbor = currentNode.neighbors[i];
                if (neighbor == null || neighbor.Weight == Mathf.Infinity)
                {
                    continue;
                }
                float newDistance = Mathf.Infinity;
                switch ((Sides)i)
                {
                    case Sides.Down:
                    case Sides.Right:
                    case Sides.Left:
                    case Sides.Up:
                        newDistance = distances[currentNode.id] + neighbor.Weight;
                        break;
                    case Sides.DR:
                        if (currentNode.neighbors[(int)Sides.Down] == null
                            && currentNode.neighbors[(int)Sides.Right] == null)
                        {
                            continue;
                        }
                        newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        break;
                    case Sides.DL:
                        if (currentNode.neighbors[(int)Sides.Down] == null
                            && currentNode.neighbors[(int)Sides.Left] == null)
                        {
                            continue;
                        }
                        newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        break;
                    case Sides.UR:
                        if (currentNode.neighbors[(int)Sides.Up] == null
                            && currentNode.neighbors[(int)Sides.Right] == null)
                        {
                            continue;
                        }
                        newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        break;
                    case Sides.UL:
                        if (currentNode.neighbors[(int)Sides.Up] == null
                            && currentNode.neighbors[(int)Sides.Left] == null)
                        {
                            continue;
                        }
                        newDistance = distances[currentNode.id] + sqrt2 * neighbor.Weight;
                        break;
                }
                if (newDistance < distances[neighbor.id])
                {
                    distances[neighbor.id] = newDistance;
                    scores[neighbor.id] = newDistance + Heuristic(neighbor, goal);
                    neighbor.previous = currentNode;
                    pQueue.Enqueue(neighbor, scores[neighbor.id]);
                }
            }
        }

        if (!found)
        {
            return path;
        }

        Tile step = goal;

        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return path;
    }

    public List<int> RevealFog(int center, int radius)
    {
        var tileIndexes = new List<int>();
        int x = center % columns;
        int y = center / columns;

        for (int j = y - radius; j <= y + radius; ++j)
        {
            if (j < 0 || j >= rows)
            {
                continue;
            }
            for (int i = x - radius; i <= x + radius; ++i)
            {
                if (i < 0 || i >= columns)
                {
                    continue;
                }
                int index = j * columns + i;
                tiles[index].revealed = true;
            }
        }
        for (int j = y - radius - 1; j <= y + radius + 1; ++j)
        {
            if (j < 0 || j >= rows)
            {
                continue;
            }
            for (int i = x - radius - 1; i <= x + radius + 1; ++i)
            {
                if (i < 0 || i >= columns)
                {
                    continue;
                }
                UpdateFog(i, j);
                tileIndexes.Add(j * columns + i);
            }
        }
        return tileIndexes;
    }

    private void UpdateFog(int i, int j)
    {
        int index = j * columns + i;
        int fogTileId = 0;
        if (j + 1 >= rows || !tiles[index + columns].revealed)
        {
            ++fogTileId;
        }
        fogTileId = fogTileId << 1;
        if (i + 1 >= columns || !tiles[index + 1].revealed)
        {
            ++fogTileId;
        }
        fogTileId = fogTileId << 1;
        if (i <= 0 || !tiles[index - 1].revealed)
        {
            ++fogTileId;
        }
        fogTileId = fogTileId << 1;
        if (j <= 0 || !tiles[index - columns].revealed)
        {
            ++fogTileId;
        }
        tiles[index].fogTileId = fogTileId;
    }

    private void UpdateFog(int index)
    {
        UpdateFog(index % columns, index / columns);
    }

    private void CheckDiagonal()
    {

    }
}
