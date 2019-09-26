using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile_Selector_Script : MonoBehaviour
{
    public GameObject cursor;

    public GameObject tileMapObj;
    Tilemap tileMap;
    //public LinkedList<TileStruct>[] world;
    public int[,] world;

    private SpriteRenderer spriteRenderer;
    public Sprite red_cursor;
    public Sprite yellow_cursor;
    public Sprite green_cursor;

    public GameObject player;

    PlayerMove playerData;

    Vector3 mousePosition;
    float zAxis = 10;

    void Start()
    {
        tileMap = tileMapObj.GetComponent<Tilemap>();
        world = tileMapObj.GetComponent<TileMap>().world;

        for (int x = 0; x < world.GetLength(0); x++)
        {
            for (int y = 0; y < world.GetLength(1); y++)
            {
                Debug.Log(world[x,y]);
            }
        }

        spriteRenderer = cursor.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = yellow_cursor;

        playerData = player.GetComponent<PlayerMove>();
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
            if (CheckTile(tile))
            {
                player.transform.position = cursor.transform.position;
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
        if (tile != null && tile.name.Contains("floor") && InRange())
        {
            spriteRenderer.sprite = green_cursor;
            return true;
        }
        spriteRenderer.sprite = red_cursor;
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

        //return search(playerCell, goalCell);
        return true;
    }

    /*public bool search(Vector3Int start, Vector3Int goal)
    {
        int playerCellIndex = -1;

        LinkedList<LinkedListNode<TileStruct>> ptr;

        for (int i = 0; i < world.Length; i++)
        {
            ptr = world[i];
            if (ptr != null && playerCell == ptr.First.Value.position)
            {
            
            }
        }
    }*/

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