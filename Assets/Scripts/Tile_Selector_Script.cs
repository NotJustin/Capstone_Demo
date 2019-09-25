using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile_Selector_Script : MonoBehaviour
{
    public GameObject cursor;

    public GameObject tileMapObj;
    Tilemap tileMap;

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
            Debug.Log(coordinate);
            TileBase tile = tileMap.GetTile(coordinate);
            CheckTile(tile);
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
        if (tile != null && InRange())
        {
            if (tile.name.Contains("floor"))
            {
                spriteRenderer.sprite = green_cursor;
                return true;
            }
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
        if (
            cursor.transform.position.x > (player.transform.position.x + playerData.moves) ||
            cursor.transform.position.x < (player.transform.position.x - playerData.moves) ||
            cursor.transform.position.y > (player.transform.position.y + playerData.moves) ||
            cursor.transform.position.y < (player.transform.position.y - playerData.moves))
        {
            return false;
        }
        else
        {
            return true;
        }
    }



}
