using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile_Selector_Script : MonoBehaviour
{
    public GameObject cursor;

    public GameObject tileMapObj;
    Tilemap tileMap;
    public int[,] world;

    private SpriteRenderer spriteRenderer;
    public Sprite red_cursor;
    public Sprite yellow_cursor;
    public Sprite green_cursor;

    public GameObject player;

    Player playerData;

    Vector3 mousePosition;
    float zAxis = 10;

    void Start()
    {
        tileMap = tileMapObj.GetComponent<Tilemap>();
        world = tileMapObj.GetComponent<TileMap>().world;

        /*for (int x = 0; x < world.GetLength(0); x++)
        {
            for (int y = 0; y < world.GetLength(1); y++)
            {
                Debug.Log(world[x,y]);
            }
        }*/
        spriteRenderer = cursor.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = yellow_cursor;

        playerData = player.GetComponent<Player>();
    }

    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = zAxis;
        cursor.transform.position = new Vector3((RoundOffset(mousePosition.x)), (RoundOffset(mousePosition.y)), zAxis);

        if (Input.GetMouseButton(0))
        {
            Vector3Int coordinate = tileMap.WorldToCell(cursor.transform.position);
            TileBase tile = tileMap.GetTile(coordinate);
            if (CheckTile(tile) && InRange())
            {
                spriteRenderer.sprite = green_cursor;
                player.transform.position = cursor.transform.position;
            }
            else
            {
                spriteRenderer.sprite = red_cursor;
            }
        }
        else
        {
            spriteRenderer.sprite = yellow_cursor;
        }

    }

    public float RoundOffset(float a)
    {
        int b = Mathf.RoundToInt(a);
        if (b > a)
        {
            return b - 0.5f;
        }
        else
        {
            return b + 0.5f;
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

    public bool InRange()
    {
        if (cursor.transform.position == player.transform.position)
        {
            return false;
        }

        Vector3Int playerCell = tileMap.WorldToCell(player.transform.position);
        Vector3Int goalCell = tileMap.WorldToCell(cursor.transform.position);

        Queue<Vector3Int> previousTileList = new Queue<Vector3Int>();
        HashSet<Vector3Int> viewedTiles = new HashSet<Vector3Int>();
        return search(previousTileList, viewedTiles, playerCell, goalCell, playerData.moves);
    }

    public bool search(Queue<Vector3Int> previousTileList, HashSet<Vector3Int> viewedTiles, Vector3Int start, Vector3Int goal, int moves)
    {
        if (!CheckTile(tileMap.GetTile(start)) || moves == 0)
        {
            return false;
        }
        moves--;
        previousTileList.Enqueue(start);
        viewedTiles.Add(start);
        Vector3Int leftTile = new Vector3Int(start.x - 1, start.y, start.z);
        Vector3Int rightTile = new Vector3Int(start.x + 1, start.y, start.z);
        Vector3Int upTile = new Vector3Int(start.x, start.y + 1, start.z);
        Vector3Int downTile = new Vector3Int(start.x, start.y - 1, start.z);
        if (CheckTile(tileMap.GetTile(leftTile)) && !viewedTiles.Contains(leftTile) && (compareTiles(leftTile, goal)))
        {
            Debug.Log("it is left");
            return true;
        }
        if (CheckTile(tileMap.GetTile(rightTile)) && !viewedTiles.Contains(rightTile) && (compareTiles(rightTile, goal)))
        {
            Debug.Log("it is right");
            return true;
        }
        if (CheckTile(tileMap.GetTile(upTile)) && !viewedTiles.Contains(upTile) && (compareTiles(upTile, goal)))
        {
            Debug.Log("it is up");
            return true;
        }
        if (CheckTile(tileMap.GetTile(downTile)) && !viewedTiles.Contains(downTile) && (compareTiles(downTile, goal)))
        {
            Debug.Log("it is down");
            return true;
        }
        if (search(previousTileList, viewedTiles, leftTile, goal, moves) || 
            search(previousTileList, viewedTiles, rightTile, goal, moves) || 
            search(previousTileList, viewedTiles, upTile, goal, moves) ||
            search(previousTileList, viewedTiles, downTile, goal, moves))
        {
            return true;
        }
        previousTileList.Dequeue();
        return false;
    }

    public bool compareTiles(Vector3Int start, Vector3Int goal)
    {
        if (start.x >= 0 && start.x < world.GetLength(0) && start.y >= 0 && start.y < world.GetLength(1) && start.x == goal.x && start.y == goal.y)
        {
            return true;
        }
        return false;
    }

    /*public bool search(int index, Vector3 goal)
    {
        Vector3Int cursorCell = tileMap.WorldToCell(cursor.transform.position);

        LinkedListNode<TileStruct> ptr;

        ptr = world[index].First;
        Debug.Log(ptr.Value.position + " " + index);
        int steps = checkNeighbors(0, ptr, cursorCell);
        if (steps > 0)
        {
            Debug.Log(steps);
            return true;
        }
        else
        {
           return search(ptr.Next
        }
        return false;
    }*/

    /*public bool search(int index, Vector3 goal)
    {

    }*/

    /*public bool checkNeighbors(LinkedListNode<TileStruct> ptr, Vector3Int goalCell)
    {
        while (ptr.Next != null)
        {
            ptr = ptr.Next;
            if (ptr.Value.position == goalCell)
            {
                return true;
            }
        }
        return false;
    }*/

    /*public int checkNeighbors(int steps, LinkedListNode<TileStruct> ptr, Vector3Int goalCell)
    {
        Queue<LinkedListNode<TileStruct>> Q = new Queue<LinkedListNode<TileStruct>>();
        HashSet<LinkedListNode<TileStruct>> S = new HashSet<LinkedListNode<TileStruct>>();
        Q.Enqueue(ptr);
        S.Add(ptr);
        while (Q.Count > 0)
        {
            LinkedListNode<TileStruct> e = Q.Dequeue();
            
            //Debug.Log("Hi sir, e is " + e.Value.position + " Number of steps taken: " + steps);
            steps++;
            if (e.Value.position == goalCell)
            {
                return steps;
            }

            if (ptr.Next != null)
            {
                ptr = ptr.Next;
                if (!S.Contains(ptr))
                {
                    Q.Enqueue(ptr);
                    S.Add(ptr);
                }
            }

        }
        //Debug.Log("steps count: " + steps);
        return steps;
    }*/

}