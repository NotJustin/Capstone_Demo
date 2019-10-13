using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMap : MonoBehaviour
{

    /// This script takes information from the Tilemap component and maps it into a 2D array called world.
    /// Each index is 0 if it is a floor, or -1 if it is not.
    /// The row/column represents the X and Y coordinate of the cell in the world.
    /// It makes it easy to check neighbors - left is x-1, above is y+1, below is y-1, right is x+1.
    /// The coordinate grid starts in the bottom left corner of the map.
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
                    //Debug.Log(tile.name);
                    if (tile.name.Contains("spawn"))
                    {
                        //Debug.Log("test");
                        world[x, y] = 2;
                    }
                    else if (tile.name.Contains("floor") || tile.name.Contains("wire"))
                    {
                        world[x, y] = 0;
                    }
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
        if (tile != null)
        {
            return true;
        }
        return false;
    }
}