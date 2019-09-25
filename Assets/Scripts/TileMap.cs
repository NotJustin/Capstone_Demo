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
    public GameObject cursor;
    public LinkedList<TileStruct>[] world;
    Tilemap tileMap;
    BoundsInt cellBounds;
    Vector3Int size;
    int zAxis = 0;

    void Awake()
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
                Vector3 coordinateWorld = new Vector3(x, y, zAxis);
                Vector3Int coordinateCell = tileMap.WorldToCell(coordinateWorld);
                TileBase tile = tileMap.GetTile(coordinateCell);
                if (CheckTile(tile))
                {
                    TileStruct newStruct = new TileStruct(tile.name, coordinateCell);
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

        /*LinkedListNode<TileStruct> ptr;
        Debug.Log(world.Length);
        for (int i = 0; i < world.Length; i++)
        {
            if (world[i] != null)
            {
                ptr = world[i].First;
                Debug.Log("Floor found at: " + ptr.Value.position);
                while (ptr.Next != null)
                {
                    ptr = ptr.Next;
                    Debug.Log("Neighbor " + i + " at position: " + ptr.Value.position);
                }
            }
        }*/
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