using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileStruct
{
    public string name;
    public Vector3Int position;

    public TileStruct(string Name, Vector3Int Position)
    {
        name = Name;
        position = Position;
    }
}

public class TileMap : MonoBehaviour
{
    //public int mapWidth;
    //public int mapHeight;
    public GameObject cursor;
    public LinkedList<TileStruct>[] world;
    Tilemap tileMap;
    BoundsInt cellBounds;
    Vector3Int size;
    int zAxis = 0;

    void Start()
    {
        tileMap = GetComponent<Tilemap>();
        tileMap.CompressBounds();
        cellBounds = tileMap.cellBounds;
        world = new LinkedList<TileStruct>[tileMap.cellBounds.size.x * tileMap.cellBounds.size.y];
        int tileCount = 0;
        for (int x = 0; x < cellBounds.size.x; x++)
        {
            for (int y = 0; y < cellBounds.size.y; y++)
            {
                Vector3Int coordinate = new Vector3Int(x, y, zAxis);
                TileBase tile = tileMap.GetTile(coordinate);
                Debug.Log(tile);
                if (CheckTile(tile))
                {
                    Debug.Log("Found tile at: " + coordinate);
                    TileStruct newStruct = new TileStruct(tile.name, coordinate);
                    world[tileCount] = new LinkedList<TileStruct>();
                    world[tileCount].AddLast(newStruct);
                    AddNeighbors(tileCount, x, y);
                    ++tileCount;
                }
                else
                {
                    world[tileCount] = null;
                    ++tileCount;
                }
            }
        }

        LinkedListNode<TileStruct> ptr;
        Debug.Log(world.Length);
        for (int i = 0; i < world.Length; i++)
        {
            Debug.Log("i = " + i);
            if (world[i] != null)
            {
                Debug.Log("Valid start found at: " + i);
                ptr = world[i].First;
                while (ptr != null)
                {
                    Debug.Log("List: " + i + " tile position: " + ptr.Value.position);
                    ptr = ptr.Next;
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

    public void AddNeighbors(int tileCount, int x, int y)
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
    }
}
