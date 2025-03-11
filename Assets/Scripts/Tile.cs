using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    Grass = 15,
    Tree,
    Hill,
    Mountain,
    Town,
    Castle,
    Dungeon,
}


public enum Sides
{
    Down,
    Right,
    Left,
    Up,
    DR,
    DL,
    UR,
    UL
}

public class Tile
{
    public int id = 0;
    public Tile[] neighbors = new Tile[8];

    public int autoTileId = 0;
    public int fogTileId = 15;
    public Tile previous = null;

    public bool revealed = false;

#if true
    public float Weight => (TileTypes)autoTileId switch
    {
        TileTypes.Tree => 5,
        TileTypes.Hill => 15,
        TileTypes.Mountain => Mathf.Infinity,
        TileTypes.Dungeon => 80,
        _ => 1,
    };
#else
    public static readonly float[] tableWeight =
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            4,9,Mathf.Infinity,1,1,1
    };
    public int Weight
    {
        get
        {
            if (autoTileId < 0)
            {
                return Mathf.Infinity;
            }

            return tableWeight[autoTileId];
        }
    }
#endif


    public void SetNeighbor(Sides side, Tile neighbor)
    {
        neighbors[(int)side] = neighbor;
    }

    public void UpdateAutoTileId()
    {
        //DRLU
        //autoTileId = 0;
        //for (int i = 0; i < neighbors.Length; ++i)
        //{
        //    if (neighbors[i] != null)
        //    {
        //        autoTileId |= 1 << i;
        //    }
        //}

        autoTileId = 0;
        for (int i = 0; i < 4; ++i)
        {
            autoTileId = autoTileId << 1;
            if (neighbors[i] != null)
            {
                ++autoTileId;
            }
        }
    }

    public void ClearNeighbor()
    {
        for (int i = 0; i < neighbors.Length; ++i)
        {
            if (neighbors[i] != null)
            {
                neighbors[i].RemoveNeighbor(this);
                neighbors[i] = null;
            }
        }
        UpdateAutoTileId();
    }

    private void RemoveNeighbor(Tile tile)
    {
        for (int i = 0; i < neighbors.Length; ++i)
        {
            if (neighbors[i] == tile)
            {
                neighbors[i] = null;
            }
        }
        UpdateAutoTileId();
    }
}
