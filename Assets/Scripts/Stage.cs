using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public Sprite[] islandSprites;
    public Sprite[] fowSprites;

    public int mapWidth = 20;
    public int mapHeight = 20;
    public Vector2 tileSize = new Vector2(16, 16);

    public Map map;

    public GameObject tilePrefab;
    public Player playerPrefab;
    public List<GameObject> tileObjs;

    private Player player;

    private LineRenderer lineRenderer;

    public int erodeIterations = 3;
    [Range(0f, 1f)]
    public float erodePercent = 0.1f;
    [Range(0f, 1f)]
    public float lakePercent = 0.1f;
    [Range(0f, 1f)]
    public float treePercent = 0.1f;
    [Range(0f, 1f)]
    public float hillPercent = 0.1f;
    [Range(0f, 1f)]
    public float mountainPercent = 0.1f;
    [Range(0f, 1f)]
    public float townPercent = 0.1f;
    [Range(0f, 1f)]
    public float dungeonPercent = 0.1f;

    public int fovRadius = 2;

    private void Awake()
    {
        map = new Map();
        tileObjs = new List<GameObject>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        ResetMap();
    }

    [ContextMenu("Reset Map")]
    public void ResetMap()
    {
        if (player != null)
            Destroy(player.gameObject);

        MakeMap();

        CreateGrid();

        CreatePlayer();
    }

    private void DrawPath(List<Vector3> path)
    {
        lineRenderer.positionCount = path.Count;
        for (int i = 0, j = lineRenderer.positionCount - 1; i < lineRenderer.positionCount; ++i, --j)
        {
            lineRenderer.SetPosition(i, path[j]);
        }
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    int index = ScreenPosToTileId(Input.mousePosition);
        //    Vector3 tilePos = GetTilePos(index);
        //
        //    Debug.Log($"Index: {index}, Name: {tileObjs[index].name} TilePos: {tilePos.x}, {tilePos.y}");
        //}

        if (Input.GetMouseButtonDown(0))
        {
            MovePlayer(Input.mousePosition);
        }
    }

    public void MakeMap()
    {
        map.NewMap(mapWidth, mapHeight);

        map.CreateIsland(
            erodeIterations,
            erodePercent,
            lakePercent,
            treePercent,
            hillPercent,
            mountainPercent,
            townPercent,
            dungeonPercent);

        Debug.Log("¸Ê »ý¼º");
    }

    public void CreateGrid()
    {
        foreach (var go in tileObjs)
        {
            Destroy(go);
        }
        tileObjs.Clear();

        var startPos = transform.position;
        var currentPos = startPos;
        currentPos.x += tileSize.x * 0.5f;
        currentPos.y -= tileSize.y * 0.5f;

        for (int j = 0; j < map.rows; ++j)
        {
            for (int i = 0; i < map.columns; ++i)
            {
                int tileId = j * map.columns + i;

                var newGo = Instantiate(tilePrefab, currentPos, Quaternion.identity, transform);

                newGo.name = $"Tile {i}, {j}";
                tileObjs.Add(newGo);

                currentPos.x += tileSize.x;

                DecorateTile(tileId);
            }

            currentPos.x = startPos.x + tileSize.x * 0.5f;
            currentPos.y -= tileSize.y;
        }
    }

    private void CreatePlayer()
    {
        var position = GetTilePos(map.playerStartTile.id);
        player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.Moved.AddListener(PlayerMoved);
        RevealTheWorld(map.playerStartTile.id);
    }

    public int ScreenPosToTileId(Vector3 screenPos)
    {
        return WorldPosToTileId(Camera.main.ScreenToWorldPoint(screenPos));
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        var localPos = worldPos - transform.position;

        int x = Mathf.FloorToInt(localPos.x / tileSize.x);
        int y = Mathf.FloorToInt(-localPos.y / tileSize.y);

        x = Mathf.Clamp(x, 0, map.columns - 1);
        y = Mathf.Clamp(y, 0, map.rows - 1);
        return y * map.columns + x;
    }

    public Vector3 GetTilePos(int x, int y)
    {
        return GetTilePos(y * map.columns + x);
    }

    public Vector3 GetTilePos(int tileId)
    {
        return tileObjs[tileId].transform.position;
    }

    public void DecorateTile(int tileId)
    {
        var spriteRenderer = tileObjs[tileId].GetComponent<SpriteRenderer>();

        if (!map.tiles[tileId].revealed)
        {
            spriteRenderer.sprite = fowSprites[map.tiles[tileId].fogTileId];
            return;
        }

        if (map.tiles[tileId].autoTileId == (int)TileTypes.Empty)
        {
            spriteRenderer.sprite = null;
        }
        else
        {
            spriteRenderer.sprite = islandSprites[map.tiles[tileId].autoTileId];
        }
    }

    public void PlayerMoved(Vector3 worldStartPos, Vector3 worldTargetPosition)
    {
        if (lineRenderer.positionCount > 0)
        {
            var lastPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            if (Vector3.SqrMagnitude(lastPos - worldStartPos) < Vector3.kEpsilonNormalSqrt)
            {
                --lineRenderer.positionCount;
            }
        }
        RevealTheWorld(WorldPosToTileId(worldTargetPosition));
    }

    public void RevealTheWorld(int index)
    {
        var tileIndexes = map.RevealFog(index, fovRadius);

        foreach (var tileIndex in tileIndexes)
        {
            DecorateTile(tileIndex);
        }
    }

    public void MovePlayer(Vector3 mousePosition)
    {
        int targetIndex = ScreenPosToTileId(mousePosition);
        if (!map.tiles[targetIndex].revealed)
        {
            return;
        }
        
        int playeridx;
        if (player.MoveStatus == Player.Status.Standing)
            playeridx = WorldPosToTileId(player.transform.position);
        else
            playeridx = WorldPosToTileId(player.TargetPos);

        if (!map.PathFind(playeridx, targetIndex, out List<Tile> path))
        {
            return;
        }

        List<Vector3> posPath = new List<Vector3>();

        foreach (var tile in path)
        {
            posPath.Add(GetTilePos(tile.id));
        }

        player.SetRoute(posPath);

        DrawPath(posPath);
    }
}
