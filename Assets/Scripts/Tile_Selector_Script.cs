using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile_Selector_Script : MonoBehaviour
{
    public GameObject cursor;

    public GameObject tileMapObj;
    Tilemap tileMap;
    public LinkedList<TileStruct>[] world;

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

        //LinkedListNode<TileStruct> ptr;
        //Debug.Log(world.Length);
        /*for (int i = 0; i < world.Length; i++)
        {
            if (world[i] != null)
            {
                ptr = world[i].First;
                //Debug.Log("Floor found at: " + ptr.Value.position);
                while (ptr.Next != null)
                {
                    ptr = ptr.Next;
                    //Debug.Log("Neighbor " + i + " at position: " + ptr.Value.position);
                }
            }
        }*/

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
            //Debug.Log(coordinate);
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
        for (int i = 0; i < world.Length; i++)
        {
            if (world[i] != null && playerCell == world[i].First.Value.position)
            {
                Debug.Log("found player tile");
                break;
            }
        }


        return true;
    }



}