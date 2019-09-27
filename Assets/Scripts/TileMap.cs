using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMap : MonoBehaviour
{
    public GameObject cursor;
    public int[,] world;
    Tilemap tileMap;
    BoundsInt cellBounds;
    Vector3Int size;
    int zAxis = 0;

    void Awake()
    {
        tileMap = GetComponent<Tilemap>();
        tileMap.CompressBounds();
        cellBounds = tileMap.cellBounds;
        world = new int[tileMap.cellBounds.size.x, tileMap.cellBounds.size.y];
        for (int x = 0; x < cellBounds.size.x; x++)
        {
            for (int y = 0; y < cellBounds.size.y; y++)
            {
                Vector3Int position = new Vector3Int(x, y, zAxis);
                TileBase tile = tileMap.GetTile(position);
                if (CheckTile(tile))
                {
                    world[x, y] = 0;
                }
                else
                {
                    world[x,y] = -1;
                }
            }
        }
    }

    public bool CheckTile(TileBase tile)
    {
        if (tile != null && tile.name.Contains("floor"))
        {
            return true;
        }
        return false;
    }

    /*public void AddNeighbors(TileStruct tile, int x, int y)
    {
        Vector3Int leftCoordinate = new Vector3Int(x - 1, y, zAxis);
        Vector3Int upCoordinate = new Vector3Int(x, y - 1, zAxis);
        Vector3Int rightCoordinate = new Vector3Int(x + 1, y, zAxis);
        Vector3Int downCoordinate = new Vector3Int(x, y + 1, zAxis);
        TileBase tileLeft = tileMap.GetTile(leftCoordinate);
        TileBase tileUp = tileMap.GetTile(upCoordinate);
        TileBase tileRight = tileMap.GetTile(rightCoordinate);
        TileBase tileDown = tileMap.GetTile(downCoordinate);

        if (CheckTile(tileLeft))
        {
            TileStruct leftStruct = new TileStruct(tileLeft.name, leftCoordinate);
            world[tileCount].AddLast(leftStruct);
        }
        if (CheckTile(tileUp))
        {
            TileStruct upStruct = new TileStruct(tileUp.name, upCoordinate);
            world[tileCount].AddLast(upStruct);
        }
        if (CheckTile(tileRight))
        {
            TileStruct rightStruct = new TileStruct(tileRight.name, rightCoordinate);
            world[tileCount].AddLast(rightStruct);
        }
        if (CheckTile(tileDown))
        {
            TileStruct downStruct = new TileStruct(tileDown.name, downCoordinate);
            world[tileCount].AddLast(downStruct);
        }
    }*/
}